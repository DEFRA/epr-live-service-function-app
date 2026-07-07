using Azure.Monitor.OpenTelemetry.Exporter;
using EPR.LiveService.FunctionApp;
using EPR.LiveService.FunctionApp.Repositories;
using EPR.LiveService.FunctionApp.Repositories.Interfaces;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.Functions.Worker.OpenTelemetry;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

const string connectionStringIdentifier = "SqlDbConnectionString";

var configuration = GetSettings();
var connectionStrings = GetSqlServerConnectionString(configuration);

if (connectionStrings is not null)
{
    builder.Services.AddSingleton(connectionStrings);
}

if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING")))
{
    builder.Services.AddOpenTelemetry()
        .UseFunctionsWorkerDefaults()
        .UseAzureMonitorExporter();
}


builder.Services.AddTransient<IOrganisationRepository, OrganisationRepository>();

builder.Build().Run();



static IConfigurationRoot GetSettings()
{
    IConfigurationRoot configurationRoot = null!;

    configurationRoot = new ConfigurationBuilder()
        .SetBasePath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
        .AddJsonFile("local.settings.json", optional: true)
        .AddEnvironmentVariables()
        .Build();

    return configurationRoot;
}


static ConnectionStrings GetSqlServerConnectionString(
    IConfigurationRoot configuration)
{
    ConnectionStrings connectionString = null!;

    connectionString = new ConnectionStrings
    {
        SqlDbConnectionString = configuration.GetConnectionString("SqlDbConnectionString") ??
                                throw new Exception("SqlDbConnectionString connection string configuration is missing")
    };

    return connectionString;
}