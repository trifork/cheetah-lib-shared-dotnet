FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/Cheetah.OpenSearch.ExampleAPI/Cheetah.OpenSearch.ExampleAPI.csproj", "Cheetah.OpenSearch.ExampleAPI/"]
RUN dotnet restore "src/Cheetah.OpenSearch.ExampleAPI/Cheetah.OpenSearch.ExampleAPI.csproj"
COPY . .
WORKDIR "/src/Cheetah.OpenSearch.ExampleAPI"
RUN dotnet build "Cheetah.OpenSearch.ExampleAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Cheetah.OpenSearch.ExampleAPI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Cheetah.OpenSearch.ExampleAPI.dll"]
