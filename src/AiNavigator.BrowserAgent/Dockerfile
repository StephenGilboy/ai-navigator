FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/AiNavigator.BrowserAgent/AiNavigator.BrowserAgent.csproj", "src/AiNavigator.BrowserAgent/"]
RUN dotnet restore "src/AiNavigator.BrowserAgent/AiNavigator.BrowserAgent.csproj"
COPY . .
WORKDIR "/src/src/AiNavigator.BrowserAgent"
RUN dotnet build "AiNavigator.BrowserAgent.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "AiNavigator.BrowserAgent.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AiNavigator.BrowserAgent.dll"]
