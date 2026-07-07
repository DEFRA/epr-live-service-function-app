using Dapper;
using EPR.LiveService.FunctionApp.Models;
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

    public async Task<GetOrganisationByOrgRefResults?> GetOrganisationByOrgRefAsync(string orgRef)
    {
        var sql = @"
            SELECT *
            FROM Organisations
            WHERE OrgRef = @OrgRef";

        var parameters = new { OrgRef = orgRef };

        var getOrganisationByOrgRefResults = await WithConnectionAsync((connection, transaction) =>
            connection.QuerySingleAsync<GetOrganisationByOrgRefResults>(sql, parameters, transaction));

        return getOrganisationByOrgRefResults;
    }
}
