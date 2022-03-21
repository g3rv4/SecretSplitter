FROM mcr.microsoft.com/dotnet/sdk:6.0.201-alpine3.15-amd64
WORKDIR /var/src
COPY src/* ./
RUN dotnet publish -c Release -o /var/publish

FROM mcr.microsoft.com/dotnet/runtime:6.0.3-alpine3.15-amd64
RUN apk add --update chromium libexif udev && \
    apk info --purge
ENV CHROMIUM_EXECUTABLE=/usr/bin/chromium-browser
WORKDIR /var/output
COPY --from=0 /var/publish /var/app
CMD ["dotnet", "/var/app/SecretSplitter.dll"]