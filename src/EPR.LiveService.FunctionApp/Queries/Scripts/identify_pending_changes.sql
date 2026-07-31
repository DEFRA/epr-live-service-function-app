SELECT
    ch.DeclarationDate        AS ChangeDeclarationDate,
    n.Name                    AS Nation,
    ch.ExternalId             AS ChangeHistoryExternalId,
    ch.OldValues              AS ChangeFrom,
    ch.NewValues              AS ChangeTo,
    o.Name                    AS OrganisationName,
    o.ReferenceNumber         AS OrganisationReferenceNumber,
    o.CompaniesHouseNumber    AS CompaniesHouseNumber,
    u.Email                   AS UserEmail
FROM
    dbo.ChangeHistory ch
    INNER JOIN dbo.Organisations o ON o.Id = ch.OrganisationId
    INNER JOIN dbo.Nations n ON n.Id = o.NationId
    INNER JOIN dbo.Persons u ON u.Id = ch.PersonId
    INNER JOIN dbo.PersonOrganisationConnections poc
        ON poc.PersonId = ch.PersonId
        AND poc.OrganisationId = ch.OrganisationId
WHERE
    o.NationId = @NationId
    AND ch.DeclarationDate >= @DeclarationCutOffDate
    AND ch.DecisionDate IS NULL
    AND poc.IsDeleted = 0
    AND o.IsDeleted = 0
    AND u.IsDeleted = 0
ORDER BY
    ch.DeclarationDate DESC;
