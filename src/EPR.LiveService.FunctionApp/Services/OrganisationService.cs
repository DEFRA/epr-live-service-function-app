using EPR.LiveService.FunctionApp.Configs;
using EPR.LiveService.FunctionApp.UserDetailsChange;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace EPR.LiveService.FunctionApp.Services;

public class OrganisationService(
    HttpClient httpClient,
    ApiEndpoint apiEndpoints,
    ILogger<OrganisationService> logger) : IOrganisationService
{
    public async Task<OrganisationUpdateResponse> UpdateOrganisationAsync(
        RegulatorDetails regulatorDetails,
        bool hasRegulatorAccepted,
        string regulatorComment,
        string bearerToken,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await UpdateOrganisationCoreAsync(
                regulatorDetails,
                hasRegulatorAccepted,
                regulatorComment,
                bearerToken,
                cancellationToken);
        }
        catch (OperationCanceledException exception) when (cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(
                exception,
                "The organisation service POST request was cancelled for change history {ChangeHistoryExternalId}.",
                regulatorDetails.ChangeHistoryExternalId);

            throw;
        }
        catch (OrganisationServiceException exception)
        {
            logger.LogError(
                exception,
                "The organisation service POST request failed for change history {ChangeHistoryExternalId} with status {StatusCode}.",
                regulatorDetails.ChangeHistoryExternalId,
                exception.StatusCode);

            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "The organisation service POST request failed for change history {ChangeHistoryExternalId}.",
                regulatorDetails.ChangeHistoryExternalId);

            throw;
        }
    }

    private async Task<OrganisationUpdateResponse> UpdateOrganisationCoreAsync(
        RegulatorDetails regulatorDetails,
        bool hasRegulatorAccepted,
        string regulatorComment,
        string bearerToken,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"{apiEndpoints.RegulatorOrganisationApproval}{regulatorDetails.ChangeHistoryExternalId}")
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
            regulatorDetails.XEprUser.ToString());

        request.Headers.Add(
            "X-EPR-Organisation",
            regulatorDetails.XEprOrganisation.ToString());

        HttpResponseMessage response;

        try
        {
            response = await httpClient.SendAsync(
                request,
                cancellationToken);
        }
        catch (OperationCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            throw new OrganisationServiceException(
                "The organisation service request timed out.",
                innerException: exception);
        }
        catch (HttpRequestException exception)
        {
            throw new OrganisationServiceException(
                "The organisation service request could not be completed.",
                exception.StatusCode,
                exception);
        }

        using (response)
        {
            if (!response.IsSuccessStatusCode)
            {
                await ThrowForUnsuccessfulResponseAsync(
                    response,
                    regulatorDetails.ChangeHistoryExternalId,
                    cancellationToken);
            }

            try
            {
                return (await response.Content.ReadFromJsonAsync<OrganisationUpdateResponse>(
                    cancellationToken: cancellationToken))
                    ?? throw new JsonException("The response body was empty.");
            }
            catch (JsonException exception)
            {
                throw new OrganisationServiceException(
                    "The organisation service returned an invalid response.",
                    response.StatusCode,
                    exception);
            }
        }
    }

    private async Task ThrowForUnsuccessfulResponseAsync(
        HttpResponseMessage response,
        Guid changeHistoryExternalId,
        CancellationToken cancellationToken)
    {
        _ = await response.Content.ReadAsStringAsync(cancellationToken);

        throw new OrganisationServiceException(
            GetFailureMessage(response.StatusCode),
            response.StatusCode);
    }

    private static string GetFailureMessage(HttpStatusCode statusCode) => statusCode switch
    {
        HttpStatusCode.BadRequest => "The organisation service rejected the request.",
        HttpStatusCode.Unauthorized => "The organisation service did not accept the supplied credentials.",
        HttpStatusCode.Forbidden => "The organisation service denied access to the requested operation.",
        HttpStatusCode.NotFound => "The organisation or user details change was not found.",
        HttpStatusCode.Conflict => "The organisation update conflicts with its current state.",
        HttpStatusCode.TooManyRequests => "The organisation service rate limit was exceeded.",
        _ when (statusCode >= HttpStatusCode.InternalServerError) =>
            "The organisation service encountered an error.",
        _ => "The organisation service request was unsuccessful."
    };

    private static string RemoveBearerPrefix(string token)
    {
        const string prefix = "Bearer ";

        return token.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
            ? token[prefix.Length..].Trim()
            : token.Trim();
    }
}
