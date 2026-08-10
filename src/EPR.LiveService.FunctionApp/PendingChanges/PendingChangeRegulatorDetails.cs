namespace EPR.LiveService.FunctionApp.PendingChanges;

public class PendingChangeRegulatorDetails
{
    public Guid XEprUser { get; set; }

    public Guid XEprOrganisation { get; set; }

    public Guid ChangeHistoryExternalId { get; set; }
}
