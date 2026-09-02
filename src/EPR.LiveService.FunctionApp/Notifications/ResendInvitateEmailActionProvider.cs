using EPR.LiveService.FunctionApp.Formatting;

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

        var fields = record
            .Where(field => FieldMappings.ContainsKey(field.Key) && field.Value is string)
            .Select(field => new QueryResultActionField(FieldMappings[field.Key], (string)field.Value))
            .ToList();
        
        return
        [
            new QueryResultAction("Re-send invitation email", ResendPath, fields)
        ];
    }
}
