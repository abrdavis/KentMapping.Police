using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using CommandLine;
using dotenv.net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RavelDev.Core.Interfaces;
using RavelDev.Core.Repo;
using RavelDev.GoogleMaps.API;
using RavelDev.GoogleMaps.Models;
using RavelDev.KentMapping.Police.Core.Models.Config;
using RavelDev.KentMapping.Police.Core.Repo;
using RavelDev.KentMapping.Police.Core.Utility;
using RavelDev.KentMapping.PoliceLogParser;

IConfiguration config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables()
    .Build();
IDictionary<string, string> envVars;
envVars = DotEnv.Read();

var keyVaultName = envVars["KEY_VAULT_NAME"];
var kvUri = $"https://{keyVaultName}.vault.azure.net";

var kvClient = new SecretClient(new Uri(kvUri), new DefaultAzureCredential());
var connectionStringSecret = await kvClient.GetSecretAsync("local-postgis-cs");
var gMapsApiSecret = await kvClient.GetSecretAsync("gmaps-api");

var gMapsApiKey = gMapsApiSecret.Value?.Value ?? string.Empty;
var connectionString = connectionStringSecret.Value?.Value ?? string.Empty;

if (string.IsNullOrEmpty(connectionString))
{
    Console.WriteLine("Connection string not configured.");
    return;
}

if (string.IsNullOrEmpty(gMapsApiKey))
{
    Console.WriteLine("Google maps api key not configured.");
    return;
}


IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddSingleton<IRepositoryConfig>(new RepositoryConfig(connectionString));
        services.AddSingleton(new GoogleMapApiConfig(gMapsApiKey));
        services.Configure<PoliceParserConfig>(config.GetSection("PoliceParserConfig"));
        services.AddSingleton<KentPoliceRepository>();
        services.AddScoped<GoogleMapsWebApi>();
        services.AddScoped<KentPoliceParser>();
    })
    .Build();

RunProcess(host.Services, args);
static void RunProcess(IServiceProvider services, string[] args)
{
    try
    {
        using IServiceScope serviceScope = services.CreateScope();
        var provider = serviceScope.ServiceProvider;

        var policeParser = provider.GetRequiredService<KentPoliceParser>();

        Parser.Default.ParseArguments<CLArguments>(args)
                   .WithParsed(o =>
                   {
                       if (o.ParseAll)
                       {
                           policeParser.ParsePendingReports();
                           policeParser.ParsePendingIncidents();
                       }
                       else if (o.ParseReports)
                       {
                           policeParser.ParsePendingReports();
                       }
                       else if (o.ParseIncidents)
                       {
                           policeParser.ParsePendingIncidents();
                       }

                       if (o.MapAddress)
                       {
                           policeParser.PopulateAddressesWithoutPlaces();
                       }
                   });
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
    }

}
