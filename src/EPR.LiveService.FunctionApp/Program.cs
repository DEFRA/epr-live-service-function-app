using Azure.Monitor.OpenTelemetry.Exporter;
using EPR.LiveService.FunctionApp.Formatting;
using EPR.LiveService.FunctionApp.Middleware;
using EPR.LiveService.FunctionApp.Notifications;
using EPR.LiveService.FunctionApp.Queries;
using EPR.LiveService.FunctionApp.Sql;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.Functions.Worker.OpenTelemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();
builder.UseMiddleware<AuthClaimsLoggingMiddleware>();

if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING")))
{
    builder.Services.AddOpenTelemetry()
        .UseFunctionsWorkerDefaults()
        .UseAzureMonitorExporter();
}

builder.Services.Configure<Dictionary<string, SqlTargetOptions>>(
    builder.Configuration.GetSection("SqlTargets"));

builder.Services.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();
builder.Services.AddSingleton<IClientPrincipalDecoder, ClientPrincipalDecoder>();
builder.Services.AddSingleton<IQueryRegistry, QueryRegistry>();
builder.Services.AddSingleton<IEmailNotificationSender, GovUkNotifyEmailSender>();
builder.Services.AddSingleton<IQueryResultActionProvider, ResendInvitateEmailActionProvider>();

// The enum-to-formatter map: every QueryOutputFormat needs an entry here.
// RunQueryFunction resolves the right one via GetRequiredKeyedService rather
// than switching on the format itself.
builder.Services.AddKeyedSingleton<IQueryResultFormatter, HtmlTableFormatter>(QueryOutputFormat.Html);
builder.Services.AddKeyedSingleton<IQueryResultFormatter, AsciiTableFormatter>(QueryOutputFormat.AsciiTable);
builder.Services.AddKeyedSingleton<IQueryResultFormatter, CsvFormatter>(QueryOutputFormat.Csv);
builder.Services.AddKeyedSingleton<IQueryResultFormatter, ListFormatter>(QueryOutputFormat.List);

var app = builder.Build();
_ = app.Services.GetRequiredService<IQueryRegistry>();

// Fail fast at startup if a QueryOutputFormat is missing its keyed registration
// above, rather than only discovering the gap on the first request for that
// format.
foreach (var format in Enum.GetValues<QueryOutputFormat>())
{
    _ = app.Services.GetRequiredKeyedService<IQueryResultFormatter>(format);
}

app.Run();

