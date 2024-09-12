using System.Net;
using DahuaAudioTest.Utils.Errors;
using DahuaAudioTest.Utils.Results;
using Flurl.Http;
using Flurl.Util;

namespace DahuaAudioTest.Utils.Extensions;

public static class FlurlExtensions
{
    public static async Task<Result<IFlurlResponse, DeviceRequestError>> SendAsyncWithResult(this IFlurlRequest request,
        HttpMethod verb, 
        HttpContent? content = null, 
        HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            IFlurlResponse? response = await request.SendAsync(verb, content, completionOption, cancellationToken);

            if (response is null)
            {
                return new DeviceRequestError("Null Response received", request.Url);
            }
            
            if (response.StatusCode != (int)HttpStatusCode.OK)
            {
                return new DeviceRequestError(string.Concat(response.Headers), request.Url);
            }

            return new Result<IFlurlResponse, DeviceRequestError>(response);
        }
        catch (FlurlHttpException ex)
        {
            return new DeviceRequestError(ex.Message, request.Url);
        }
    }

    public static async Task<DahuaApiResponse> ParseResponse(this IFlurlResponse response)
    {
        string stringResponse = await response.GetStringAsync();
        var dahuaResponse = new DahuaApiResponse(stringResponse.ToKeyValuePairs());

        return dahuaResponse;
    }
}

public class DahuaApiResponse
{
    public DahuaApiResponse(IEnumerable<(string Key, object Value)> parsedResponse)
    {
        ParsedResponse = parsedResponse;
    }

    public IEnumerable<(string Key, object Value)> ParsedResponse { get; set; }
}