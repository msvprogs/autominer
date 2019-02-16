FROM microsoft/dotnet:2.2.2-aspnetcore-runtime
EXPOSE 52045
WORKDIR /app
COPY Msv.AutoMiner/binaries/coin-info /app
ENTRYPOINT ["dotnet", "Msv.AutoMiner.CoinInfoService.dll"]