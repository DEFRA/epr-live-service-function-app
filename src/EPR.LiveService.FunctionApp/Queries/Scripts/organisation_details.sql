-- All search parameters are optional; any supplied are combined with AND.
-- Name and Email match any part of the value (LIKE wildcards in the input are
-- escaped so they are treated literally). If no parameter is supplied the
-- query returns no rows rather than every organisation.
SELECT 
  p.Id as PersonId,
  p.FirstName,
  p.LastName,
  p.Email,
  pocs.PersonRoleId,
  pocs.Id  AS PersonOC_Id,
  org.Id AS OrganisationId,
  org.ReferenceNumber,
  org.CompaniesHouseNumber,
  org.ExternalId As OrgExternalId,
  org.Name,
  nat.Name AS Nation,
  porgr.[Name] AS PersonRoleName,
  e.ServiceRoleId,
  e.Id AS EnrolmentId,
  sr.[Name] AS ServiceRoleName,
  sr.[Key] AS ServiceRoleKey,
  es.Id AS EnrolmentStatusId,
  es.[Name] AS EnrolmentStatus,
  u.UserId as UserId,
  e.IsDeleted IsEnrolmentDeleted,
  u.IsDeleted as IsUserDeleted,
  p.IsDeleted AS IsPersonDeleted,
  u.InvitedBy
FROM [Persons] p
  INNER JOIN [PersonOrganisationConnections] pocs ON pocs.PersonId = p.Id
  INNER JOIN PersonInOrganisationRoles porgr ON porgr.Id = pocs.PersonRoleId
  INNER JOIN Organisations org ON pocs.OrganisationId = org.Id
  INNER JOIN Enrolments e ON pocs.Id = e.ConnectionId
  INNER JOIN EnrolmentStatuses es ON e.EnrolmentStatusId = es.Id
  INNER JOIN ServiceRoles sr ON sr.Id = e.ServiceRoleId
  INNER JOIN [Services] s ON  s.Id = sr.ServiceId
  INNER JOIN Users u ON u.Id = p.UserId
  INNER JOIN Nations nat ON nat.Id = org.NationId
WHERE
  (@ReferenceNumber IS NULL OR org.ReferenceNumber = @ReferenceNumber)
  AND (@CompaniesHouseNumber IS NULL OR org.CompaniesHouseNumber = @CompaniesHouseNumber)
  AND (
    @Name IS NULL
    OR org.Name LIKE '%' + REPLACE(REPLACE(REPLACE(REPLACE(@Name,
      '\', '\\'), '%', '\%'), '_', '\_'), '[', '\[') + '%' ESCAPE '\'
  )
  AND (
    @Email IS NULL
    OR p.Email LIKE '%' + REPLACE(REPLACE(REPLACE(REPLACE(@Email,
      '\', '\\'), '%', '\%'), '_', '\_'), '[', '\[') + '%' ESCAPE '\'
  )
  -- guard: refuse to run with nothing filled in
  AND (@ReferenceNumber IS NOT NULL OR @CompaniesHouseNumber IS NOT NULL OR @Name IS NOT NULL OR @Email IS NOT NULL)
ORDER BY org.Name
OPTION (RECOMPILE);
