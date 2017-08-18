#
# Stage 1, Prebuild
#
FROM microsoft/aspnetcore-build:1.1.2 as builder

# Install runtime dependencies
RUN apt-get update \
 && apt-get install -y --no-install-recommends ca-certificates bzip2 libfontconfig \
 && apt-get clean \
 && rm -rf /var/lib/apt/lists/*

COPY src/Squidex/package.json /tmp/package.json

# Install Node packages
RUN cd /tmp && npm install

COPY . .

WORKDIR /

# Build Frontend
RUN cp -a /tmp/node_modules /src/Squidex/ \
 && cd /src/Squidex \
 && npm run build:copy \
 && npm run build

# Test Backend
RUN dotnet restore Squidex.sln

# Publish
RUN dotnet publish src/Squidex/Squidex.csproj --output /out/ --configuration Release

#
# Stage 2, Build runtime
#
FROM microsoft/aspnetcore:1.1.2

# Default AspNetCore directory
WORKDIR /app

# Copy from nuild stage
COPY --from=builder /out/ .

EXPOSE 80

ENTRYPOINT ["dotnet", "Squidex.dll"]
