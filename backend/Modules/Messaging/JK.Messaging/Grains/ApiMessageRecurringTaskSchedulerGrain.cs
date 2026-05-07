using Microsoft.Extensions.Configuration;
using JK.Messaging.Contracts;
using JK.Messaging.Contracts.Enums;
using Cronos;
using JK.Messaging.Database;
using JK.Messaging.Database.Entities;
using JK.Messaging.Database.Repositories;
using JK.Messaging.Grains;
using JK.Messaging.Models;
using JK.Platform.Persistence.EfCore;
using Microsoft.Extensions.Logging;
using Orleans.Runtime;


namespace JK.Messaging.Grains;

public class ApiMessageRecurringTaskSchedulerGrain :
    Grain,
    IApiMessageRecurringTaskSchedulerGrain,
    IRemindable
{
    private const string SyncReminderName = "api-message-recurring-sync";
    private const string TaskReminderPrefix = "api-message-recurring-task:";

    private static readonly TimeSpan SyncPeriod = TimeSpan.FromHours(1);
    private static readonly TimeSpan FallbackReminderPeriod = TimeSpan.FromDays(30);

    private readonly ILogger<ApiMessageRecurringTaskSchedulerGrain> _logger;
    private readonly IUnitOfWork<MessagingDbContext> _unitOfWork;
    private readonly IConfiguration _configuration;

    public ApiMessageRecurringTaskSchedulerGrain(
        IUnitOfWorkFactory<MessagingDbContext> unitOfWorkFactory,
        ILogger<ApiMessageRecurringTaskSchedulerGrain> logger,
        IConfiguration configuration)
    {
        _unitOfWork = unitOfWorkFactory.Create();
        _logger = logger;
        _configuration = configuration;
    }

    public async Task EnsureSyncReminderAsync()
    {
        await this.RegisterOrUpdateReminder(
            reminderName: SyncReminderName,
            dueTime: TimeSpan.FromSeconds(10),
            period: SyncPeriod);

        await SyncRemindersAsync();
    }

    public async Task ReceiveReminder(string reminderName, TickStatus status)
    {
        if (reminderName == SyncReminderName)
        {
            await SyncRemindersAsync();
            return;
        }

        if (reminderName.StartsWith(TaskReminderPrefix))
        {
            var taskName = reminderName[TaskReminderPrefix.Length..];
            await ExecuteRecurringTaskAsync(taskName);
            return;
        }

        _logger.LogWarning("Unknown reminder received: {ReminderName}", reminderName);
    }

    public async Task SyncRemindersAsync()
    {
        var recurringTaskRepository =
            _unitOfWork.GetRepository<IApiMessageRecurringTaskRepository>();

        var enabledTasks = await recurringTaskRepository.GetEnabledAsync();

        foreach (var task in enabledTasks)
        {
            await EnsureTaskReminderAsync(task);
        }

        await RemoveDisabledOrDeletedTaskRemindersAsync(enabledTasks);

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ExecuteRecurringTaskAsync(string taskName)
    {
        var recurringTaskRepository =
            _unitOfWork.GetRepository<IApiMessageRecurringTaskRepository>();

        var task = await recurringTaskRepository.GetFirstOrDefaultAsync(taskName);

        if (task is null || !task.IsEnabled)
        {
            await UnregisterTaskReminderAsync(taskName);
            return;
        }

        await CreateApiMessageTasksAsync(task);

        var now = DateTime.UtcNow;

        task.LastRunAtUtc = now;
        task.NextRunAtUtc = GetNextOccurrenceUtc(task.CronExpression, now);
        task.UpdatedAtUtc = now;

        await recurringTaskRepository.UpdateAsync(task);
        await recurringTaskRepository.SaveChangesAsync();

        await _unitOfWork.SaveChangesAsync();

        await RegisterTaskReminderAsync(task);
    }

    private async Task EnsureTaskReminderAsync(ApiMessageRecurringTaskModel task)
    {
        var now = DateTime.UtcNow;

        if (task.NextRunAtUtc is null || task.NextRunAtUtc <= now)
        {
            task.NextRunAtUtc = GetNextOccurrenceUtc(task.CronExpression, now);
            task.UpdatedAtUtc = now;
        }

        await RegisterTaskReminderAsync(task);
    }

    private async Task RegisterTaskReminderAsync(ApiMessageRecurringTaskModel task)
    {
        if (task.NextRunAtUtc is null)
        {
            _logger.LogWarning(
                "Recurring task {TaskName} has no next run time.",
                task.TaskName);

            return;
        }

        var dueTime = task.NextRunAtUtc.Value - DateTime.UtcNow;

        if (dueTime < TimeSpan.Zero)
        {
            dueTime = TimeSpan.Zero;
        }

        await this.RegisterOrUpdateReminder(
            reminderName: GetTaskReminderName(task.TaskName),
            dueTime: dueTime,
            period: FallbackReminderPeriod);
    }

    private async Task RemoveDisabledOrDeletedTaskRemindersAsync(
        IReadOnlyCollection<ApiMessageRecurringTaskModel> enabledTasks)
    {
        var validReminderNames = enabledTasks
            .Select(x => GetTaskReminderName(x.TaskName))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var reminders = await this.GetReminders();

        foreach (var reminder in reminders)
        {
            if (!reminder.ReminderName.StartsWith(TaskReminderPrefix))
                continue;

            if (validReminderNames.Contains(reminder.ReminderName))
                continue;

            await this.UnregisterReminder(reminder);
        }
    }

    private async Task UnregisterTaskReminderAsync(string taskName)
    {
        var reminderName = GetTaskReminderName(taskName);

        var reminder = await this.GetReminder(reminderName);

        if (reminder is not null)
        {
            await this.UnregisterReminder(reminder);
        }
    }

    private async Task CreateApiMessageTasksAsync(ApiMessageRecurringTaskModel recurringTask)
    {
        var urls = _configuration.GetSection($"Messaging:RecurrentTasks:{recurringTask.TaskName}:Urls").Get<List<string>>();
        if (urls == null || urls.Count == 0)
        {
            return;
        }

        var repository = _unitOfWork.GetRepository<IApiMessageTaskRepository>();

        var apiMessageTask = new ApiMessageTaskEntity
        {
            Id = Guid.NewGuid().ToString(),
            TaskName = recurringTask.TaskName,
            State = ApiMessageStateEnum.Created,
            CreatedOn = DateTime.UtcNow
        };

        await repository.AddAsync(apiMessageTask);
        await _unitOfWork.SaveChangesAsync();

        var grain = GrainFactory.GetGrain<IApiMessageTaskGrain>(apiMessageTask.Id);
        await grain.Register(new RegisterApiMessageTaskCommand
        {
            Id = apiMessageTask.Id,
            TaskName = apiMessageTask.TaskName
        });
    }

    private static string GetTaskReminderName(string taskName)
    {
        return $"{TaskReminderPrefix}{taskName}";
    }

    private static DateTime? GetNextOccurrenceUtc(
        string cronExpression,
        DateTime fromUtc)
    {
        var expression = CronExpression.Parse(cronExpression);

        return expression.GetNextOccurrence(
            fromUtc,
            TimeZoneInfo.Utc);
    }
}