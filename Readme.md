# Cosmos DB container sync app
[![CI](https://github.com/xiaomi7732/CosmosSync/actions/workflows/ci.yml/badge.svg)](https://github.com/xiaomi7732/CosmosSync/actions/workflows/ci.yml)

## Overview

Uses change feed processor to sync up Azure Cosmos DB containers easily. This is part of [CodeWithSaar](https://github.com/xiaomi7732/CodeWithSaar). If you find it useful, please star this repo.

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

</details>

## Logs

When it is running, you will see logs like this

```shell
========================================
Sync your CosmosDB containers with ease.
========================================
Interactive mode: False
info: CodeWithSaar.CosmosDBSync.CLI.ChangeFeedProcessorManager[0] Starting backup service.
info: CodeWithSaar.CosmosDBSync.CLI.ChangeFeedProcessorManager[0] Checking source database TestDB
info: CodeWithSaar.CosmosDBSync.CLI.ChangeFeedProcessorManager[0] Checking source container TestContainer
info: CodeWithSaar.CosmosDBSync.CLI.ChangeFeedProcessorManager[0] Prepare the destination database
info: CodeWithSaar.CosmosDBSync.CLI.ChangeFeedProcessorManager[0] Target database TestDB exists.
info: CodeWithSaar.CosmosDBSync.CLI.ChangeFeedProcessorManager[0] Prepare target container TargetContainer
info: CodeWithSaar.CosmosDBSync.CLI.ChangeFeedProcessorManager[0] Target container TargetContainer created
info: CodeWithSaar.CosmosDBSync.CLI.ChangeFeedProcessorManager[0] Prepare lease container TargetContainerLease...
info: CodeWithSaar.CosmosDBSync.CLI.ChangeFeedProcessorManager[0] Lease container TargetContainerLease created.
info: CodeWithSaar.CosmosDBSync.CLI.ChangeFeedProcessorManager[0] Starting Change Feed Processor...
info: CodeWithSaar.CosmosDBSync.CLI.ChangeFeedProcessorManager[0] Watching the change feed ...
info: CodeWithSaar.CosmosDBSync.CLI.ChangeFeedProcessorManager[0] Started handling a batch of changes for lease 0...
info: CodeWithSaar.CosmosDBSync.CLI.ChangeFeedProcessorManager[0] Change Feed request consumed 1 RU.
info: CodeWithSaar.CosmosDBSync.CLI.ChangeFeedProcessorManager[0] SessionToken $0:-1#12
info: CodeWithSaar.CosmosDBSync.CLI.ChangeFeedProcessorManager[0]                                     From => To
info: CodeWithSaar.CosmosDBSync.CLI.ChangeFeedProcessorManager[0]     ebff9be1-1921-4cb4-b4a0-17dea07eed5c => ebff9be1-1921-4cb4-b4a0-17dea07eed5c
info: CodeWithSaar.CosmosDBSync.CLI.ChangeFeedProcessorManager[0]     b16ab8c1-fdbd-41c4-83f8-b8c3f5ae5e37 => b16ab8c1-fdbd-41c4-83f8-b8c3f5ae5e37
info: CodeWithSaar.CosmosDBSync.CLI.ChangeFeedProcessorManager[0]     3baaca3b-be80-4a12-b743-f853c0d545e3 => 3baaca3b-be80-4a12-b743-f853c0d545e3
info: CodeWithSaar.CosmosDBSync.CLI.ChangeFeedProcessorManager[0]     464a7173-59df-4074-b17f-bdea38b2ec8b => 464a7173-59df-4074-b17f-bdea38b2ec8b
info: CodeWithSaar.CosmosDBSync.CLI.ChangeFeedProcessorManager[0]     27d38ff4-8b0d-4018-91f7-131357693be8 => 27d38ff4-8b0d-4018-91f7-131357693be8
info: CodeWithSaar.CosmosDBSync.CLI.ChangeFeedProcessorManager[0]     e4f375b9-ff75-4414-a972-7a7f52e032ea => e4f375b9-ff75-4414-a972-7a7f52e032ea
info: CodeWithSaar.CosmosDBSync.CLI.ChangeFeedProcessorManager[0]     72383ac7-d6f4-4a5e-bf56-b172f2fdafb2 => 72383ac7-d6f4-4a5e-bf56-b172f2fdafb2
info: CodeWithSaar.CosmosDBSync.CLI.ChangeFeedProcessorManager[0]     098d478a-cec4-474a-8b48-30861a575d1b => 098d478a-cec4-474a-8b48-30861a575d1b
info: CodeWithSaar.CosmosDBSync.CLI.ChangeFeedProcessorManager[0]     f2c79f41-806e-4657-83c4-0ff160c60b24 => f2c79f41-806e-4657-83c4-0ff160c60b24
info: CodeWithSaar.CosmosDBSync.CLI.ChangeFeedProcessorManager[0]     bbe41737-1ade-44df-8e33-217f11b8b452 => bbe41737-1ade-44df-8e33-217f11b8b452
info: CodeWithSaar.CosmosDBSync.CLI.ChangeFeedProcessorManager[0]     14fe365f-fef8-44d1-b828-86f581261142 => 14fe365f-fef8-44d1-b828-86f581261142
info: CodeWithSaar.CosmosDBSync.CLI.ChangeFeedProcessorManager[0] Finished handling a batch of changes.
```