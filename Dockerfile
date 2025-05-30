# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["CatVault.Api.csproj", "./"]
RUN dotnet restore

COPY . .
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_GC_DOCKER_SWEEP=true \
    DOTNET_GC_DOCKER_SWEEP_MAXHEAPSIZE=75

# copy build output
COPY --from=build /app/publish ./

# expose ports
EXPOSE 80
EXPOSE 443

# entrypoint
ENTRYPOINT ["dotnet", "CatVault.Api.dll"]
