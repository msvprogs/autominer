FROM microsoft/dotnet:2.2.2-aspnetcore-runtime
EXPOSE 52045
WORKDIR /app
COPY binaries/api /app
ENTRYPOINT ["dotnet", "Msv.AutoMiner.CoinInfoService.dll"]