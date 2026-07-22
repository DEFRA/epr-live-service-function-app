namespace EPR.LiveService.FunctionApp.Notifications;

public interface IEmailNotificationSender
{
    Task SendAsync(
        string emailAddress,
        string templateId,
        Dictionary<string, dynamic> personalisation);
}
