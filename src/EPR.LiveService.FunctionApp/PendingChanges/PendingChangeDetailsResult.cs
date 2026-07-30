namespace EPR.LiveService.FunctionApp.PendingChanges;

public class PendingChangeDetailsResult
{
    public Guid XEprUser { get; set; }

    public Guid XEprOrganisation { get; set; }

    public Guid ChangeHistoryExternalId { get; set; }
}
