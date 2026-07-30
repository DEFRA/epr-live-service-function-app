SELECT UserId, Email
FROM dbo.Users
WHERE
  (@MatchType = 'equals'   AND Email = @Email)
  OR (@MatchType = 'contains' AND Email LIKE '%' + @Email + '%')