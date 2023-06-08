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

RUN groupadd --gid 1001 nonroot \
  # user needs a home folder, i.e. to store azure credentials
  && useradd --gid nonroot --create-home --uid 1001 nonroot \
  && mkdir -p /home/nonroot/.ssh \
  && chown -R nonroot:nonroot /home/nonroot \
  && chmod 0700 /home/nonroot/.ssh \
  && chown nonroot:nonroot /app

USER nonroot
WORKDIR /src
ENTRYPOINT ["dotnet","test", "Cheetah.ComponentTest.XUnit/Cheetah.ComponentTest.XUnit.csproj"]
