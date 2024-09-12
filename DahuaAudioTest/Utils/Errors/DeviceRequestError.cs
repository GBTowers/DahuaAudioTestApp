namespace DahuaAudioTest.Utils.Errors;

public class DeviceRequestError : Exception
{
    public string RequestUrl { get; }
    public DeviceRequestError(string message, string requestUrl) : base(message)
    {
        RequestUrl = requestUrl;
    }
}