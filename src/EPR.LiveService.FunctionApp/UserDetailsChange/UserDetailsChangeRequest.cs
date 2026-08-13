using System.Net.Mail;

namespace EPR.LiveService.FunctionApp.UserDetailsChange;

public class UserDetailsChangeRequest
{
    public string? RegulatorEmail { get; set; }

    public string? UserEmail { get; set; }

    public string? UserOrganisationId { get; set; }

    public string? RegulatorResponse { get; set; }

    public string? RegulatorComments { get; set; }

    public IReadOnlyList<string> Validate()
    {
        var errors = new List<string>();

        AddRequiredError(errors, RegulatorEmail, nameof(RegulatorEmail));
        AddRequiredError(errors, UserEmail, nameof(UserEmail));
        AddRequiredError(errors, UserOrganisationId, nameof(UserOrganisationId));
        AddRequiredError(errors, RegulatorResponse, nameof(RegulatorResponse));
        AddEmailError(errors, RegulatorEmail, nameof(RegulatorEmail));
        AddEmailError(errors, UserEmail, nameof(UserEmail));

        if (!string.IsNullOrWhiteSpace(RegulatorResponse)
            && !RegulatorResponse.Equals("Accepted", StringComparison.OrdinalIgnoreCase)
            && !RegulatorResponse.Equals("Rejected", StringComparison.OrdinalIgnoreCase))
        {
            errors.Add("RegulatorResponse must be either Accepted or Rejected.");
        }

        if (RegulatorResponse?.Equals("Rejected", StringComparison.OrdinalIgnoreCase) == true
            && string.IsNullOrWhiteSpace(RegulatorComments))
        {
            errors.Add("RegulatorComments is required when RegulatorResponse is Rejected.");
        }

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
