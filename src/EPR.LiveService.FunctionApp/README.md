# epr-live-service-function-app

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

## Local authentication

When the Functions environment is `Development`, the app simulates the
`X-MS-CLIENT-PRINCIPAL` header normally supplied by Azure App Service Easy Auth.
The simulated user belongs to the expected tenant and has the `user` role, so
authenticated endpoints such as `GET /api/me` can be exercised locally.

The middleware does not replace a header supplied by the caller and is not
registered in non-development environments.
