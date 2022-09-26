#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0.9 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["src/Microsoft.Health.Dicom.Pin.InferenceWorker/Microsoft.Health.Dicom.Pin.InferenceWorker.csproj", "src/Microsoft.Health.Dicom.Pin.InferenceWorker/"]
COPY ["src/Microsoft.Health.Dicom.Pin.Core/Microsoft.Health.Dicom.Pin.Core.csproj", "src/Microsoft.Health.Dicom.Pin.Core/"]
COPY ["src/Microsoft.Health.Dicom.Client/Microsoft.Health.Dicom.Client.csproj", "src/Microsoft.Health.Dicom.Client/"]
COPY ["forks/Microsoft.Health.FellowOakDicom/Microsoft.Health.FellowOakDicom.csproj", "forks/Microsoft.Health.FellowOakDicom/"]
COPY ["src/Microsoft.Health.Dicom.Pin.CosmosDb/Microsoft.Health.Dicom.Pin.CosmosDb.csproj", "src/Microsoft.Health.Dicom.Pin.CosmosDb/"]
COPY ["src/Microsoft.Health.Dicom.Pin.ServiceBus/Microsoft.Health.Dicom.Pin.ServiceBus.csproj", "src/Microsoft.Health.Dicom.Pin.ServiceBus/"]
COPY ["src/Microsoft.Health.Dicom.Pin.Storage/Microsoft.Health.Dicom.Pin.Storage.csproj", "src/Microsoft.Health.Dicom.Pin.Storage/"]
RUN dotnet restore "src/Microsoft.Health.Dicom.Pin.InferenceWorker/Microsoft.Health.Dicom.Pin.InferenceWorker.csproj"
COPY . .
WORKDIR "/src/src/Microsoft.Health.Dicom.Pin.InferenceWorker"
RUN dotnet build "Microsoft.Health.Dicom.Pin.InferenceWorker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Microsoft.Health.Dicom.Pin.InferenceWorker.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Microsoft.Health.Dicom.Pin.InferenceWorker.dll"]
