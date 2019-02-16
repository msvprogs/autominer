FROM microsoft/dotnet:2.2.2-aspnetcore-runtime
EXPOSE 2052
EXPOSE 2053
WORKDIR /app
COPY binaries/api /app
ENTRYPOINT ["dotnet", "Msv.AutoMiner.FrontEnd.dll"]