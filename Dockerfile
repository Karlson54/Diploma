# ──────────────────────────────────────────
# Stage 1: Build
# ──────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем csproj файлы и восстанавливаем зависимости отдельным слоем
# Docker кэширует этот слой — повторная сборка не будет качать NuGet пакеты заново
COPY TimeTracker.API/TimeTracker.API.csproj         TimeTracker.API/
COPY TimeTracker.Core/TimeTracker.Core.csproj       TimeTracker.Core/
COPY TimeTracker.Data/TimeTracker.Data.csproj       TimeTracker.Data/

RUN dotnet restore TimeTracker.API/TimeTracker.API.csproj

# Копируем весь исходный код и публикуем
COPY . .
RUN dotnet publish TimeTracker.API/TimeTracker.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ──────────────────────────────────────────
# Stage 2: Runtime
# ──────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Создаём непривилегированного пользователя — best practice безопасности
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "TimeTracker.API.dll"]