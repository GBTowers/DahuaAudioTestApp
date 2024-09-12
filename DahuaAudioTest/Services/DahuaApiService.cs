using DahuaAudioTest.Utils.Extensions;
using Flurl.Http;
using Flurl.Http.Configuration;
using Microsoft.Extensions.Logging;

namespace DahuaAudioTest.Services;

public class DahuaApiService
{
    private readonly ILogger<DahuaApiService> _logger;
    private readonly IFlurlClientCache _clientCache;

    private class EndPoints
    {
        public const string AudioInput = "devAudioInput.cgi";
        public const string AudioOutput = "devAudioOutput.cgi";
        public const string AudioStream = "audio.cgi";
    }

    public DahuaApiService(ILogger<DahuaApiService> logger, IFlurlClientCache clientCache)
    {
        _logger = logger;
        _clientCache = clientCache;
    }

    public async Task<Stream> GetAudioStream(string deviceAddress)
    {
        IFlurlClient? client = _clientCache.GetOrAdd(deviceAddress, deviceAddress + "cgi-bin");

        Stream? result = await client
            .Request()
            .AppendPathSegment(EndPoints.AudioStream)
            .SetQueryParams(new
            {
                action = "getAudio",
                httptype = "singlepart",
                channel = 1
            }).GetStreamAsync();

        return result;
    }
}