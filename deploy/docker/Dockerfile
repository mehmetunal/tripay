# TriPay.Web — kurumsal site imajı
# docker build -f deploy/docker/Dockerfile -t tripay/web:latest .

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY TriPay.sln ./
COPY Directory.Packages.props Directory.Build.props Directory.Build.targets ./
COPY TriPay.Web/TriPay.Web.csproj TriPay.Web/
RUN dotnet restore TriPay.Web/TriPay.Web.csproj
COPY TriPay.Web/ TriPay.Web/
RUN dotnet publish TriPay.Web/TriPay.Web.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "TriPay.Web.dll"]
