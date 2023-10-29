# [Choice] .NET version: 6.0-bullseye-slim, 6.0-jammy, 6.0-focal
FROM mcr.microsoft.com/devcontainers/dotnet:1-6.0-jammy@sha256:d52b18fa27b4e32e159d6e04edf27df728da5bfe5083804fd32e7fb006318ada

ENV DOCKER_BUILDKIT=1
ENV DOCKER_DEFAULT_PLATFORM=linux/amd64

# [Optional] Uncomment this section to install additional OS packages.
RUN apt-get update && export DEBIAN_FRONTEND=noninteractive \
     && apt-get -y install --no-install-recommends git curl exa


# Install Docker to use in docker-in-docker
RUN curl -fsSL https://get.docker.com -o get-docker.sh && sh get-docker.sh 
# Add user "admin" to the Docker group
#&& usermod -a -G docker admin
#ADD https://raw.githubusercontent.com/docker/docker-ce/master/components/cli/contrib/completion/bash/docker /etc/bash_completion.d/docker.sh

USER vscode
ARG GITHUB_ACTOR
ARG GITHUB_TOKEN

RUN dotnet nuget add source https://nuget.pkg.github.com/trifork/index.json -n trifork-github -u $GITHUB_ACTOR -p $GITHUB_TOKEN --store-password-in-clear-text
RUN dotnet tool update -g docfx

# Install ACT
#  ./bin/act -s GITHUB_TOKEN=${GITHUB_TOKEN} -s PACKAGE_PAT=${GITHUB_TOKEN}  -j component-test
RUN curl -s https://raw.githubusercontent.com/nektos/act/master/install.sh | sudo bash
