using GS1.WinClient.Services;
using Microsoft.Extensions.DependencyInjection;

namespace GS1.WinClient;

static class Program
{
    public static IServiceProvider ServiceProvider { get; private set; } = null!;

    
    ///  The main entry point for the application.
    
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        
        // DI Container kurulumu
        var services = new ServiceCollection();
        ConfigureServices(services);
        ServiceProvider = services.BuildServiceProvider();

        Application.Run(ServiceProvider.GetRequiredService<MainForm>());
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // API Base URL
        var apiBaseUrl = "https://bulutalbum.com.tr"; // veya appsettings'den okunabilir

        // HttpClient
        services.AddHttpClient<ApiClient>(client =>
        {
            client.BaseAddress = new Uri(apiBaseUrl);
            client.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        });

        // Forms
        services.AddTransient<MainForm>();
    }
}