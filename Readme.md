# Cosmos DB container sync app

## Overview

Uses change feed processor to sync up Azure Cosmos DB containers easily.

## Quick Start

1. Download the binary
1. Run:

    ```shell
    cosmossync.exe --interactive
    ```

1. Follow the prompt to finish the rest of it.

## Or if you prefer docker, use this:

1. Pull the image (for the first time)

    ```shell
    docker pull saars/cosmossync
    ```

1. Run it

    ```shell
    docker run -it saars/cosmossync
    ```

1. Follow the prompt to finish the rest of it.

1. Use `Ctrl + C` to quit at any point.

## Interactive mode

<details>
<summary>Expand to see the example.</summary>

Use command line parameter `--interactive`/`-i` to run it in interactive mode, where the database and container information will be gathered line by line.

```shell
PS D:\Tools>.\cosmossync.exe -i
========================================
Sync your CosmosDB containers with ease.
========================================
Interactive mode: True
Let's start with Source DB:
Source DB connection string:
```

Notices: CLI in docker container runs in interactive mode by default.

</details>

## By configuration file

<details>
<summary>Expand to see the example.</summary>

## Usage

1. Go to the binary folder.

2. Update `appesettings.json`, for example, update the values accordingly:

    ```jsonc
    {
        "SourceCosmosDB": {
            "ConnectionString": "Source DB Connection string",
            "DatabaseId": "Source Database Name"
        },
        "DestCosmosDB": {
            "ConnectionString": "Target DB Connection string",
            "DatabaseId": "Target Database Name"
        },
        "BackupOptions": {
            "SourceContainerName": "SourceContainerName",
            "DestContainerName": "TargetContainerName",
            "DestContainerPartitionKeyPath": "/id",
            "LeaseContainerName": "LeaseContainerName"
        }
    }
    ```

1. Run the app:

    ```shell
    ./cosmossync.exe
    ```

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

</details>