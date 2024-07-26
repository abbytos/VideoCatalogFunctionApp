using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using VideoCatalogFunctionApp.Utilities;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
        // You can also add other sources here if necessary
    })
    .ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;

        // Log configuration values for debugging
        Console.WriteLine($"Key Vault URL: {configuration["KeyVault:Url"]}");
        Console.WriteLine($"Blob Storage Container Name: {configuration["BlobStorage:ContainerName"]}");
        Console.WriteLine($"Blob Storage Connection String Secret Name: {configuration["BlobStorage:ConnectionStringSecretName"]}");

        // Retrieve Key Vault URL from configuration
        var keyVaultUrl = configuration["KeyVault:Url"];
        if (string.IsNullOrEmpty(keyVaultUrl))
        {
            throw new InvalidOperationException("Key Vault URL is not configured.");
        }

        // Add the SecretClient
        services.AddSingleton(new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential()));

        // Register other services
        services.AddTransient<KeyVaultHelper>();
        services.AddTransient<ConfigurationHelper>();

        services.AddSingleton(sp =>
        {
            var keyVaultHelper = sp.GetRequiredService<KeyVaultHelper>();
            var connectionString = keyVaultHelper.GetSecretAsync(configuration["BlobStorage:ConnectionStringSecretName"]).GetAwaiter().GetResult();
            return new BlobServiceClient(connectionString);
        });

        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
    })
    .Build();

await host.RunAsync();
