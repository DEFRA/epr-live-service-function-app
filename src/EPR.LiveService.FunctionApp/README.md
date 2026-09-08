# epr-live-service-function-app

## Query result limit

Set `QueryResults__MaxRows` in the Function App's application settings to control
the maximum number of rows returned by any query (for example, `250`). The default
is `1000` when omitted. For local development, add the same key under `Values` in
`local.settings.json`.

The setting is read at startup. After changing it in production, restart the
Function App if the settings update has not already restarted it; no application
redeployment is required. Invalid values fail startup: the value must be an
integer between `1` and `2147483646`, leaving room for one overflow-detection row.
Queries exceeding the configured limit return HTTP 400 with a request to narrow
the search, without returning partial results.

## GOV.UK Notify configuration

The **Re-send Invitation Email** feature uses
template `958280bf-e77e-4940-ba37-74340c02e44d` and requires the following
configuration setting:

```text
GovUkNotify__ApiKey=<GOV.UK Notify API key>
```

Use this double-underscore key both as a deployed environment variable and under
`Values` in `local.settings.json` for local development. .NET configuration maps
the double underscore to `GovUkNotify:ApiKey`.

## API endpoint configuration

The regulator organisation approval endpoint is configured using:

```text
ApiEndpoint__RegulatorOrganisationApproval=api/regulators/regulator-organisation/approval/
```

The value must include the trailing slash because the change-history identifier
is appended to it when the request is created.

`ApiConfig__OrganisationServiceBaseUrl`,
`ApiConfig__OrganisationServiceClientId`,
`ApiConfig__Timeout`, and `ApiEndpoint__RegulatorOrganisationApproval` are all
required. The application validates them during startup, and `Timeout` must be a
positive integer.
