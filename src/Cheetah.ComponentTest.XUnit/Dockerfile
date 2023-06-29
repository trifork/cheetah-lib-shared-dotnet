FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

ARG GITHUB_ACTOR
ARG GITHUB_TOKEN

WORKDIR /src
COPY "Cheetah.ComponentTest.XUnit/NuGet-CI.Config" "NuGet.config"
COPY ["Cheetah.ComponentTest.XUnit/Cheetah.ComponentTest.XUnit.csproj", "Cheetah.ComponentTest.XUnit/"]
COPY ["Cheetah.ComponentTest/Cheetah.ComponentTest.csproj", "Cheetah.ComponentTest/"]
COPY ["Cheetah.Core/Cheetah.Core.csproj", "Cheetah.Core/"]
RUN --mount=type=secret,id=GITHUB_TOKEN \
    GITHUB_TOKEN="$(cat /run/secrets/GITHUB_TOKEN)" \
    dotnet restore "Cheetah.ComponentTest.XUnit/Cheetah.ComponentTest.XUnit.csproj"
COPY . .

WORKDIR /src
ENTRYPOINT ["dotnet","test", "Cheetah.ComponentTest.XUnit/Cheetah.ComponentTest.XUnit.csproj"]
