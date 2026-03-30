FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ./ConversionService/ConversionService.sln ./ConversionService/
COPY ./ConversionService/src/ConversionService.Api/ ./ConversionService/src/ConversionService.Api/
COPY ./ConversionService/src/ConversionService.Application/ ./ConversionService/src/ConversionService.Application/
COPY ./ConversionService/src/ConversionService.Domain/ ./ConversionService/src/ConversionService.Domain/
COPY ./ConversionService/src/ConversionService.Infrastructure/ ./ConversionService/src/ConversionService.Infrastructure/

WORKDIR /src/ConversionService/src/ConversionService.Api
RUN dotnet restore

RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "ConversionService.Api.dll"]