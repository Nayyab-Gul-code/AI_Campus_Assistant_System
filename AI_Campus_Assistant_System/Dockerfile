FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 1. Sab se pehle .csproj file copy kar ke restore karte hain
COPY ["AI_Campus_Assistant_System.csproj", "."]
RUN dotnet restore "AI_Campus_Assistant_System.csproj"

# 2. Baki ka sara code copy karte hain
COPY . .

# 3. Project ko publish karte hain
RUN dotnet publish "AI_Campus_Assistant_System.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 4. Final runtime image taiyar karte hain
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "AI_Campus_Assistant_System.dll"]
ENTRYPOINT ["dotnet", "AI_Final_Campus_Assistant.dll"]

ENTRYPOINT ["dotnet", "AI_Campus_Assistant.dll"]

