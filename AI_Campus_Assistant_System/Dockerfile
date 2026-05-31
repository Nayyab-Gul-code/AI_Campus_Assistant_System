FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["AI_Campus_Assistant.csproj", "."]
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 80
<<<<<<< Updated upstream
ENTRYPOINT ["dotnet", "AI_Final_Campus_Assistant.dll"]
=======
ENTRYPOINT ["dotnet", "AI_Campus_Assistant.dll"]
>>>>>>> Stashed changes
