docker compose build
docker compose up

docker images
docker ps
docker ps -a

# 1. Construir la imagen de AWS_SNS_MinimalApi.
# 2. Construir la imagen de AWS_SQS_Worker.
# 3. Levantar dos contenedores:
#	- uno con la Minimal API (sns-api) expuesto en http://localhost:5000
#	- otro con el Worker (sqs-worker) escuchando mensajes en SQS.
