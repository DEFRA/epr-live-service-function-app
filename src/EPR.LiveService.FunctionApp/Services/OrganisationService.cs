using EPR.LiveService.FunctionApp.Configs;
using EPR.LiveService.FunctionApp.UserDetailsChange;
using Microsoft.Extensions.Logging;
using System.Net;
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
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await UpdateOrganisationCoreAsync(
                regulatorDetails,
                hasRegulatorAccepted,
                regulatorComment,
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

        request.Headers.Add(
            "X-EPR-User",
            regulatorDetails.XEprUser.ToString());

        request.Headers.Add(
            "X-EPR-Organisation",
            regulatorDetails.XEprOrganisation.ToString());

        var jsonPayload = await request.Content.ReadAsStringAsync(cancellationToken);
        logger.LogInformation(
            "Sending organisation update JSON payload: {JsonPayload}",
            jsonPayload);

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
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            logger.LogInformation(
                "Organisation update response with status {StatusCode}: {ResponseContent}",
                (int)response.StatusCode,
                responseContent);

            if (!response.IsSuccessStatusCode)
            {
                ThrowForUnsuccessfulResponse(response);
            }

            try
            {
                return JsonSerializer.Deserialize<OrganisationUpdateResponse>(
                    responseContent,
                    JsonSerializerOptions.Web)
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

    private static void ThrowForUnsuccessfulResponse(HttpResponseMessage response)
    {
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
}
