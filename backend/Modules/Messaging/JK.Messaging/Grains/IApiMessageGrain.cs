namespace JK.Messaging.Grains;

public interface IApiMessageGrain: IGrainWithStringKey
{
    Task SendApiMessage(string fullUrl);
    Task<bool> Register(string cronString);
}