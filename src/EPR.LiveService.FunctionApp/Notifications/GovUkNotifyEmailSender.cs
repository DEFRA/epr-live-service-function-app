using Notify.Client;
using Microsoft.Extensions.Configuration;
using System.Diagnostics.CodeAnalysis;

namespace EPR.LiveService.FunctionApp.Notifications;

[ExcludeFromCodeCoverage]
public class GovUkNotifyEmailSender : IEmailNotificationSender
{
    private readonly string? _apiKey;

    public GovUkNotifyEmailSender(IConfiguration configuration)
    {
        _apiKey = configuration["GovUkNotify:ApiKey"];
    }

    public async Task SendAsync(
        string emailAddress,
        string templateId,
        Dictionary<string, dynamic> personalisation)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            throw new InvalidOperationException(
                "Missing GOV.UK Notify API key configuration 'GovUkNotify:ApiKey'.");
        }

        var client = new NotificationClient(_apiKey);
        await client.SendEmailAsync(emailAddress, templateId, personalisation);
    }
}
