FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src
RUN chown -R app:app /src
COPY NuGet.Config .
COPY Cheetah.Kafka.ExampleProcessor.csproj .
RUN \
    --mount=type=secret,id=GITHUB_ACTOR,uid=$APP_UID \
    --mount=type=secret,id=GITHUB_TOKEN,uid=$APP_UID \
    GITHUB_ACTOR="$(cat /run/secrets/GITHUB_ACTOR)" \
    GITHUB_TOKEN="$(cat /run/secrets/GITHUB_TOKEN)" \
    dotnet restore

COPY . .
RUN ls
RUN dotnet publish -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0

LABEL version="1.0.0"

WORKDIR /app
COPY --from=build /app ./
ENTRYPOINT ["dotnet", "Cheetah.Kafka.ExampleProcessor.dll"]