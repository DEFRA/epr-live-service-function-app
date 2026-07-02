# EPR Live Services Toolkit Functions

## Overview

These functions provide streamlined access to frequently required queries run against production data.

## How To Run

### Prerequisites
In order to run the service you will need the following dependencies:

- .NET 10
- Azure CLI

### developer configuration

TODO


### Run

TODO

Go to `src/EPR.LiveServices.FunctionApp` directory and execute:

```
func start
```

### Docker

TODO

Run in terminal at the solution source root:

```
docker build -t producervalidation -f EPR.LiveService.FunctionApp/Dockerfile . 
```

Fill out the environment variables and run the following command:
```
docker run -e AzureWebJobsStorage="X" -e FUNCTIONS_EXTENSION_VERSION="X" -e FUNCTIONS_WORKER_RUNTIME="X" -e Redis:ConnectionString="X" -e ServiceBus:ConnectionString="X" -e ServiceBus:SplitQueueName="X" -e StorageAccount:PomContainer="X" -e SubmissionApi:BaseUrl="X" -e Validation:Disabled="X" -e Validation:MaxIssuesToProces="X" producervalidation
```

## How To Test

### Unit tests

On root directory `src`, execute:

```
dotnet test
```

### Pact tests

N/A

### Integration tests

N/A

## How To Debug

Use debugging tools in your chosen IDE.

## Environment Variables - deployed environments

TODO


### Monitoring and Health Check
Enable Health Check in the Azure portal and set the URL path to `TODO`

## Directory Structure

### Source files

- `EPR.LiveService.FunctionApp`- Function .NET source files


## Contributing to this project

Please read the [contribution guidelines](CONTRIBUTING.md) before submitting a pull request.

## Licence

[Licence information](LICENCE.md).