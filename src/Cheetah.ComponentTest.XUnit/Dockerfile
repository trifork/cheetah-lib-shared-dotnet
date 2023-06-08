FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

ARG GITHUB_ACTOR
ARG GITHUB_TOKEN

WORKDIR /src
COPY "Cheetah.ComponentTest.XUnit/NuGet-CI.Config" "NuGet.config"
COPY ["Cheetah.ComponentTest.XUnit/Cheetah.ComponentTest.XUnit.csproj", "Cheetah.ComponentTest.XUnit/"]
COPY ["Cheetah.Componenttest/Cheetah.ComponentTest.csproj", "Cheetah.Componenttest/"]
COPY ["Cheetah.Core/Cheetah.Core.csproj", "Cheetah.Core/"]
RUN dotnet restore "Cheetah.ComponentTest.XUnit/Cheetah.ComponentTest.XUnit.csproj"
COPY . .

USER integrationTest
WORKDIR /src
ENTRYPOINT ["dotnet","test", "Cheetah.ComponentTest.XUnit/Cheetah.ComponentTest.XUnit.csproj"]
