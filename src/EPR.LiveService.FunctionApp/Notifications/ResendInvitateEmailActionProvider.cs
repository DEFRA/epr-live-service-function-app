using EPR.LiveService.FunctionApp.Formatting;
using Microsoft.AspNetCore.WebUtilities;

namespace EPR.LiveService.FunctionApp.Notifications;

public class ResendInvitateEmailActionProvider : IQueryResultActionProvider
{
    private const string InvitationDetailsQueryId = "invitation_details";
    private const string ResendPath = "/api/resend-invite-email";

    private static readonly IReadOnlyDictionary<string, string> FieldMappings =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["InvitedUserEmail"] = "EmailAddress",
            ["OrganisationName"] = "OrganisationName",
            ["FirstName"] = "FirstName",
            ["LastName"] = "LastName",
            ["InviteLink"] = "JoinTheTeamLink"
        };

    public IEnumerable<QueryResultAction> GetActions(
        string queryId,
        IReadOnlyDictionary<string, object> record)
    {
        if (!string.Equals(queryId, InvitationDetailsQueryId, StringComparison.OrdinalIgnoreCase))
        {
            return [];
        }

        var parameters = record
            .Where(field => FieldMappings.ContainsKey(field.Key) && field.Value is string)
            .ToDictionary(
                field => FieldMappings[field.Key],
                field => (string?)field.Value,
                StringComparer.OrdinalIgnoreCase);

        return
        [
            new QueryResultAction(
                "Re-send invitation email",
                QueryHelpers.AddQueryString(ResendPath, parameters))
        ];
    }
}
