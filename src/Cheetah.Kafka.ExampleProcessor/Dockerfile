FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/Cheetah.Kafka.ExampleProcessor/Cheetah.Kafka.ExampleProcessor.csproj", "Cheetah.Kafka.ExampleProcessor/"]
RUN dotnet restore "src/Cheetah.Kafka.ExampleProcessor/Cheetah.Kafka.ExampleProcessor.csproj"
COPY . .
WORKDIR "/src/Cheetah.Kafka.ExampleProcessor"
RUN dotnet build "Cheetah.Kafka.ExampleProcessor.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Cheetah.Kafka.ExampleProcessor.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Cheetah.Kafka.ExampleProcessor.dll"]
