namespace EPR.LiveService.FunctionApp.Models;

public class GetOrganisationByOrgRefResults
{
    public string PersonId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string PersonRoleId { get; set; }
    public string PersonOC_Id { get; set; }
    public string OrganisationId { get; set; }
    public string ReferenceNumber { get; set; }
    public Guid OrgExternalId { get; set; }
    public string Name { get; set; }
    public string Nation { get; set; }
    public string PersonRoleName { get; set; }
    public string ServiceRoleId { get; set; }
    public string EnrolmentId { get; set; }
    public string ServiceRoleName { get; set; }
    public string ServiceRoleKey { get; set; }
    public string EnrolmentStatusId { get; set; }
    public string EnrolmentStatus { get; set; }
    public Guid UserId { get; set; }
    public string IsEnrolmentDeleted { get; set; }
    public string IsUserDeleted { get; set; }
    public string IsPersonDeleted { get; set; }
    public string InvitedBy { get; set; }

}