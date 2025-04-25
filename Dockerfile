FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "ToDoList/ToDoList.csproj"
RUN dotnet publish "ToDoList/ToDoList.csproj" -c Release -o /app/publish /p:UseAppHost=false
RUN echo "Contents of /app/publish:" && ls -la /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
RUN echo "Contents of /app:" && ls -la /app
ENTRYPOINT ["dotnet", "ToDoList.dll"]