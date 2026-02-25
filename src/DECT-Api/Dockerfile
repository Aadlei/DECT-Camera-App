FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 5000
EXPOSE 5001

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/DECT-Api/DECT-Api.csproj", "src/DECT-Api/"]
COPY ["src/DECT-Shared/DECT-Shared.csproj", "src/DECT-Shared/"]
RUN dotnet restore "src/DECT-Api/DECT-Api.csproj"
COPY . .
WORKDIR "/src/src/DECT-Api"
RUN dotnet build "./DECT-Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./DECT-Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DECT-Api.dll"]
