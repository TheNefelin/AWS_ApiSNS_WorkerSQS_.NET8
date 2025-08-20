
docker compose build
docker compose up

docker-compose build
docker-compose up

# Construir imagen de la API
cd AWS_ApiSNS_WorkerSQS_.NET8
docker build -f AWS_SNS_MinimalApi/Dockerfile -t sns-api:latest .

cd AWS_ApiSNS_WorkerSQS_.NET8/AWS_SNS_MinimalApi
docker build -t sns-api:latest .
# ----------------------------------------

# Construir imagen del worker
docker build -f AWS_SQS_Worker/Dockerfile -t sqs-worker:latest .

cd AWS_ApiSNS_WorkerSQS_.NET8/AWS_SQS_Worker
docker build -t sqs-worker:latest .
# ----------------------------------------

docker images
docker ps
docker ps -a

# 1. Construir la imagen de AWS_SNS_MinimalApi.
# 2. Construir la imagen de AWS_SQS_Worker.
# 3. Levantar dos contenedores:
#	- uno con la Minimal API (sns-api) expuesto en http://localhost:5000
#	- otro con el Worker (sqs-worker) escuchando mensajes en SQS.
