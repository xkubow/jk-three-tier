namespace JK.Messaging.Contracts.Enums;

public enum ApiMessageStateEnum
{
    Waiting = 0,
    Created = 1,
    InQueue = 2,
    Processing = 3,
    Done = 4,
    Failed = 5,
    Disabled = 6,
    Suspended = 7,
    Cancelled = 8
}