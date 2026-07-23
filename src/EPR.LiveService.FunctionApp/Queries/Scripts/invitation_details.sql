WITH InviterDetails AS (
    SELECT
        u.Email AS InviterEmail,
        p.FirstName AS InviterFirstName,
        p.LastName AS InviterLastName,
        org.Name AS InviterOrgName,
        org.ReferenceNumber AS InviterOrgRef,
        org.OrganisationTypeId AS InviterOrgTypeId
    FROM dbo.Users u
    INNER JOIN dbo.Persons p
        ON p.UserId = u.Id
    INNER JOIN dbo.PersonOrganisationConnections poc
        ON poc.PersonId = p.Id
    INNER JOIN dbo.Organisations org
        ON org.Id = poc.OrganisationId
)

SELECT
    CONCAT(
        'https://www.notifications.service.gov.uk/services/9faf5afd-714a-4e59-93b6-bf9dc8d26196/templates/',
        CASE
            WHEN sr.[Key] = 'Regulator.Admin' THEN 'f2363374-9f0c-420e-a91d-b4af16ff9333'
            WHEN sr.[Key] = 'Regulator.Basic' THEN 'f2363374-9f0c-420e-a91d-b4af16ff9333'
            WHEN id.InviterOrgTypeId = 6 THEN 'c8d5bc5c-580a-4817-9bab-4fd26c0dabee'
            WHEN porgr.[Name] = 'Admin' THEN '958280bf-e77e-4940-ba37-74340c02e44d'
            WHEN porgr.[Name] = 'Employee' THEN '8a27f8b7-7022-489c-ac9e-208435ad1fac'
            ELSE NULL
        END
    ) AS TemplateLink,
    COALESCE(NULLIF(sr.[Key], ''), porgr.[Name]) AS RoleKey,
    p.Email AS InvitedUserEmail,
    id.InviterFirstName,
    id.InviterLastName,
    id.InviterEmail,
    id.InviterOrgName,
    p.FirstName,
    p.LastName,
    org.Name AS OrganisationName,
    org.ReferenceNumber AS OrgRef,
    es.[Name] AS EnrolmentStatus,
    e.EnrolmentStatusId,
    u.InviteToken,
    CONCAT(
        'http://report-packaging-data.defra.gov.uk/create-account/invitation/',
        REPLACE(u.InviteToken, '=', '%3D')
    ) AS InviteLink,
    e.IsDeleted AS EnrolmentDeleted,
    poc.IsDeleted AS ConnectionDeleted,
    p.IsDeleted AS PersonDeleted,
    u.IsDeleted AS UserDeleted
FROM dbo.Users u
INNER JOIN dbo.Persons p
    ON p.UserId = u.Id
INNER JOIN dbo.PersonOrganisationConnections poc
    ON poc.PersonId = p.Id
INNER JOIN dbo.Enrolments e
    ON e.ConnectionId = poc.Id
INNER JOIN dbo.EnrolmentStatuses es
    ON es.Id = e.EnrolmentStatusId
INNER JOIN dbo.ServiceRoles sr
    ON sr.Id = e.ServiceRoleId
INNER JOIN dbo.Organisations org
    ON org.Id = poc.OrganisationId
INNER JOIN dbo.PersonInOrganisationRoles porgr
    ON porgr.Id = poc.PersonRoleId
LEFT JOIN InviterDetails id
    ON id.InviterEmail = u.InvitedBy
WHERE u.Email = @Email
ORDER BY e.EnrolmentStatusId;
