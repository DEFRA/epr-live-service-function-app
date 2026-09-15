WITH RegulatorDetails AS
(
    SELECT
        u.UserId AS XEprUser,
        o.ExternalId AS XEprOrganisation
    FROM dbo.Users u
    INNER JOIN dbo.Persons p ON p.UserId = u.Id
    INNER JOIN dbo.PersonOrganisationConnections poc ON poc.PersonId = p.Id
    INNER JOIN dbo.Organisations o ON o.Id = poc.OrganisationId
    WHERE u.Email = @RegulatorEmail
      AND u.IsDeleted = 0
      AND o.IsDeleted = 0
      AND poc.IsDeleted = 0
),
LatestChangeHistory AS
(
    SELECT TOP (1)
        ch.ExternalId AS ChangeHistoryExternalId
    FROM dbo.Users u
    INNER JOIN dbo.Persons p ON p.UserId = u.Id
    INNER JOIN dbo.PersonOrganisationConnections poc ON poc.PersonId = p.Id
    INNER JOIN dbo.Organisations o ON o.Id = poc.OrganisationId
    INNER JOIN dbo.ChangeHistory ch
        ON ch.PersonId = p.Id
        AND ch.OrganisationId = o.Id
    WHERE u.Email = @UserEmail
      AND o.ReferenceNumber = @UserOrganisationId
      AND ch.IsActive = 1
      AND ch.DecisionDate IS NULL
      AND ch.IsDeleted = 0
      AND o.IsDeleted = 0
      AND poc.IsDeleted = 0
    ORDER BY ch.DeclarationDate DESC
)
SELECT
    regulator.XEprUser,
    regulator.XEprOrganisation,
    changeHistory.ChangeHistoryExternalId
FROM RegulatorDetails regulator
CROSS JOIN LatestChangeHistory changeHistory;
