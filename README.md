# AWS SNS Minimal API + AWS SQS Worker Service .NET8

### Requerimientos
- AWS_SNS_MinimalApi
```
AWSSDK.SimpleNotificationService
```

- AWS_SQS_WorkerService
```
AWSSDK.SQS
```

### Estructura del Proyecto
```
AWS_ApiSNS_WorkerSQS_.NET8/
│
├── AWS_SNS_MinimalApi/
│   ├── Dockerfile
│   └── AWS_SNS_MinimalApi.csproj
│
├── AWS_SQS_Worker/
│   ├── Dockerfile
│   └── AWS_SQS_Worker.csproj
│
└── AWS_ApiSNS_WorkerSQS_.NET8.sln
```

## GitHubActions
- Ruta del archivo
- .github/workflows/docker-build-push.yml
```yaml
name: Build and Push Docker Images

# Se ejecuta cuando hacemos push a main
on:
  push:
    branches:
      - main

jobs:
  build-and-push:
    runs-on: ubuntu-latest

    env:
      DOCKER_HUB_USERNAME: ${{ secrets.DOCKER_HUB_USERNAME }}
      DOCKER_HUB_ACCESS_TOKEN: ${{ secrets.DOCKER_HUB_ACCESS_TOKEN }}
      IMAGE_TAG: latest

    steps:
      # 1️⃣ Checkout del repo
      - name: Checkout repository
        uses: actions/checkout@v3

      # 2️⃣ Configurar Docker
      - name: Log in to Docker Hub
        uses: docker/login-action@v2
        with:
          username: ${{ env.DOCKER_HUB_USERNAME }}
          password: ${{ env.DOCKER_HUB_ACCESS_TOKEN }}

      # 3️⃣ Build y push de AWS_SNS_MinimalApi
      - name: Build and push SNS API image
        uses: docker/build-push-action@v5
        with:
          context: ./AWS_SNS_MinimalApi
          file: ./AWS_SNS_MinimalApi/Dockerfile
          push: true
          tags: ${{ env.DOCKER_HUB_USERNAME }}/sns-api:${{ env.IMAGE_TAG }}

      # 4️⃣ Build y push de AWS_SQS_Worker
      - name: Build and push SQS Worker image
        uses: docker/build-push-action@v5
        with:
          context: ./AWS_SQS_Worker
          file: ./AWS_SQS_Worker/Dockerfile
          push: true
          tags: ${{ env.DOCKER_HUB_USERNAME }}/sqs-worker:${{ env.IMAGE_TAG }}

```

### 🔹 Explicación paso a paso
1. `on: push` → se dispara cada vez que haces push a main.
2. `actions/checkout@v3` → clona tu repositorio en la runner de GitHub.
3. `docker/login-action` → loguea Docker en Docker Hub usando secretos.
4. `docker/build-push-action` → build de la imagen desde el Dockerfile de cada proyecto y push al repositorio.
5. `tags` → usa el nombre de usuario de Docker Hub y el tag latest (puedes usar versiones o SHA de commit).

### 🔹 GitHub Secrets necesarios
- Debes crear en tu repo:

| Secret                    | Descripción                                             |
| ------------------------- | ------------------------------------------------------- |
| `DOCKER_HUB_USERNAME`     | Tu usuario de Docker Hub                                |
| `DOCKER_HUB_ACCESS_TOKEN` | Token de acceso generado en Docker Hub (no tu password) |


## AWS_SNS_MinimalApi + AWS_SQS_Worker

## AWS_SNS_WebApi + AWS_SQS_WebConsole_Worker
### Estructura del Proyecto
```
AWS_ApiSNS_WorkerSQS_.NET8/
│
├── AWS_ClassLibrary/
│   ├── Context/
│   │   └── AppDbContext.cs
│   ├── DTOs/
│   │   ├── CompanyDTO.cs
│   │   ├── DonationResultDTO.cs
│   │   └── ProductDTO.cs
│   ├── Entities/
│   │   ├── Company.cs
│   │   ├── DailyStatistics.cs
│   │   ├── Donation.cs
│   │   ├── Product.cs
│   │   └── UserRewards.cs
│   ├── Models/
│   │   ├── ApiResponse.cs
│   │   ├── AWSOptions.cs
│   │   ├── DonationProcessingTask.cs
│   │   ├── DonationTaskData.cs
│   │   ├── FormDonation.cs
│   │   ├── FormEmail.cs
│   │   ├── SnsMessage.cs
│   │   └── SnsMessageAttribute.cs
│   ├── Repositories/
│   │   ├── CompanyRepository.cs
│   │   ├── ICompanyRepository.cs
│   │   ├── IProductRepository.cs
│   │   └── ProductRepository.cs
│   └── Services/
│       ├── Application/
│       │   ├── CompanyService.cs
│       │   ├── DonationService.cs
│       │   ├── ICompanyService.cs
│       │   ├── IDonationService.cs
│       │   ├── INotificationService.cs
│       │   ├── IPdfGenerationService.cs
│       │   ├── IProductService.cs
│       │   ├── NotificationService.cs
│       │   ├── PdfGenerationService.cs
│       │   └── ProductService.cs
│       ├── Infrastructure/
│       │   ├── AwsS3Service.cs
│       │   ├── AwsSnsService.cs
│       │   ├── AwsSqsService.cs
│       │   ├── IAwsS3Service.cs
│       │   ├── IAwsSnsService.cs
│       │   └── IAwsSqsService.cs
│       └── Utilities/
│           └── InvoiceGenerator.cs
│
├── AWS_SNS_WebApi/
│   ├── Controllers/
│   │   └── DonationsController.cs
│   ├── Mediators/
│   │   ├── IApiMediator.cs
│   │   └── ApiMediator.cs
│   └── Program.cs
│
├── AWS_SQS_WebConsole_Worker/
│   ├── Mediators/
│   │   ├── IWorkerMediator.cs
│   │   └── WorkerMediator.cs
│   ├── Services/
│   │   ├── ConsoleSimulatorService.cs
│   │   ├── DonationProcessor.cs
│   │   ├── IConsoleSimulatorService.cs
│   │   └── IDonationProcessor.cs
│   └── Program.cs
│
└── AWS_ApiSNS_WorkerSQS_.NET8.sln
```
