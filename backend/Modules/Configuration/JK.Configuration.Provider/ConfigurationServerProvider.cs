using System.Reflection;
using Grpc.Core;
using JK.Configuration.Proto;
using JK.Platform.Grpc.Client.Factory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using static JK.Configuration.Proto.ConfigurationGrpc;

namespace JK.Configuration.Provider;

public sealed class ConfigurationServerProvider : ConfigurationProvider, IDisposable
{
    private const int ReloadPeriodInMillisecondsDefault = 60_000;
    private const int InitialRetryInMillisecondsDefault = 10_000;

    private static Exception? _lastLoadException;

    private readonly ILogger<ConfigurationServerProvider> _logger;
    private readonly IGrpcClientFactory<ConfigurationGrpcClient> _grpcClientFactory;
    private readonly GrpcConfigurationRequest _grpcConfigurationRequest;

    private readonly bool _reloadEnabled;
    private readonly string _configurationServerUrl;
    private int _reloadPeriodInMilliseconds;
    private int _initialRetryMilliseconds;

    private bool _firstLoad = true;
    private CancellationTokenSource? _cts;
    private Task? _refreshWorker;

    public ConfigurationServerProvider(
        IConfigurationBuilder configurationBuilder,
        ConfigurationServerSource configurationServerSource)
    {
        var bootstrapBuilder = new ConfigurationBuilder();

        foreach (var source in configurationBuilder.Sources)
        {
            if (source is not ConfigurationServerSource)
            {
                bootstrapBuilder.Add(source);
            }
        }

        var configuration = bootstrapBuilder.Build();

        using var loggerFactory = LoggerFactory.Create(logging =>
        {
            logging.AddConfiguration(configuration.GetSection("Logging"));
            logging.AddConsole();
        });

        _logger = loggerFactory.CreateLogger<ConfigurationServerProvider>();

        _grpcClientFactory = configurationServerSource.GrpcClientFactory
            ?? throw new InvalidOperationException(
                "IGrpcClientFactory<ConfigurationGrpcClient> is not configured.");

        _reloadEnabled = configuration.GetValue<bool?>("ConfigurationProvider:ReloadEnabled") ?? true;
        _configurationServerUrl = configuration["ConfigurationProvider:ServerUrl"]
            ?? throw new InvalidOperationException("Missing configuration key: ConfigurationProvider:ServerUrl");
        _reloadPeriodInMilliseconds = configuration.GetValue<int?>("ConfigurationProvider:ReloadPeriodInMilliseconds")
            ?? ReloadPeriodInMillisecondsDefault;
        _initialRetryMilliseconds = configuration.GetValue<int?>("ConfigurationProvider:InitialRetryMilliseconds")
            ?? InitialRetryInMillisecondsDefault;

        var marketCode = configuration["Platform:MarketCode"] ?? string.Empty;
        var serviceCode =
            configuration["Platform:ServiceCode"]
            ?? Assembly.GetEntryAssembly()?.GetName().Name
            ?? "UnknownService";

        _grpcConfigurationRequest = new GrpcConfigurationRequest
        {
            MarketCode = marketCode,
            ServiceCode = serviceCode
        };

        _logger.LogInformation(
            "ConfigurationServerProvider settings: ServerUrl={ServerUrl}, MarketCode={MarketCode}, ServiceCode={ServiceCode}, ReloadEnabled={ReloadEnabled}",
            _configurationServerUrl,
            marketCode,
            serviceCode,
            _reloadEnabled);
    }

    public override void Load()
    {
        LoadAsync(CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();

        if (_cts is not null)
        {
            return;
        }

        _cts = new CancellationTokenSource();
        var ct = _cts.Token;

        _refreshWorker ??= Task.Run(async () =>
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(_reloadPeriodInMilliseconds, ct);

                    if (_reloadEnabled)
                    {
                        await LoadAsync(ct);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "ConfigurationServerProvider refresh loop failed.");
                }
            }
        }, ct);
    }

    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        var firstLoad = _firstLoad;

        try
        {
            var configurations = await LoadConfigurationAsync(cancellationToken);

            var newData = BuildConfigurationData(configurations, firstLoad);

            Data = newData;

            if (!firstLoad)
            {
                OnReload();
            }
        }
        catch (RpcException rpcException)
        {
            _logger.LogWarning(
                "ConfigurationServerProvider failed to load configuration. Status={StatusCode}, Detail={Detail}, ServiceCode={ServiceCode}",
                rpcException.StatusCode,
                rpcException.Status.Detail,
                _grpcConfigurationRequest.ServiceCode);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "ConfigurationServerProvider failed to load configuration. ServiceCode={ServiceCode}",
                _grpcConfigurationRequest.ServiceCode);
        }
    }

    private Dictionary<string, string?> BuildConfigurationData(IReadOnlyCollection<GrpcConfiguration> configurations, bool firstLoad)
    {
        var newData = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            ["Platform:ServiceCode"] = _grpcConfigurationRequest.ServiceCode
        };

        if (configurations?.Any() == true)
        {
            var groupedConfigurations = configurations.GroupBy(c => c.Key);

            foreach (var group in groupedConfigurations)
            {
                if (string.IsNullOrWhiteSpace(group.Key))
                {
                    continue;
                }

                var configList = group.ToList();
                var isListConfiguration = configList.Any(c => c.IsList);

                if (isListConfiguration)
                {
                    ProcessListConfiguration(group.Key, configList, newData, firstLoad);
                }
                else
                {
                    ProcessSingleConfiguration(group.Key, configList, newData, firstLoad);
                }
            }
        }

        return newData;
    }

    private void ProcessListConfiguration(string key, List<GrpcConfiguration> configList, Dictionary<string, string?> newData, bool firstLoad)
    {
        _logger.LogDebug(
            "Processing list configuration. Key={Key}, Count={Count}",
            key,
            configList.Count);

        for (int i = 0; i < configList.Count; i++)
        {
            var configuration = configList[i];
            var indexedKey = $"{key}:{i}";
            newData[indexedKey] = configuration.Value;

            if (!firstLoad &&
                Data.TryGetValue(indexedKey, out var currentValue) &&
                currentValue != configuration.Value)
            {
                _logger.LogWarning(
                    "Configuration list value changed. Key={Key}, OldValue={OldValue}, NewValue={NewValue}",
                    indexedKey,
                    currentValue,
                    configuration.Value);
            }
        }
    }

    private void ProcessSingleConfiguration(string key, List<GrpcConfiguration> configList, Dictionary<string, string?> newData, bool firstLoad)
    {
        var configuration = configList.First();

        if (configList.Count > 1)
        {
            _logger.LogWarning(
                "ConfigurationServerProvider duplicate key detected for non-list configuration. Key={Key}, Count={Count}",
                key,
                configList.Count);
        }

        newData[configuration.Key] = configuration.Value;

        if (!firstLoad &&
            Data.TryGetValue(configuration.Key, out var currentValue) &&
            currentValue != configuration.Value)
        {
            _logger.LogWarning(
                "Configuration value changed. Key={Key}, OldValue={OldValue}, NewValue={NewValue}",
                configuration.Key,
                currentValue,
                configuration.Value);
        }

        UpdateProviderSettings(configuration.Key, configuration.Value);
    }

    private void UpdateProviderSettings(string key, string? value)
    {
        if (string.Equals(key, "ConfigurationProvider:ReloadPeriodInMilliseconds", StringComparison.OrdinalIgnoreCase))
        {
            _ = int.TryParse(value, out _reloadPeriodInMilliseconds);
        }
        else if (string.Equals(key, "ConfigurationProvider:InitialRetryMilliseconds", StringComparison.OrdinalIgnoreCase))
        {
            _ = int.TryParse(value, out _initialRetryMilliseconds);
        }
    }

    private async Task<IReadOnlyCollection<GrpcConfiguration>> LoadConfigurationAsync(CancellationToken cancellationToken)
    {
        try
        {
            var client = _grpcClientFactory.GetClient(_configurationServerUrl);
            var result = await client.GetConfigurationAsync(_grpcConfigurationRequest, cancellationToken: cancellationToken);

            if (_firstLoad)
            {
                _firstLoad = false;
                _logger.LogInformation("ConfigurationServerProvider initial configuration loaded.");
            }

            return result.Configurations.ToList();
        }
        catch (Exception ex)
        {
            if (_firstLoad)
            {
                LogFirstConfigurationLoadException(ex);

                await Task.Delay(_initialRetryMilliseconds, cancellationToken);
                return await LoadConfigurationAsync(cancellationToken);
            }

            throw;
        }
    }

    private void LogFirstConfigurationLoadException(Exception exception)
    {
        if (_lastLoadException is null || exception.Message != _lastLoadException.Message)
        {
            _lastLoadException = exception;

            if (exception is RpcException rpcException)
            {
                _logger.LogError(
                    "ConfigurationServerProvider failed to load initial configuration. Status={StatusCode}, Detail={Detail}. Next try after {RetryMilliseconds}ms.",
                    rpcException.StatusCode,
                    rpcException.Status.Detail,
                    _initialRetryMilliseconds);
            }
            else
            {
                _logger.LogError(
                    exception,
                    "ConfigurationServerProvider failed to load initial configuration. Next try after {RetryMilliseconds}ms.",
                    _initialRetryMilliseconds);
            }
        }
        else
        {
            if (exception is RpcException rpcException)
            {
                _logger.LogDebug(
                    "ConfigurationServerProvider failed to load initial configuration again. Status={StatusCode}, Detail={Detail}. Next try after {RetryMilliseconds}ms.",
                    rpcException.StatusCode,
                    rpcException.Status.Detail,
                    _initialRetryMilliseconds);
            }
            else
            {
                _logger.LogDebug(
                    "ConfigurationServerProvider failed to load initial configuration again. Message={Message}. Next try after {RetryMilliseconds}ms.",
                    exception.Message,
                    _initialRetryMilliseconds);
            }
        }
    }

    ~ConfigurationServerProvider()
    {
        Dispose(false);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!disposing)
        {
            return;
        }

        _cts?.Cancel();

        if (_refreshWorker is not null)
        {
            try
            {
                _refreshWorker.ConfigureAwait(false).GetAwaiter().GetResult();
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "ConfigurationServerProvider exception during dispose. ServiceCode={ServiceCode}",
                    _grpcConfigurationRequest.ServiceCode);
            }
        }

        _cts?.Dispose();
        _cts = null;
        _refreshWorker = null;
    }
}