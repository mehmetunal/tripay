# TriPay Web — çok aşamalı, non-root imaj
# docker build -f deploy/docker/Dockerfile -t tripay/web:latest .

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY TriPay.sln ./
COPY Directory.Packages.props ./
COPY TriPay.Services/TriPay.Services.csproj TriPay.Services/
COPY TriPay/TriPay.csproj TriPay/
RUN dotnet restore TriPay/TriPay.csproj
COPY . .
RUN dotnet publish TriPay/TriPay.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
HEALTHCHECK --interval=30s --timeout=5s --start-period=20s --retries=3 \
  CMD wget -qO- http://127.0.0.1:8080/health/live || exit 1
ENTRYPOINT ["dotnet", "TriPay.Web.dll"]
