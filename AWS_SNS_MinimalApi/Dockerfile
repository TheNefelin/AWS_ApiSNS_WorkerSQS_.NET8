# Etapa 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["AWS_SNS_MinimalApi/AWS_SNS_MinimalApi.csproj", "AWS_SNS_MinimalApi/"]
RUN dotnet restore "AWS_SNS_MinimalApi/AWS_SNS_MinimalApi.csproj"
COPY . .
WORKDIR "/src/AWS_SNS_MinimalApi"
RUN dotnet publish -c Release -o /app/publish

# Etapa 2: Runtime (ASP.NET Core)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "AWS_SNS_MinimalApi.dll"]
