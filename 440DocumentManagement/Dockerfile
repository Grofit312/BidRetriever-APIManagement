FROM microsoft/aspnetcore:2.0 AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/aspnetcore-build:2.0 AS build
WORKDIR /src
COPY 440DocumentManagement.csproj 440DocumentManagement/
RUN dotnet restore 440DocumentManagement/440DocumentManagement.csproj
COPY . 440DocumentManagement/
WORKDIR /src/440DocumentManagement
RUN dotnet build 440DocumentManagement.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish 440DocumentManagement.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "440DocumentManagement.dll"]
