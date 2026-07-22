using System.Net.Mail;

namespace EPR.LiveService.FunctionApp.Notifications;

public class ResendEprPackagingRequest
{
    public string? EmailAddress { get; set; }

    public string? OrganisationName { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? JoinTheTeamLink { get; set; }

    public IReadOnlyList<string> Validate()
    {
        var errors = new List<string>();

        AddRequiredError(errors, EmailAddress, nameof(EmailAddress));
        AddRequiredError(errors, OrganisationName, nameof(OrganisationName));
        AddRequiredError(errors, FirstName, nameof(FirstName));
        AddRequiredError(errors, LastName, nameof(LastName));
        AddRequiredError(errors, JoinTheTeamLink, nameof(JoinTheTeamLink));

        if (!string.IsNullOrWhiteSpace(EmailAddress)
            && !MailAddress.TryCreate(EmailAddress, out _))
        {
            errors.Add("EmailAddress must be a valid email address.");
        }

        if (!string.IsNullOrWhiteSpace(JoinTheTeamLink)
            && (!Uri.TryCreate(JoinTheTeamLink, UriKind.Absolute, out var uri)
                || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)))
        {
            errors.Add("JoinTheTeamLink must be an absolute HTTP or HTTPS URL.");
        }

        return errors;
    }

    public Dictionary<string, dynamic> ToPersonalisation() => new()
    {
        [nameof(OrganisationName)] = OrganisationName!,
        [nameof(FirstName)] = FirstName!,
        [nameof(LastName)] = LastName!,
        [nameof(JoinTheTeamLink)] = JoinTheTeamLink!
    };

    private static void AddRequiredError(List<string> errors, string? value, string name)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors.Add($"{name} is required.");
        }
    }
}
