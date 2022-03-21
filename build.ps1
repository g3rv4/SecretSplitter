docker build . -t g3rv4/secretsplitter:amd64 --build-arg ARCH=amd64
docker push g3rv4/secretsplitter:amd64

docker build . -t g3rv4/secretsplitter:arm64v8 --build-arg ARCH=arm64v8
docker push g3rv4/secretsplitter:arm64v8

docker manifest create `
g3rv4/secretsplitter:latest `
--amend g3rv4/secretsplitter:amd64 `
--amend g3rv4/secretsplitter:arm64v8

docker manifest push g3rv4/secretsplitter:latest
