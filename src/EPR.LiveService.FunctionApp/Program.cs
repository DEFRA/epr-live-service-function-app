using Azure.Monitor.OpenTelemetry.Exporter;
using EPR.LiveService.FunctionApp.Queries;
using EPR.LiveService.FunctionApp.Sql;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.Functions.Worker.OpenTelemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING")))
{
    builder.Services.AddOpenTelemetry()
        .UseFunctionsWorkerDefaults()
        .UseAzureMonitorExporter();
}

builder.Services.Configure<Dictionary<string, SqlTargetOptions>>(
    builder.Configuration.GetSection("SqlTargets"));

builder.Services.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();
builder.Services.AddSingleton<IQueryRegistry, QueryRegistry>();

var app = builder.Build();
_ = app.Services.GetRequiredService<IQueryRegistry>();
app.Run();

