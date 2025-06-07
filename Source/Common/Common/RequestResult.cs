namespace Common;

public enum RequestResult : byte
{
    Success,
    ErrorTimedOut,
    ErrorReceivedButNoResponse
}