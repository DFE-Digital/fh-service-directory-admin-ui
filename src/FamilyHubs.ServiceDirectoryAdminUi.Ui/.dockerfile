FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
ARG workdir
WORKDIR /App
COPY /$workdir App/
EXPOSE 80
EXPOSE 443
ENTRYPOINT ["dotnet", "FamilyHubs.ServiceDirectoryAdminUI.dll"]