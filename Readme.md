# CADev Cosmos DB container back app

## Overview

Uses change feed processor to backup containers.

## Usage

1. Create a target Cosmos DB account, assuming a db named `ChangeAnalysisData`.

1. Create a secret.json like this under `/src/CADevBackup.ChangeFeedProcessor`

    ```jsonc
    {
        "SourceCosmosDB": {
            "DatabaseUri": "https://??source??.documents.azure.com:443/",
            "DatabaseKey": "???",
            "DatabaseId": "ChangeAnalysisData"
        },
        "DestCosmosDB": {
            "DatabaseUri": "https://??dest??.documents.azure.com:443/",
            "DatabaseKey": "????",
            "DatabaseId": "ChangeAnalysisData"
        }
    }
    ```

1. Apply the secrets:

    ```shell
    type ./secrets.json | dotnet user-secrets set
    ```

1. Update [./src/CADevBackup.ChangeFeedProcessor/appsettings.jsonc](./src/CADevBackup.ChangeFeedProcessor/appsettings.jsonc) for Backup options.

1. Run the app.

    Logs look like this:

    ```shell
    info: CADevBackup.ChangeFeedProcessing.ChangeFeedProcessorManager[0] Starting Change Feed Processor...
    info: CADevBackup.ChangeFeedProcessing.ChangeFeedProcessorManager[0] Change Feed Processor started.
    info: CADevBackup.ChangeFeedProcessing.ChangeFeedProcessorManager[0] Started handling changes for lease 0...
    info: CADevBackup.ChangeFeedProcessing.ChangeFeedProcessorManager[0] Change Feed request consumed 1 RU.
    info: CADevBackup.ChangeFeedProcessing.ChangeFeedProcessorManager[0] SessionToken $0:6#1132055#2=468290
    warn: CADevBackup.ChangeFeedProcessing.ChangeFeedProcessorManager[0] New target container. Is this expected?
    info: CADevBackup.ChangeFeedProcessing.ChangeFeedProcessorManager[0] 464a7173-59df-4074-b17f-bdea38b2ec8b => 464a7173-59df-4074-b17f-bdea38b2ec8b
    info: CADevBackup.ChangeFeedProcessing.ChangeFeedProcessorManager[0] Finished handling changes.
    info: CADevBackup.ChangeFeedProcessing.ChangeFeedProcessorManager[0] Started handling changes for lease 0...
    info: CADevBackup.ChangeFeedProcessing.ChangeFeedProcessorManager[0] Change Feed request consumed 1 RU.
    info: CADevBackup.ChangeFeedProcessing.ChangeFeedProcessorManager[0] SessionToken $0:6#1132056#2=468290
    info: CADevBackup.ChangeFeedProcessing.ChangeFeedProcessorManager[0] 464a7173-59df-4074-b17f-bdea38b2ec8b => 464a7173-59df-4074-b17f-bdea38b2ec8b
    info: CADevBackup.ChangeFeedProcessing.ChangeFeedProcessorManager[0] Finished handling changes.
    ```