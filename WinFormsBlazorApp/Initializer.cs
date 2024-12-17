

using DessertPlate.Commands;
using DessertPlate.Commands.Abstraction;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Registry;
using Serilog;

namespace WinFormsBlazorApp;
internal static class Initializer
{
    public static IServiceProvider Services { get; private set; } = default!;
    public static void Initialize()
    {
        try
        {
            var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                    .AddUserSecrets(typeof(Initializer).Assembly, optional: true)
                    .Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("SourceSystem", "UI")                                
            .CreateLogger();

            ServiceCollection services = new();

            services.AddSingleton<IConfiguration>(configuration);

            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddSerilog(dispose: true);
            });

            Log.Information("Initializer After AddLogging...");

            services.AddWindowsFormsBlazorWebView();
            services.AddBlazorWebViewDeveloperTools();

            services.AddResiliencePipeline(HttpCommandClient.ResiliencePipelineKey, builder =>
            {

            });
            
            services.AddHttpClient();
            //services.AddArtworkCommandRequestJsonOptions();

            services.AddSingleton<ICommandClient>(sp =>
            {
                var commandRegister = sp.GetRequiredService<ICommandRegister>();
                HttpClient httpClient = new()
                {
                    BaseAddress = new(configuration.GetValue<string>("Api:Commands") ?? throw new InvalidOperationException("Api:Commands not configured"))
                };
                var resilience = sp.GetRequiredService<ResiliencePipelineProvider<string>>();
                var logger = sp.GetRequiredService<ILogger<HttpCommandClient>>();
                var jsonProvider = sp.GetRequiredService<ICommandClientRequestJsonOptionProvider>();
                return new HttpCommandClient(
                    httpClient,
                    commandRegister,
                    resilience,
                    jsonProvider,
                    logger);
            });

            services.AddHttpClient("Maintenance", config =>
            {
                config.BaseAddress = new(configuration.GetValue<string>("Api:Maintenance") ?? throw new InvalidOperationException("Api:Maintenance not configured"));
            });

            Log.Information("Initializer Before AddUI... ");

            //services.AddUI(configuration.GetValue<string>("Api:CommandsHub") ?? throw new InvalidOperationException("Api:CommandsHub not configured"));

            Log.Information("Initializer After AddUI... ");

            //services.AddSingleton<ISelectFileService, SelectFileService>();

            //services.AddSingleton<IErrorBoundaryLogger, ErrorBundaryLogger>();

            Services = services.BuildServiceProvider();
        }
        catch(Exception ex)
        {
            Log.Error(ex, "err An unexpected error occured while initializing application");
            Log.Fatal(ex, "An unexpected error occured while initializing application");
            throw;
        }
    }
}
