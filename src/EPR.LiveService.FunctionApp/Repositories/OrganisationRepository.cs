using Dapper;
using EPR.LiveService.FunctionApp.Repositories.Interfaces;
using EPR.LiveService.Functions.Repositories;

namespace EPR.LiveService.FunctionApp.Repositories;

public class OrganisationRepository : RepositoryBase, IOrganisationRepository
{
    public OrganisationRepository(
        ConnectionStrings connectionStrings,
        IUnitOfWork? uow = null) : base(connectionStrings, uow)
    {
    }

    public async Task<IEnumerable<dynamic>> GetOrganisationByOrgRefAsync(string referenceNumber)
    {
        var parameters = new 
        { 
            ReferenceNumber = referenceNumber 
        };

        var sql = @"
            SELECT
                p.Id as PersonId,
                p.FirstName,
                p.LastName,
                p.Email,
                pocs.PersonRoleId,
                pocs.Id AS PersonOC_Id,
                org.Id AS OrganisationId,
                org.ReferenceNumber,
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
              INNER JOIN Organisations org ON pocs.OrganisationId = org.Id
              INNER JOIN PersonInOrganisationRoles porgr ON porgr.Id = pocs.PersonRoleId
              INNER JOIN Enrolments e ON pocs.Id = e.ConnectionId
              INNER JOIN EnrolmentStatuses es ON e.EnrolmentStatusId = es.Id
              INNER JOIN ServiceRoles sr ON sr.Id = e.ServiceRoleId
              INNER JOIN[Services] s ON  s.Id = sr.ServiceId
              INNER JOIN Users u ON u.Id = p.UserId
              INNER JOIN Nations nat ON nat.Id = org.NationId
            WHERE
              org.ReferenceNumber = @ReferenceNumber
            ORDER BY org.Name";

        var getOrganisationByOrgRefResults = await WithConnectionAsync((connection, transaction) =>
            connection.QueryAsync<dynamic>(sql, parameters, transaction));

        return getOrganisationByOrgRefResults.ToList();
    }
}
