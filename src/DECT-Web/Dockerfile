FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/DECT-Web/DECT-Web.csproj", "src/DECT-Web/"]
COPY ["src/DECT-Shared/DECT-Shared.csproj", "src/DECT-Shared/"]
RUN dotnet restore "src/DECT-Web/DECT-Web.csproj"
COPY . .
WORKDIR "/src/src/DECT-Web"
RUN dotnet publish "./DECT-Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish

FROM nginx:alpine AS final
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html
COPY src/DECT-Web/nginx.conf /etc/nginx/nginx.conf
EXPOSE 80