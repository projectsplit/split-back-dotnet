# Use the official .NET 7.0 SDK image as a base
FROM mcr.microsoft.com/dotnet/sdk:7.0

# Set the working directory in the container to /app
WORKDIR /app

# Copy the published output of your ASP.NET Core app to the container's /app folder
COPY ./publish .

# Expose port 5000 5001 for the container
EXPOSE 5000 5001

# Start the app using the dotnet command
ENTRYPOINT ["dotnet", "SplitBackApi.dll"]
