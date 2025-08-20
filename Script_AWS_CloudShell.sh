# Check disk space
df -h

# Clean Docker system (images, containers, cache)
docker system prune -a -f

# Clean Docker volumes (careful, deletes unused volumes)
docker volume prune -f

# Clean Docker system including volumes
docker system prune -a --volumes -f

# Dlete file or folder if it exists
rm -rf AWS_ApiSNS_WorkerSQS_.NET8
rm text.txt

# Clone repo
git clone https://github.com/TheNefelin/AWS_ApiSNS_WorkerSQS_.NET8.git

# Enter project folder
ls
pwd

cd AWS_ApiSNS_WorkerSQS_.NET8

# sns-api image -------------------------------------------
# Login to ECR
aws ecr get-login-password --region us-east-1 ...

# Build Docker image
docker build -f AWS_SNS_MinimalApi/Dockerfile -t sns-api:latest .
docker build -f AWS_SNS_MinimalApi/Dockerfile -t ecomexpress-sns-api-repo .

# Tag Docker image for ECR
docker tag ....

# Push Docker image to ECR
docker push ....

# sqs-worker image ----------------------------------------
# Login to ECR
aws ecr get-login-password --region us-east-1 ...

# Build Docker image
docker build -f AWS_SQS_Worker/Dockerfile -t sqs-worker:latest .
docker build -f AWS_SQS_Worker/Dockerfile -t ecomexpress-sqs-worker-repo .

# Tag Docker image for ECR
docker tag ....

# Push Docker image to ECR
docker push ....

# --------------------------------------------------------

# Check disk space again
tar -czf context.tar.gz .dockerignore Dockerfile * && du -sh context.tar.gz
tar --exclude-from=.dockerignore -cf - . | wc -c
