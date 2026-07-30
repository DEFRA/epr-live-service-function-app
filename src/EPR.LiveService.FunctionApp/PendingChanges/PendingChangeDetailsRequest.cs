using System.Net.Mail;

namespace EPR.LiveService.FunctionApp.PendingChanges;

public class PendingChangeDetailsRequest
{
    public string? BearerToken { get; set; }

    public string? RegulatorEmail { get; set; }

    public string? UserEmail { get; set; }

    public string? UserOrganisationId { get; set; }

    public IReadOnlyList<string> Validate()
    {
        var errors = new List<string>();

        AddRequiredError(errors, BearerToken, nameof(BearerToken));
        AddRequiredError(errors, RegulatorEmail, nameof(RegulatorEmail));
        AddRequiredError(errors, UserEmail, nameof(UserEmail));
        AddRequiredError(errors, UserOrganisationId, nameof(UserOrganisationId));
        AddEmailError(errors, RegulatorEmail, nameof(RegulatorEmail));
        AddEmailError(errors, UserEmail, nameof(UserEmail));

        return errors;
    }

    private static void AddRequiredError(List<string> errors, string? value, string name)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors.Add($"{name} is required.");
        }
    }

    private static void AddEmailError(List<string> errors, string? value, string name)
    {
        if (!string.IsNullOrWhiteSpace(value) && !MailAddress.TryCreate(value, out _))
        {
            errors.Add($"{name} must be a valid email address.");
        }
    }
}
