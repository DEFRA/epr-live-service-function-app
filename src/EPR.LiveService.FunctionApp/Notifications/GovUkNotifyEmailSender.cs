using Notify.Client;
using Microsoft.Extensions.Configuration;
using System.Diagnostics.CodeAnalysis;

namespace EPR.LiveService.FunctionApp.Notifications;

[ExcludeFromCodeCoverage(Justification = "SendAsync makes a real outbound call via the GOV.UK Notify SDK client, constructed inline rather than injected — not mockable without a network call or a wrapping interface.")]
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
