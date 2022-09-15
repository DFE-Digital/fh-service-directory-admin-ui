FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
ARG workdir
COPY /$workdir App/
WORKDIR /App
ENTRYPOINT ["dotnet", "FamilyHubs.ServiceDirectoryAdminUI.dll"]