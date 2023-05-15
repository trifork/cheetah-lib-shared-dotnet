FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

ARG GITHUB_ACTOR
ARG GITHUB_TOKEN

WORKDIR /src
COPY "Cheetah.ComponentTest.Test/NuGet-CI.Config" "NuGet.config"
COPY ["Cheetah.ComponentTest.Test/Cheetah.ComponentTest.Test.csproj", "Cheetah.ComponentTest.Test/"]
COPY ["Cheetah.Componenttest/Cheetah.ComponentTest.csproj", "Cheetah.Componenttest/"]
RUN dotnet restore "Cheetah.ComponentTest.Test/Cheetah.ComponentTest.Test.csproj" && \
    dotnet restore "Cheetah.Componenttest/Cheetah.ComponentTest.csproj"
COPY . .
WORKDIR "/src/Cheetah.ComponentTest.Test/"
RUN dotnet build "Cheetah.ComponentTest.Test.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Cheetah.ComponentTest.Test/Cheetah.ComponentTest.Test.csproj" -c Release -o /app/publish

FROM base AS final

RUN apt-get update && apt-get install -y curl \
    && rm -rf /var/lib/apt/lists/*

# Create a new user with bash and a homedir
RUN useradd -r -u 1001 -m -s /bin/bash dotnetuser

# Set the user for subsequent commands
USER dotnetuser

HEALTHCHECK --start-period=5s --interval=5s \
    CMD curl --fail http://localhost:80/health || exit 1

WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Cheetah.ComponentTest.Test.dll"]
