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
<<<<<<< HEAD
<<<<<<< HEAD
ENTRYPOINT ["dotnet", "AI_Campus_Assistant_System.dll"]
=======
<<<<<<< Updated upstream
ENTRYPOINT ["dotnet", "AI_Final_Campus_Assistant.dll"]
=======
ENTRYPOINT ["dotnet", "AI_Campus_Assistant.dll"]
>>>>>>> Stashed changes
>>>>>>> f1a3adaf3b9e1d8dcbdf92dfe19c32e92791316c
=======


ENTRYPOINT ["dotnet", "AI_Campus_Assistant.dll"]

>>>>>>> 9218f6b2885240b92e8f9a0f20cad21b558134b5
