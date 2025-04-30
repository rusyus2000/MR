#!/usr/bin/env bash
# setup_api.sh - scaffold .NET 8 Web API project with required folder structure

# Variables
default_project_name="portalApi"

# Create solution directory
mkdir -p api
cd api

# Create a new solution
dotnet new sln -n $default_project_name

# Create Web API project
dotnet new webapi -n $default_project_name

# Remove default WeatherForecast files
rm $default_project_name/Controllers/WeatherForecastController.cs
rm $default_project_name/WeatherForecast.cs

# Add project to solution
dotnet sln add $default_project_name/$default_project_name.csproj

# Navigate into project
cd $default_project_name

# Create folder structure
mkdir -p Controllers Models DTOs Services Data

# Create empty files
touch Controllers/ItemsController.cs
touch Controllers/LookupsController.cs
touch Controllers/SearchController.cs

touch Models/Item.cs
touch Models/LookupValue.cs

touch DTOs/ItemDto.cs
touch DTOs/CreateItemDto.cs
touch DTOs/LookupDto.cs

touch Services/IItemService.cs
touch Services/ItemService.cs
touch Services/ILookupService.cs
touch Services/LookupService.cs
touch Services/ISearchService.cs
touch Services/SearchService.cs

touch Data/AppDbContext.cs
touch Data/SampleDataSeeder.cs

# Optional: Install EF Core InMemory provider
dotnet add package Microsoft.EntityFrameworkCore.InMemory

# Restore and build
cd ..
dotnet restore

echo "Scaffolding complete. Open the solution at api/$default_project_name.sln"

