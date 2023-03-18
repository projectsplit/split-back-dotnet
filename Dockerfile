# Use the official .NET 7.0 SDK image as a base
FROM mcr.microsoft.com/dotnet/sdk:7.0

# Set the working directory in the container to /app
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Development
# Copy the published output of your ASP.NET Core app to the container's /app folder
COPY ./publish .

# Expose port 80 443 for the container
EXPOSE 80 443

# Start the app using the dotnet command
ENTRYPOINT ["dotnet", "SplitBackApi.dll"]
