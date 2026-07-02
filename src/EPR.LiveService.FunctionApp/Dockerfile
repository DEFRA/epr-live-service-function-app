# Use the SDK image to build your application
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS installer-env

# Copy everything
COPY EPR.LiveService.FunctionApp/. ./EPR.LiveService.FunctionApp/.
COPY NuGet.Config ./
COPY stylecop.ruleset ./

# Use Build Argument to insert PAT into Nuget file
#ARG PAT=localhost
#RUN sed -i "s|</configuration>|<packageSourceCredentials><epr-packaging-common><add key=\"Username\" value=\"PAT\" /><add key=\"ClearTextPassword\" value=\"${PAT}\" /></epr-packaging-common></packageSourceCredentials></configuration>|" NuGet.Config

# Install Internal CA certificate
#COPY EPR.LiveService.FunctionApp/Certificates/internal-ca.crt /usr/local/share/ca-certificates/internal-ca.crt
#RUN chmod 644 /usr/local/share/ca-certificates/internal-ca.crt && update-ca-certificates

# Build the application
RUN dotnet publish EPR.LiveService.FunctionApp/*.csproj --output /home/site/wwwroot

# Use the Azure Functions isolated image as the final image
FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated10.0

# Create a non-root user and set permissions
RUN groupadd -r dotnet && \
    useradd -r -g dotnet dotnet && \
    chown -R dotnet /home

# Switch to the non-root user for the final image
USER dotnet

# Set environment variables
ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true

# Copy the published application from the installer-env image
COPY --from=installer-env ["/home/site/wwwroot", "/home/site/wwwroot"]