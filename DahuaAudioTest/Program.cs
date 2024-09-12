using System.Net;
using DahuaAudioTest.Services;
using Flurl.Http;
using Flurl.Http.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace DahuaAudioTest;

internal class Program
{
    public static void Main(string[] args)
    {
        var builder = new ConfigurationBuilder();
        BuildInitialConfig(builder);

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Build()).CreateLogger();
        
        Log.Logger.Information("Application Starting");

        IHost host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                IConfigurationSection audioSettings = context.Configuration.GetSection(nameof(HostAudioService));
                services.Configure<AudioSettings>(audioSettings);
                services.AddTransient<HostAudioService>();
                services.AddSingleton<IFlurlClientCache>(sp => new FlurlClientCache().WithDefaults(clientBuilder =>
                {
                    clientBuilder
                        .AllowAnyHttpStatus()
                        .ConfigureInnerHandler(handler =>
                        {
                            handler.Credentials = new NetworkCredential("admin", "lgc14298");
                        });
                }));
            })
            .UseSerilog()
            .Build();
        var svc = ActivatorUtilities.CreateInstance<HostAudioService>(host.Services);

        svc.Run();
        
        Log.Logger.Information("Finished all processes");

        Console.ReadLine();
    }

    private static void BuildInitialConfig(IConfigurationBuilder builder)
    {
        builder.SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
            .AddEnvironmentVariables();
    }
}