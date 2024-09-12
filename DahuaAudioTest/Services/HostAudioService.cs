using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NAudio.Wave;
using OutputDevice = (int deviceNumber, NAudio.Wave.WaveOutCapabilities caps);
using InputDevice = (int deviceNumber, NAudio.Wave.WaveInCapabilities caps);

namespace DahuaAudioTest.Services;

public class HostAudioService
{
    private readonly ILogger<HostAudioService> _logger;
    private readonly AudioSettings _settings;
    private WaveFileWriter? _fileWriter;
    private WaveInEvent? _inputDevice;

    public HostAudioService(ILogger<HostAudioService> logger, IOptions<AudioSettings> settings)
    {
        _logger = logger;
        _settings = settings.Value;
    }

    public void Run()
    {
        _logger.LogInformation("Running Audio Service");

        OutputDevice outputDevice = GetOutputDevice();

        InputDevice inputDevice = GetInputDevice();
        
        RecordAudio(inputDevice);

    }

    #region Init Devices
    private OutputDevice GetOutputDevice()
    {
        int deviceCount = WaveOut.DeviceCount;
        _logger.LogInformation("Searching through {Count} output devices", deviceCount);
        
        for (int n = -1; n < deviceCount; n++)
        {
            WaveOutCapabilities caps = WaveOut.GetCapabilities(n);
            if (!caps.ProductName.Contains(_settings.ProductName)) continue;
            
            _logger.LogInformation("Found output device: {@Device}", caps);
            return (n, caps);
        }
        _logger.LogWarning("Could not find configured device, using Microsoft Sound Mapper instead");

        return (-1, WaveOut.GetCapabilities(-1));
    }

    private InputDevice GetInputDevice()
    {
        int deviceCount = WaveIn.DeviceCount;
        _logger.LogInformation("Searching through {Count} input devices", deviceCount);
        
        for (int n = -1; n < deviceCount; n++)
        {
            WaveInCapabilities caps = WaveIn.GetCapabilities(n);
            if (!caps.ProductName.Contains(_settings.ProductName)) continue;
            
            _logger.LogInformation("Found output device: {@Device}", caps);
            return (n, caps);
        }
        _logger.LogWarning("Could not find configured device, using Microsoft Sound Mapper instead");

        return (-1, WaveIn.GetCapabilities(-1));
    }
    #endregion

    private void RecordAudio(InputDevice device)
    {
        string outputFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "NAudio");
        Directory.CreateDirectory(outputFolder);
        string outputFilePath = Path.Combine(outputFolder,"recorded.wav");
        
        _logger.LogInformation("Recording to {filePath}", outputFilePath);

        _inputDevice = new WaveInEvent {DeviceNumber = device.deviceNumber};

        _fileWriter = new WaveFileWriter(outputFilePath, _inputDevice.WaveFormat);
        
        _inputDevice.DataAvailable += waveSource_DataAvailable;
        
        _inputDevice.RecordingStopped += waveSource_RecordingStopped;
        
        
        _inputDevice.StartRecording();
        
        Console.WriteLine("Press enter to stop");
        Console.ReadLine();
        
        _inputDevice.StopRecording();
    }

    private void StreamAudio()
    {
        
    }
    
    
    #region Event Handlers
    private void waveSource_RecordingStopped(object? sender, StoppedEventArgs args)
    {
        _logger.LogInformation("Disposing of file writer and device");
        _fileWriter?.Dispose();
        _fileWriter = null;
        
        _inputDevice?.Dispose();
    }

    private void waveSource_DataAvailable(object? sender, WaveInEventArgs args)
    {
        _logger.LogInformation("Recording {@Info}", args);
        
        if (_fileWriter == null) return;
        _fileWriter.Write(args.Buffer, 0, args.BytesRecorded);

        if (_fileWriter.Position <= _inputDevice!.WaveFormat.AverageBytesPerSecond * 10) return;
        _logger.LogInformation("Stopped recording after {@info}", args);
        _inputDevice.StopRecording();
    }
    #endregion
}