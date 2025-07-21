FROM mcr.microsoft.com/dotnet/sdk:8.0 AS deps
WORKDIR /src

COPY Payments/Payments.csproj Payments/Payments.csproj
RUN dotnet restore Payments/Payments.csproj

FROM deps AS build
WORKDIR /src

COPY Payments/ Payments/
RUN dotnet publish -c release -o /publish --no-restore Payments/Payments.csproj

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build /publish ./

ENV ASPNETCORE_URLS=http://:8080
EXPOSE 8080
USER app
ENTRYPOINT ["dotnet", "Payments.dll"]