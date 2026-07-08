SELECT
    al.Id,
    al.Timestamp,
    al.Entity,
    al.Operation,
    al.InternalId,
    al.ExternalId,
    -- Acting user
    u_actor.Email                                           AS ActingUserEmail,
    CONCAT(p_actor.FirstName, ' ', p_actor.LastName)        AS ActingUserName,
    CASE
        WHEN p_actor.Id IS NULL THEN 'System/Unknown'
        WHEN poc_actor.OrganisationId = o.Id THEN 'Internal (same org)'
        ELSE 'Regulator/External'
    END                                                     AS ActingUserContext,
    -- Subject person (extracted from JSON)
    CASE WHEN al.Entity = 'Person' THEN
        JSON_VALUE(COALESCE(al.NewValues, al.OldValues), '$.Email')
    END                                                     AS SubjectEmail,
    CASE WHEN al.Entity = 'Person' THEN
        NULLIF(CONCAT(
            JSON_VALUE(COALESCE(al.NewValues, al.OldValues), '$.FirstName'),
            ' ',
            JSON_VALUE(COALESCE(al.NewValues, al.OldValues), '$.LastName')
        ), ' ')
    END                                                     AS SubjectName,
    -- Enrolment detail
    CASE WHEN al.Entity = 'Enrolment' THEN
        es_new.Name
    END                                                     AS NewEnrolmentStatus,
    CASE WHEN al.Entity = 'Enrolment' THEN
        es_old.Name
    END                                                     AS OldEnrolmentStatus,
    CASE WHEN al.Entity = 'Enrolment' THEN
        sr.Name
    END                                                     AS ServiceRole,
    -- Connection detail
    CASE WHEN al.Entity = 'PersonOrganisationConnection' THEN
        porgr.Name
    END                                                     AS PersonRole,
    CASE WHEN al.Entity = 'PersonOrganisationConnection' THEN
        JSON_VALUE(COALESCE(al.NewValues, al.OldValues), '$.JobTitle')
    END                                                     AS JobTitle,
    -- Invite detail
    CASE WHEN al.Entity = 'User' THEN
        JSON_VALUE(al.NewValues, '$.Email')
    END                                                     AS InvitedEmail,
    CASE WHEN al.Entity = 'User' THEN
        JSON_VALUE(al.NewValues, '$.InvitedBy')
    END                                                     AS InvitedBy,
    -- Deletion changes
    CASE
        WHEN al.Changes LIKE '%IsDeleted%'
             AND JSON_VALUE(al.NewValues, '$.IsDeleted') = 'true'  THEN 'REMOVED'
        WHEN al.Changes LIKE '%IsDeleted%'
             AND JSON_VALUE(al.NewValues, '$.IsDeleted') = 'false' THEN 'RESTORED'
    END                                                     AS DeletionChange,
    al.Changes
FROM dbo.AuditLogs al
JOIN dbo.Organisations o
    ON o.ExternalId = al.OrganisationId
-- Acting user
LEFT JOIN dbo.Users u_actor
    ON u_actor.UserId = al.UserId
LEFT JOIN dbo.Persons p_actor
    ON p_actor.UserId = u_actor.Id
    AND p_actor.IsDeleted = 0
LEFT JOIN dbo.PersonOrganisationConnections poc_actor
    ON poc_actor.PersonId = p_actor.Id
    AND poc_actor.IsDeleted = 0
-- Enrolment status lookups (old and new)
LEFT JOIN dbo.EnrolmentStatuses es_new
    ON es_new.Id = TRY_CAST(JSON_VALUE(al.NewValues, '$.EnrolmentStatusId') AS INT)
LEFT JOIN dbo.EnrolmentStatuses es_old
    ON es_old.Id = TRY_CAST(JSON_VALUE(al.OldValues, '$.EnrolmentStatusId') AS INT)
-- Service role lookup
LEFT JOIN dbo.ServiceRoles sr
    ON sr.Id = TRY_CAST(JSON_VALUE(COALESCE(al.NewValues, al.OldValues), '$.ServiceRoleId') AS INT)
-- Person role lookup
LEFT JOIN dbo.PersonInOrganisationRoles porgr
    ON porgr.Id = TRY_CAST(
        JSON_VALUE(COALESCE(al.NewValues, al.OldValues), '$.PersonRoleId') AS INT
    )
WHERE o.ReferenceNumber = @ReferenceNumber
ORDER BY al.Timestamp DESC;