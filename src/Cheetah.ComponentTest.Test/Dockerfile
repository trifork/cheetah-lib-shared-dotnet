FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

ARG GITHUB_ACTOR
ARG GITHUB_TOKEN

WORKDIR /src
COPY "Cheetah.ComponentTest.Test/NuGet-CI.Config" "NuGet.config"
COPY ["Cheetah.ComponentTest.Test/Cheetah.ComponentTest.Test.csproj", "Cheetah.ComponentTest.Test/"]
COPY ["Cheetah.ComponentTest/Cheetah.ComponentTest.csproj", "Cheetah.ComponentTest/"]
COPY ["Cheetah.Auth/Cheetah.Auth.csproj", "Cheetah.Auth/"]
RUN --mount=type=secret,id=GITHUB_TOKEN \
    GITHUB_TOKEN="$(cat /run/secrets/GITHUB_TOKEN)" \
    dotnet restore "Cheetah.ComponentTest.Test/Cheetah.ComponentTest.Test.csproj"
COPY . .

WORKDIR /src
ENTRYPOINT ["dotnet","test", "Cheetah.ComponentTest.Test/Cheetah.ComponentTest.Test.csproj"]
