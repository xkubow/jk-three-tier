using JK.Messaging.Contracts;
using JK.Messaging.States;

namespace JK.Messaging.Grains;

public interface IApiMessageTaskGrain : IGrainWithStringKey
{
    Task<bool> Register(RegisterApiMessageTaskCommand taskModel);
    Task<ApiMessageTaskState> GetState();
    Task CancelAsync();
}