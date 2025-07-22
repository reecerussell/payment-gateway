FROM mcr.microsoft.com/dotnet/sdk:8.0 AS deps
WORKDIR /src

COPY Payments/Payments.csproj Payments/Payments.csproj
COPY Payments.Abstractions/Payments.Abstractions.csproj Payments.Abstractions/Payments.Abstractions.csproj

RUN dotnet restore Payments/Payments.csproj

FROM deps AS build
WORKDIR /src

COPY Payments/ Payments/
COPY Payments.Abstractions/ Payments.Abstractions/

RUN dotnet publish -c release -o /publish --no-restore Payments/Payments.csproj

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build /publish ./

EXPOSE 8080
USER app
ENTRYPOINT ["dotnet", "Payments.dll"]

HEALTHCHECK --interval=60s --timeout=5s --start-period=10s --retries=3 CMD ["curl", "-f", "http://localhost:8080/health"]