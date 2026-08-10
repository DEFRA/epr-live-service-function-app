using EPR.LiveService.FunctionApp.Configs;
using EPR.LiveService.FunctionApp.PendingChanges;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace EPR.LiveService.FunctionApp.Services;

public class OrganisationService(
    HttpClient httpClient,
    ApiEndpoints apiEndpoints) : IOrganisationService
{
    public async Task<string> UpdateOrganisationAsync(
        PendingChangeRegulatorDetails pendingChangeRegulatorDetails,
        bool hasRegulatorAccepted,
        string regulatorComment,
        string bearerToken,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"{apiEndpoints.RegulatorOrganisationApproval}{pendingChangeRegulatorDetails.ChangeHistoryExternalId}")
            {
                Content = JsonContent.Create(
                    new {
                        regulatorComment,
                        hasRegulatorAccepted
                    })
            };

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                RemoveBearerPrefix(bearerToken));

        request.Headers.Add(
            "X-EPR-User",
            pendingChangeRegulatorDetails.XEprUser.ToString());

        request.Headers.Add(
            "X-EPR-Organisation",
            pendingChangeRegulatorDetails.XEprOrganisation.ToString());

        using var response = await httpClient.SendAsync(
            request,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<string>(
            cancellationToken: cancellationToken))!;
    }

    private static string RemoveBearerPrefix(string token)
    {
        const string prefix = "Bearer ";

        return token.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
            ? token[prefix.Length..].Trim()
            : token.Trim();
    }
}
