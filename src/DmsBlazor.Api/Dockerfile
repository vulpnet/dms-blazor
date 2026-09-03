# Build đa giai đoạn — giai đoạn build có SDK đầy đủ, giai đoạn chạy chỉ có
# runtime để image gọn nhẹ hơn (quan trọng khi host free tier giới hạn tài nguyên).

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy trước các file .csproj để tận dụng cache layer Docker khi chỉ code đổi
COPY DmsBlazor.slnx .
COPY src/DmsBlazor.Api/DmsBlazor.Api.csproj src/DmsBlazor.Api/
COPY src/DmsBlazor.Shared/DmsBlazor.Shared.csproj src/DmsBlazor.Shared/
RUN dotnet restore src/DmsBlazor.Api/DmsBlazor.Api.csproj

COPY src/DmsBlazor.Api/ src/DmsBlazor.Api/
COPY src/DmsBlazor.Shared/ src/DmsBlazor.Shared/
RUN dotnet publish src/DmsBlazor.Api/DmsBlazor.Api.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "DmsBlazor.Api.dll"]
