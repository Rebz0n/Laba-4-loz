# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем всё содержимое репозитория
COPY . .

# Восстанавливаем зависимости (путь в кавычках из-за пробелов)
RUN dotnet restore "Laba 4 loz/Laba 4 loz.csproj"

# Собираем и публикуем
RUN dotnet publish "Laba 4 loz/Laba 4 loz.csproj" -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Копируем результат сборки из предыдущего этапа
COPY --from=build /app/publish .

# Открываем порт
EXPOSE 8080

# Запускаем (имя DLL обычно совпадает с именем csproj)
ENTRYPOINT ["dotnet", "Laba 4 loz.dll"]
