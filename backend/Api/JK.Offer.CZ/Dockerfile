FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY . .

RUN dotnet restore Api/JK.Offer.CZ/JK.Offer.CZ.csproj
RUN dotnet publish Api/JK.Offer.CZ/JK.Offer.CZ.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "JK.Offer.CZ.dll"]
