FROM microsoft/dotnet:2.2.2-aspnetcore-runtime
EXPOSE 6285
EXPOSE 6284
EXPOSE 6283
WORKDIR /app
COPY Msv.AutoMiner/binaries/api /app
ENTRYPOINT ["dotnet", "Msv.AutoMiner.ControlCenterService.dll"]