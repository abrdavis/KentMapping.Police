using Azure.Identity;
using Dapper;
using dotenv.net;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using RavelDev.Core.Interfaces;
using RavelDev.Core.Repo;
using RavelDev.Core.Sql.TypeHandler;
using RavelDev.KentMapping.Police.Core.Repo;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins(
                              "https://kentpolice.raveldev.com",
                              "https://www.kentpolice.raveldev.com").AllowAnyHeader().AllowCredentials();
                      });
});
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
});

IDictionary<string, string> envVars;
envVars = DotEnv.Read();


bool useKeyVaultForSecrets = false;
var connectionString = string.Empty;
if (useKeyVaultForSecrets)
{
    var keyVaultName = envVars["KEY_VAULT_NAME"];

    Uri vaultUri = new Uri($"https://{keyVaultName}.vault.azure.net/");

    var credentails = new DefaultAzureCredential();

    builder.Configuration.AddAzureKeyVault(vaultUri, credentails);

    connectionString = builder.Configuration["local-postgis-cs"];
}
else
{
    connectionString = Environment.GetEnvironmentVariable("POLICE_API_CS");
    Console.WriteLine($"API Key: {connectionString}");
}


builder.Services.AddSingleton<IRepositoryConfig>(new RepositoryConfig(connectionString));
builder.Services.AddScoped<KentPoliceRepository>();


var app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(MyAllowSpecificOrigins);
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
