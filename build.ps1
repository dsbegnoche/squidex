# Build the image
#docker build . -t squidex-build-image -f dockerfile.build
docker build . -t squidex-build-image -f Dockerfile

# Open the image
docker create --name squidex-build-container squidex-build-image

# Copy the output to the host file system
docker cp squidex-build-container:/out ./publish

# Cleanup
docker rm squidex-build-container
