# Steps to release a docker container CLI

This is for the dev to release a container CLI.

## Build image locally

Assuming under `src`.

```shell
docker build -t cosmossync .
```

## Run it locally for debugging

```shell
docker run -it cosmossync
```

## Tag it before releasing

```shell
docker tag cosmossync saars/cosmossync
```

## Push the image

```shell
docker push saars/cosmossync
```