using Azure.Identity;
using Dapper;
using dotenv.net;
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
                          policy.WithOrigins("http://localhost:5173",
                              "https://localhost:5173").AllowAnyHeader().AllowCredentials();
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

var keyVaultName = envVars["KEY_VAULT_NAME"];

Uri vaultUri = new Uri($"https://{keyVaultName}.vault.azure.net/");

var credentails = new DefaultAzureCredential();

builder.Configuration.AddAzureKeyVault(vaultUri, credentails);

var connectionString = builder.Configuration["local-postgis-cs"];

builder.Services.AddSingleton<IRepositoryConfig>(new RepositoryConfig(connectionString));
builder.Services.AddScoped<KentPoliceRepository>();


var app = builder.Build();

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
