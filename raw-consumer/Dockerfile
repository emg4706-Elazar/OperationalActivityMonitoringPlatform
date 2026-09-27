FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY RawConsumer.csproj ./

RUN dotnet restore

COPY . .

RUN dotnet publish -c release -o /app/publish


FROM mcr.microsoft.com/dotnet/runtime:8.0 AS final

WORKDIR /app

COPY --from=build /app/publish ./

ENTRYPOINT ["dotnet", "RawConsumer.dll"]

