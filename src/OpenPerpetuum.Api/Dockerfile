FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 49953
EXPOSE 44332

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY ["OpenPerpetuum.Api/OpenPerpetuum.Api.csproj", "OpenPerpetuum.Api/"]
RUN dotnet restore "OpenPerpetuum.Api/OpenPerpetuum.Api.csproj"
COPY . .
WORKDIR "/src/OpenPerpetuum.Api"
RUN dotnet build "OpenPerpetuum.Api.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "OpenPerpetuum.Api.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "OpenPerpetuum.Api.dll"]