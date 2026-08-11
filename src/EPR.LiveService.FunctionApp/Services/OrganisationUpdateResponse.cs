namespace EPR.LiveService.FunctionApp.Services;

public class OrganisationUpdateResponse
{
    public OrganisationChangeHistory ChangeHistory { get; set; } = null!;

    public bool HasUserDetailsChangeAccepted { get; set; }

    public bool HasUserDetailsChangeRejected { get; set; }
}

public class OrganisationChangeHistory
{
    public int ApprovedById { get; set; }

    public string ApproverComments { get; set; } = null!;

    public OrganisationBusinessAddress BusinessAddress { get; set; } = null!;

    public string CompaniesHouseNumber { get; set; } = null!;

    public DateTimeOffset CreatedOn { get; set; }

    public DateTimeOffset DecisionDate { get; set; }

    public DateTimeOffset DeclarationDate { get; set; }

    public string EmailAddress { get; set; } = null!;

    public Guid ExternalId { get; set; }

    public int Id { get; set; }

    public bool IsActive { get; set; }

    public DateTimeOffset LastUpdatedOn { get; set; }

    public string Nation { get; set; } = null!;

    public OrganisationPersonDetails NewValues { get; set; } = null!;

    public OrganisationPersonDetails OldValues { get; set; } = null!;

    public int OrganisationId { get; set; }

    public string OrganisationName { get; set; } = null!;

    public string OrganisationReferenceNumber { get; set; } = null!;

    public string OrganisationType { get; set; } = null!;

    public int PersonId { get; set; }

    public string ServiceRole { get; set; } = null!;

    public string Telephone { get; set; } = null!;
}

public class OrganisationBusinessAddress
{
    public string? BuildingName { get; set; }

    public string? BuildingNumber { get; set; }

    public string Country { get; set; } = null!;

    public string? County { get; set; }

    public string? DependentLocality { get; set; }

    public string? Locality { get; set; }

    public string Postcode { get; set; } = null!;

    public string Street { get; set; } = null!;

    public string? SubBuildingName { get; set; }

    public string Town { get; set; } = null!;
}

public class OrganisationPersonDetails
{
    public string FirstName { get; set; } = null!;

    public string JobTitle { get; set; } = null!;

    public string LastName { get; set; } = null!;
}
