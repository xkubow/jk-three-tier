using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using JK.Messaging.Contracts;
using JK.Messaging.Grains;
using JK.Messaging.Proto;
using Orleans;
using ApiMessageState = JK.Messaging.Proto.ApiMessageState;
using CreateApiMessageTaskRequest = JK.Messaging.Proto.CreateApiMessageTaskRequest;

namespace JK.Messaging.Grpc;

public class ApiMessageTaskGrpcService : GrpcApiMessageTask.GrpcApiMessageTaskBase
{
    private readonly IClusterClient _clusterClient;

    public ApiMessageTaskGrpcService(IClusterClient clusterClient)
    {
        _clusterClient = clusterClient;
    }

    public override async Task<ApiMessageTaskStateResponse> Create(CreateApiMessageTaskRequest request, ServerCallContext context)
    {
        var grain = _clusterClient.GetGrain<IApiMessageTaskGrain>(request.TaskId);

        var registered = await grain.Register(new RegisterApiMessageTaskCommand
        {
            Id = request.TaskId,
            TaskName = request.TaskName,
            TargetUrl = request.TargetUrl,
            MaxAttempts = request.MaxAttempts,
            Delay = request.Delay?.ToTimeSpan(),
            RetryDelay = request.RetryDelay?.ToTimeSpan()
        });

        if (!registered)
            throw new RpcException(new Status(StatusCode.AlreadyExists, $"Task '{request.TaskId}' has already been registered."));

        var state = await grain.GetState();

        return MapToResponse(state);
    }

    public override async Task<ApiMessageTaskStateResponse> GetState(GetApiMessageTaskStateRequest request, ServerCallContext context)
    {
        var grain = _clusterClient.GetGrain<IApiMessageTaskGrain>(request.TaskId);
        var state = await grain.GetState();
        return MapToResponse(state);
    }

    public override async Task<Empty> Cancel(CancelApiMessageTaskRequest request, ServerCallContext context)
    {
        var grain = _clusterClient.GetGrain<IApiMessageTaskGrain>(request.TaskId);
        await grain.CancelAsync();
        return new Empty();
    }

    private static ApiMessageTaskStateResponse MapToResponse(States.ApiMessageTaskState state)
    {
        return new ApiMessageTaskStateResponse
        {
            TaskId = state.TaskId,
            TaskName = state.TaskName,
            TargetUrl = state.TargetUrl,
            TaskState = (ApiMessageState)state.TaskState,
            Attempts = state.Attempts,
            MaxAttempts = state.MaxAttempts,
            LastError = state.LastError ?? string.Empty,
            CreatedOn = Timestamp.FromDateTime(DateTime.SpecifyKind(state.CreatedOn, DateTimeKind.Utc)),
            StartTime = state.StartTime.HasValue ? Timestamp.FromDateTime(DateTime.SpecifyKind(state.StartTime.Value, DateTimeKind.Utc)) : null,
            FinishTime = state.FinishTime.HasValue ? Timestamp.FromDateTime(DateTime.SpecifyKind(state.FinishTime.Value, DateTimeKind.Utc)) : null,
            NextRetryOn = state.NextRetryOn.HasValue ? Timestamp.FromDateTime(DateTime.SpecifyKind(state.NextRetryOn.Value, DateTimeKind.Utc)) : null
        };
    }
}
