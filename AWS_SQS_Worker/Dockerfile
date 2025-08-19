# Etapa 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["AWS_SQS_Worker/AWS_SQS_Worker.csproj", "AWS_SQS_Worker/"]
RUN dotnet restore "AWS_SQS_Worker/AWS_SQS_Worker.csproj"
COPY . .
WORKDIR "/src/AWS_SQS_Worker"
RUN dotnet publish -c Release -o /app/publish

# Etapa 2: Runtime (console app)
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "AWS_SQS_Worker.dll"]
