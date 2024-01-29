# Cheetah Nuget Packages

This repository is a mono-repo containing all of the NuGet packages offered as part of the Cheetah data platform.

## Project structure

The project is structured as follows:
- `/.github` - Contains GitHub actions pipelines used to build, test, release, etc.
- `/docs` - Contains all publicly visible documentation.
- `/src` - Contains all source code. The root contains `/Cheetah.Shared.sln`, which includes all of the projects in `/src`.
- `README.md` - This file, contains internal developer documentation.

## Prerequisites
- [cheetah-development-infrastructure](https://github.com/trifork/cheetah-development-infrastructure) is cloned and running.

## Developing locally

For each major package, there is an associated example project:
- `Cheetah.OpenSearch` -> `Cheetah.OpenSearch.ExampleAPI`
- `Cheetah.Kafka` -> `Cheetah.Kafka.ExampleProcessor`

These example projects are not built and run as a part of any pipeline, but are intended to be easy-to-alter playgrounds which can quickly be run to verify new changes locally.

Functionality should of course secured through proper tests, but the example projects provide a way to easily test out new functionality as a "regular" service, which uses the package.

### Running tests

Most projects in the repository contains an associateed `.Test` project. These tests are and should be designed to run directly from an IDE or `dotnet test` without any alterations, given that development infrastructure is running with the necessary services.

A consequence of this is that all integration tests, that use services from development infrastructure, should always default to using `localhost` exposed ports. Ideally the configuration of these tests should also be possible to override through environment variables, so that we may change it in other environments.

### Building and serving docs

The publicly facing documentation, which gets included in [cheetah-artifact-documentation](https://github.com/trifork/cheetah-artifact-documentation) and served at [Cheetah Data Platform Documentation](http://docs.cheetah.trifork.dev) is found in `/docs`.

We use docfx to compile and build documentation, which, assuming docfx is installed, can be built and served locally by running the following command from the `/docs` directory:

```sh
docfx docfx.json --serve
```

This will compile all articles into html and build api documentation based on XML docs in the code. The locally hosted docfx is served at [http://localhost:8080](http://localhost:8080)

## Releasing a new version

[Run actions from here](https://github.com/trifork/cheetah-lib-shared-dotnet/actions)

In order to create a new release, you must first create a new release branch.

This can be done by running the `.NET Create Release Branch` action and specifying which package you want to release, as well as the version increment to perform on main.

Each release branch is responsible for a minor version of a single package, e.g. `release/Cheetah.Kafka-v0.2` is responsible for releasing `Cheetah.Kafka` in versions `0.2.X`, starting with `0.2.0`.

Once the release branch is successfully created, run the `.NET Create Release` branch from the release branch. It will automatically build, test and release the new version of the package.

### Releasing a patch

Releasing a patch of an existing release is a straight-forward process. Simply make your changes onto the relevant release branch, then run the `.NET Create Release` action manually on the release branch

Make sure to merge your changes back onto main and any release branches with higher versions to that fixes are applied to all relevant version and to main.

### Note on version bumps

Be aware that when you create a release branch, you create it based on the _current_ `VersionPrefix` value in the project's .csproj on `main`. This means that the version increment you select doesn't determine the version that you're about to release, but instead determines next bump.

This is usually not an issue when creating minor updates, but can cause issues when you're intending to release a new major, since the major bump needs to either be expected by the person who released previously, or you need to first create a release branch which bumps to the next major and then create another release branch which bumps to the following minor.

Imagine the following scenario:
- Cheetah.Kafka's newest version is 1.1, main has been minor bumped to 1.2
- We make breaking changes to Cheetah.Kafka on main
- We create a new release branch and ask it to do a major bump
- A new release branch is created called `release/Cheetah.Kafka-v1.2`

At this point, if we create a release from the new release branch, we end up releasing breaking changes in a minor increment. Instead:
- Delete `release/Cheetah.kafka-v1.2`
- Create another release branch, this time selecting to only do a minor bump
- A new release branch is created called `release/Cheetah.Kafka-v2.0`
- Create a release from the new release branch.

An alternate approach is to instead do a major bump before merging breaking changes to main. This also ensures that we do not accidentally release breaking changes in a minor by forgetting that someone has made unreleased breaking changes on main (This has happened before).

## Published Nuget packages

The following packages are published as NuGet packages to both the GitHub NuGet repository and customer NuGet repositories:
- Cheetah.Kafka
- Cheetah.OpenSearch

Any other projects that these two projects refer to are also implicitly published, since they will get baked into the NuGet package through the [`Fat-Pack` script](https://github.com/trifork/cheetah-infrastructure-utils/blob/main/.github/actions/dotnet/dotnet-fat-pack/Fat-Pack.ps1).

This currently only applies to `Cheetah.Auth` which is built-in to both the Kafka and OpenSearch packages.

Bear in mind that there are several packages in the solution that are _NOT_ published anywhere. These packages primarily contain functionality from before the restructuring into Cheetah.Kafka and Cheetah.OpenSearch.

At some point, that currently dead code should either be reintroduced as their own packages, migrated to one of the published packages or properly burried and deleted.

## Development Recommendations

### Be mindful of when to use `internal` instead of `public`

Anything that is public can and will be used and miused by users, make sure that it's intentional that they have access to it.

Using the `internal` keyword instead of `public` allows you to use it within the same project as if it was public, while denying access to it for anyone using the package.

### Test out the "feel"

When developing packages for other developers, the developer experience is important. Ideally you want others to be able to use the package without first needing to read an entire book's worth of documentation - It also saves you from having to write the book.

Ensure that new features and changes are intuitive to use and makes sense from a user perspective by trying it out using the example projects.

### Don't leave empty XML comments

During active development, it is tempting to accept the suggestion of bootstrapping XML docs when creating a new publicly visible method because of the red squiggly that screams that it is missing.

If you're not ready to write the docs immediately, leave the error there untill you are ready to write the docs. If you bootstrap it early with the intention of coming back to it later, you *will* at some point forget to fill it in, and you no longer have an error to warn you.

### Be careful with wrappers

It is sometimes tempting to gain control over external packages by wrapping their functionality in a wrapper class of some sorts. This has proven to often cause more issues than benefits, since it leaves us with the responsibility of implementing and documenting wrapping logic for everything that whatever we wrap can do.

Whenever possible, avoid wrapping and prefer using either extension methods to expand functionality or factories to simplify creation of objects that require complex initialization.