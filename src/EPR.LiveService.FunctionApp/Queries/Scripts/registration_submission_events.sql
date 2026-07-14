SELECT 
  org.Name AS OrganisationName,
  subEv.[type] AS Type,
  subEv.Decision,
  subEv.Comments,
  SubmissionDate AS SubmissionDate,
  subEv.Created AS Timestamp,
  sub.SubmissionId,
  sub.SubmissionType,
  sub.SubmissionPeriod,
  us.Email AS Email,
  subEv.load_ts
FROM rpd.Organisations org 
  LEFT JOIN rpd.[Submissions] sub on sub.OrganisationId = org.ExternalId
  LEFT JOIN [rpd].[SubmissionEvents] subEv on subEv.submissionid=sub.submissionid
  LEFT JOIN rpd.users us on us.userid= subEv.userid
WHERE 
  org.ReferenceNumber = @ReferenceNumber
  and sub.SubmissionPeriod LIKE '%@SubmissionYear'
  and sub.SubmissionType ='Registration'
ORDER BY subEv.Created DESC