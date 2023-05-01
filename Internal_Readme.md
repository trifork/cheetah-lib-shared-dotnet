# Cheetah.Shared (Documentation for developers)

## Making releases

Edit `<VersionPrefix>` and `ReleaseNotes.md` to create a new release.
Any new version registered in `origin/main` will result in a release + release notes with a tag

## Running tests locally

To run unit tests locally run:

```sh
dotnet test --filter 'FullyQualifiedName!~Integration'
```

To run unit tests locally you need OpenSearch running on port 9200.
To run integration tests locally run:
```sh
dotnet test --filter 'FullyQualifiedName~Integration'
```

## Writing tests

Writing unit tests has nothing specific to it.

Writing integration tests has the following requirements:

- The test function must contain 'Integration' in its name (usually as a suffix)
- The test function must set-up its own data and tear it down afterwards
- Integration tests for OpenSearch require an OS instance running on port 9200


## Formatting

```sh
dotnet format
```

```sh
docker run --rm -e APPLY_FIXES="true" -e PRINT_ALPACA="false" -e ENABLE_LINTERS="DOCKERFILE_HADOLINT,MARKDOWN_MARKDOWNLINT,BASH_EXEC,YAML_YAMLLINT,ENV_DOTENV_LINTER,CSHARP_DOTNET_FORMAT" -e DEFAULT_WORKSPACE="/code" -v "$PWD:/code" megalinter/megalinter-dotnet:v5
```
