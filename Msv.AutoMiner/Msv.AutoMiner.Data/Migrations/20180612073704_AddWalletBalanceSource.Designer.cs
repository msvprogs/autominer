﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Msv.AutoMiner.Common.Data.Enums;
using Msv.AutoMiner.Data;
using System;

namespace Msv.AutoMiner.Data.Migrations
{
    [DbContext(typeof(AutoMinerDbContext))]
    [Migration("20180612073704_AddWalletBalanceSource")]
    partial class AddWalletBalanceSource
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                .HasAnnotation("ProductVersion", "2.0.1-rtm-125");

            modelBuilder.Entity("Msv.AutoMiner.Data.ApiKey", b =>
                {
                    b.Property<string>("Key")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(128);

                    b.Property<DateTime?>("Expires");

                    b.Property<byte>("Type");

                    b.Property<int?>("UsagesLeft");

                    b.HasKey("Key");

                    b.ToTable("ApiKeys");
                });

            modelBuilder.Entity("Msv.AutoMiner.Data.Coin", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<byte>("Activity");

                    b.Property<byte>("AddressFormat");

                    b.Property<string>("AddressPrefixes")
                        .HasMaxLength(64);

                    b.Property<Guid>("AlgorithmId");

                    b.Property<bool>("DisableBlockRewardChecking");

                    b.Property<bool>("GetDifficultyFromLastPoWBlock");

                    b.Property<bool>("IgnoreInactiveMarket");

                    b.Property<string>("LastNetworkInfoMessage")
                        .HasMaxLength(8192);

                    b.Property<int?>("LastNetworkInfoResult");

                    b.Property<byte[]>("LogoImageBytes")
                        .HasMaxLength(32768);

                    b.Property<string>("MaxTarget")
                        .HasMaxLength(128);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(256);

                    b.Property<string>("NetworkInfoApiName")
                        .HasMaxLength(64);

                    b.Property<byte>("NetworkInfoApiType");

                    b.Property<string>("NetworkInfoApiUrl")
                        .HasMaxLength(512);

                    b.Property<string>("NodeHost")
                        .HasMaxLength(512);

                    b.Property<string>("NodeLogin")
                        .HasMaxLength(64);

                    b.Property<string>("NodePassword")
                        .HasMaxLength(64);

                    b.Property<int>("NodePort");

                    b.Property<string>("RewardCalculationJavaScript")
                        .HasMaxLength(16384);

                    b.Property<string>("Symbol")
                        .IsRequired()
                        .HasMaxLength(32);

                    b.HasKey("Id");

                    b.HasIndex("AlgorithmId");

                    b.ToTable("Coins");
                });

            modelBuilder.Entity("Msv.AutoMiner.Data.CoinAlgorithm", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<byte>("Activity");

                    b.Property<string>("AdditionalArguments")
                        .HasMaxLength(128);

                    b.Property<string>("AlgorithmArgument")
                        .HasMaxLength(32);

                    b.Property<string>("Aliases")
                        .HasMaxLength(256);

                    b.Property<double?>("Intensity");

                    b.Property<int?>("KnownValue");

                    b.Property<int?>("MinerId");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(64);

                    b.Property<byte>("ProfitabilityFormulaType");

                    b.HasKey("Id");

                    b.HasIndex("MinerId");

                    b.ToTable("CoinAlgorithms");
                });

            modelBuilder.Entity("Msv.AutoMiner.Data.CoinFiatValue", b =>
                {
                    b.Property<Guid>("CoinId");

                    b.Property<int>("FiatCurrencyId");

                    b.Property<DateTime>("DateTime");

                    b.Property<byte>("Source");

                    b.Property<double>("Value");

                    b.HasKey("CoinId", "FiatCurrencyId", "DateTime", "Source");

                    b.HasIndex("FiatCurrencyId");

                    b.ToTable("CoinFiatValues");
                });

            modelBuilder.Entity("Msv.AutoMiner.Data.CoinNetworkInfo", b =>
                {
                    b.Property<Guid>("CoinId");

                    b.Property<DateTime>("Created");

                    b.Property<double>("BlockReward");

                    b.Property<double>("BlockTimeSeconds");

                    b.Property<double>("Difficulty");

                    b.Property<long>("Height");

                    b.Property<DateTime?>("LastBlockTime");

                    b.Property<int?>("MasternodeCount");

                    b.Property<double>("NetHashRate");

                    b.Property<double?>("TotalSupply");

                    b.HasKey("CoinId", "Created");

                    b.ToTable("CoinNetworkInfos");
                });

            modelBuilder.Entity("Msv.AutoMiner.Data.CoinProfitability", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

                    b.Property<double>("BtcPerDay");

                    b.Property<Guid>("CoinId");

                    b.Property<double>("CoinsPerDay");

                    b.Property<double>("ElectricityCost");

                    b.Property<int>("PoolId");

                    b.Property<DateTime>("Requested");

                    b.Property<int>("RigId");

                    b.Property<double>("UsdPerDay");

                    b.HasKey("Id");

                    b.HasIndex("CoinId");

                    b.HasIndex("PoolId");

                    b.ToTable("CoinProfitabilities");
                });

            modelBuilder.Entity("Msv.AutoMiner.Data.Exchange", b =>
                {
                    b.Property<int>("Type");

                    b.Property<byte>("Activity");

                    b.Property<string>("IgnoredCurrencies")
                        .HasMaxLength(512);

                    b.Property<string>("PrivateKey")
                        .HasMaxLength(256);

                    b.Property<string>("PublicKey")
                        .HasMaxLength(256);

                    b.HasKey("Type");

                    b.ToTable("Exchanges");
                });

            modelBuilder.Entity("Msv.AutoMiner.Data.ExchangeCurrency", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

                    b.Property<Guid?>("CoinId");

                    b.Property<DateTime>("DateTime");

                    b.Property<int>("Exchange");

                    b.Property<bool>("IsActive");

                    b.Property<double>("MinWithdrawAmount");

                    b.Property<string>("Symbol")
                        .IsRequired()
                        .HasMaxLength(16);

                    b.Property<double>("WithdrawalFee");

                    b.HasKey("Id");

                    b.HasIndex("CoinId");

                    b.HasIndex("Symbol");

                    b.ToTable("ExchangeCurrencies");
                });

            modelBuilder.Entity("Msv.AutoMiner.Data.ExchangeMarketPrice", b =>
                {
                    b.Property<Guid>("SourceCoinId");

                    b.Property<Guid>("TargetCoinId");

                    b.Property<int>("ExchangeType")
                        .HasColumnName("Exchange");

                    b.Property<DateTime>("DateTime");

                    b.Property<double>("BuyFeePercent");

                    b.Property<double>("HighestBid");

                    b.Property<bool>("IsActive");

                    b.Property<double>("LastDayHigh");

                    b.Property<double>("LastDayLow");

                    b.Property<double>("LastDayVolume");

                    b.Property<double>("LastPrice");

                    b.Property<double>("LowestAsk");

                    b.Property<double>("SellFeePercent");

                    b.HasKey("SourceCoinId", "TargetCoinId", "ExchangeType", "DateTime");

                    b.HasIndex("ExchangeType");

                    b.HasIndex("TargetCoinId");

                    b.HasIndex("SourceCoinId", "TargetCoinId", "ExchangeType", "DateTime");

                    b.ToTable("ExchangeMarketPrices");
                });

            modelBuilder.Entity("Msv.AutoMiner.Data.FiatCurrency", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(32);

                    b.Property<string>("Symbol")
                        .IsRequired()
                        .HasMaxLength(32);

                    b.HasKey("Id");

                    b.ToTable("FiatCurrencies");
                });

            modelBuilder.Entity("Msv.AutoMiner.Data.Miner", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

                    b.Property<byte>("Activity");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(64);

                    b.HasKey("Id");

                    b.ToTable("Miners");
                });

            modelBuilder.Entity("Msv.AutoMiner.Data.MinerVersion", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("AdditionalArguments")
                        .HasMaxLength(128);

                    b.Property<string>("AlgorithmArgument")
                        .HasMaxLength(32);

                    b.Property<int?>("ApiPort");

                    b.Property<string>("ApiPortArgument")
                        .HasMaxLength(32);

                    b.Property<byte>("ApiType");

                    b.Property<string>("BenchmarkArgument")
                        .HasMaxLength(32);

                    b.Property<string>("BenchmarkResultRegex")
                        .HasMaxLength(256);

                    b.Property<string>("ExeFilePath")
                        .HasMaxLength(512);

                    b.Property<string>("ExeSecondaryFilePath")
                        .HasMaxLength(512);

                    b.Property<string>("IntensityArgument")
                        .HasMaxLength(32);

                    b.Property<string>("InvalidShareRegex")
                        .HasMaxLength(256);

                    b.Property<int>("MinerId");

                    b.Property<bool>("OmitUrlSchema");

                    b.Property<string>("PasswordArgument")
                        .HasMaxLength(32);

                    b.Property<byte>("Platform");

                    b.Property<string>("PortArgument")
                        .HasMaxLength(32);

                    b.Property<string>("ServerArgument")
                        .HasMaxLength(32);

                    b.Property<string>("SpeedRegex")
                        .HasMaxLength(256);

                    b.Property<DateTime>("Uploaded");

                    b.Property<string>("UserArgument")
                        .HasMaxLength(32);

                    b.Property<string>("ValidShareRegex")
                        .HasMaxLength(256);

                    b.Property<string>("Version")
                        .HasMaxLength(32);

                    b.Property<string>("ZipPath")
                        .HasMaxLength(512);

                    b.HasKey("Id");

                    b.HasIndex("MinerId");

                    b.ToTable("MinerVersions");
                });

            modelBuilder.Entity("Msv.AutoMiner.Data.MultiCoinPool", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

                    b.Property<byte>("Activity");

                    b.Property<int>("ApiProtocol");

                    b.Property<string>("ApiUrl")
                        .HasMaxLength(256);

                    b.Property<string>("MiningUrl")
                        .HasMaxLength(256);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(64);

                    b.Property<string>("SiteUrl")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.ToTable("MultiCoinPools");
                });

            modelBuilder.Entity("Msv.AutoMiner.Data.MultiCoinPoolCurrency", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Algorithm")
                        .HasMaxLength(64);

                    b.Property<double>("Hashrate");

                    b.Property<bool>("IsIgnored");

                    b.Property<int>("MultiCoinPoolId");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(128);

                    b.Property<int>("Port");

                    b.Property<string>("Symbol")
                        .IsRequired()
                        .HasMaxLength(64);

                    b.Property<int>("Workers");

                    b.HasKey("Id");

                    b.HasIndex("MultiCoinPoolId");

                    b.ToTable("MultiCoinPoolCurrencies");
                });

            modelBuilder.Entity("Msv.AutoMiner.Data.Pool", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

                    b.Property<byte>("Activity");

                    b.Property<string>("ApiKey")
                        .HasMaxLength(256);

                    b.Property<string>("ApiPoolName")
                        .HasMaxLength(64);

                    b.Property<int>("ApiProtocol");

                    b.Property<string>("ApiSecondaryUrl")
                        .HasMaxLength(256);

                    b.Property<string>("ApiUrl")
                        .HasMaxLength(256);

                    b.Property<byte>("Availability");

                    b.Property<Guid>("CoinId");

                    b.Property<double>("FeeRatio");

                    b.Property<string>("Host")
                        .IsRequired()
                        .HasMaxLength(128);

                    b.Property<bool>("IsAnonymous");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(128);

                    b.Property<int?>("PoolUserId");

                    b.Property<int>("Port");

                    b.Property<int>("Priority");

                    b.Property<byte>("Protocol");

                    b.Property<DateTime?>("ResponsesStoppedDate");

                    b.Property<double>("TimeZoneCorrectionHours");

                    b.Property<bool>("UseBtcWallet");

                    b.Property<string>("WorkerLogin")
                        .HasMaxLength(128);

                    b.Property<string>("WorkerPassword")
                        .HasMaxLength(64);

                    b.HasKey("Id");

                    b.HasIndex("CoinId");

                    b.ToTable("Pools");
                });

            modelBuilder.Entity("Msv.AutoMiner.Data.PoolAccountState", b =>
                {
                    b.Property<int>("PoolId");

                    b.Property<DateTime>("DateTime");

                    b.Property<double>("ConfirmedBalance");

                    b.Property<long>("HashRate");

                    b.Property<int>("InvalidShares");

                    b.Property<long>("PoolHashRate");

                    b.Property<long?>("PoolLastBlock");

                    b.Property<int>("PoolWorkers");

                    b.Property<double>("UnconfirmedBalance");

                    b.Property<int>("ValidShares");

                    b.HasKey("PoolId", "DateTime");

                    b.ToTable("PoolAccountStates");
                });

            modelBuilder.Entity("Msv.AutoMiner.Data.PoolPayment", b =>
                {
                    b.Property<int>("PoolId");

                    b.Property<string>("ExternalId")
                        .HasMaxLength(64);

                    b.Property<double>("Amount");

                    b.Property<string>("BlockHash")
                        .HasMaxLength(256);

                    b.Property<string>("CoinAddress")
                        .HasMaxLength(256);

                    b.Property<DateTime>("DateTime");

                    b.Property<string>("Transaction")
                        .HasMaxLength(256);

                    b.Property<byte>("Type");

                    b.Property<int?>("WalletId");

                    b.HasKey("PoolId", "ExternalId");

                    b.HasIndex("WalletId");

                    b.ToTable("PoolPayments");
                });

            modelBuilder.Entity("Msv.AutoMiner.Data.Rig", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

                    b.Property<byte>("Activity");

                    b.Property<byte[]>("ClientCertificateSerial")
                        .HasMaxLength(256);

                    b.Property<byte[]>("ClientCertificateThumbprint")
                        .HasMaxLength(256);

                    b.Property<DateTime>("Created");

                    b.Property<byte>("DifficultyAggregationType");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(64);

                    b.Property<byte>("PriceAggregationType");

                    b.Property<string>("RegistrationPassword")
                        .HasMaxLength(32);

                    b.HasKey("Id");

                    b.ToTable("Rigs");
                });

            modelBuilder.Entity("Msv.AutoMiner.Data.RigCommand", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("Created");

                    b.Property<int>("RigId");

                    b.Property<DateTime?>("Sent");

                    b.Property<int>("Type");

                    b.HasKey("Id");

                    b.ToTable("RigCommands");
                });

            modelBuilder.Entity("Msv.AutoMiner.Data.RigHeartbeat", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("ContentsJson")
                        .HasMaxLength(32768);

                    b.Property<DateTime>("Received");

                    b.Property<string>("RemoteAddress")
                        .HasMaxLength(64);

                    b.Property<int>("RigId");

                    b.HasKey("Id");

                    b.ToTable("RigHeartbeats");
                });

            modelBuilder.Entity("Msv.AutoMiner.Data.RigMiningState", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

                    b.Property<Guid>("CoinId");

                    b.Property<DateTime>("DateTime");

                    b.Property<long>("HashRate");

                    b.Property<int>("InvalidShares");

                    b.Property<int>("RigId");

                    b.Property<int>("ValidShares");

                    b.HasKey("Id");

                    b.ToTable("RigMiningStates");
                });

            modelBuilder.Entity("Msv.AutoMiner.Data.Setting", b =>
                {
                    b.Property<string>("Key")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(32);

                    b.Property<string>("Value")
                        .HasMaxLength(1024);

                    b.HasKey("Key");

                    b.ToTable("Settings");
                });

            modelBuilder.Entity("Msv.AutoMiner.Data.TelegramUser", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("UserName")
                        .HasMaxLength(64);

                    b.HasKey("Id");

                    b.ToTable("TelegramUsers");
                });

            modelBuilder.Entity("Msv.AutoMiner.Data.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

                    b.Property<byte>("Activity");

                    b.Property<string>("Login")
                        .IsRequired()
                        .HasMaxLength(64);

                    b.Property<string>("Name")
                        .HasMaxLength(64);

                    b.Property<byte[]>("PasswordHash")
                        .HasMaxLength(32);

                    b.Property<int>("Role");

                    b.Property<byte[]>("Salt")
                        .HasMaxLength(32);

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Msv.AutoMiner.Data.UserLogin", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("DateTime");

                    b.Property<string>("RemoteAddress")
                        .IsRequired()
                        .HasMaxLength(64);

                    b.Property<int>("UserId");

                    b.HasKey("Id");

                    b.ToTable("UserLogins");
                });

            modelBuilder.Entity("Msv.AutoMiner.Data.Wallet", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

                    b.Property<byte>("Activity");

                    b.Property<string>("Address")
                        .HasMaxLength(256);

                    b.Property<byte>("BalanceSource");

                    b.Property<Guid>("CoinId");

                    b.Property<DateTime>("Created");

                    b.Property<int?>("ExchangeType");

                    b.Property<bool>("IsMiningTarget");

                    b.HasKey("Id");

                    b.HasIndex("CoinId");

                    b.HasIndex("ExchangeType");

                    b.ToTable("Wallets");
                });

            modelBuilder.Entity("Msv.AutoMiner.Data.WalletBalance", b =>
                {
                    b.Property<int>("WalletId");

                    b.Property<DateTime>("DateTime");

                    b.Property<double>("Balance");

                    b.Property<double>("BlockedBalance");

                    b.Property<double>("UnconfirmedBalance");

                    b.HasKey("WalletId", "DateTime");

                    b.ToTable("WalletBalances");
                });

            modelBuilder.Entity("Msv.AutoMiner.Data.WalletOperation", b =>
                {
                    b.Property<int>("WalletId");

                    b.Property<string>("ExternalId")
                        .HasMaxLength(64);

                    b.Property<double>("Amount");

                    b.Property<DateTime>("DateTime");

                    b.Property<string>("TargetAddress")
                        .HasMaxLength(256);

                    b.Property<string>("Transaction")
                        .HasMaxLength(256);

                    b.HasKey("WalletId", "ExternalId");

                    b.ToTable("WalletOperations");
                });

            modelBuilder.Entity("Msv.AutoMiner.Data.Coin", b =>
                {
                    b.HasOne("Msv.AutoMiner.Data.CoinAlgorithm", "Algorithm")
                        .WithMany()
                        .HasForeignKey("AlgorithmId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Msv.AutoMiner.Data.CoinAlgorithm", b =>
                {
                    b.HasOne("Msv.AutoMiner.Data.Miner", "Miner")
                        .WithMany()
                        .HasForeignKey("MinerId");
                });

            modelBuilder.Entity("Msv.AutoMiner.Data.CoinFiatValue", b =>
                {
                    b.HasOne("Msv.AutoMiner.Data.Coin", "Coin")
                        .WithMany()
                        .HasForeignKey("CoinId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Msv.AutoMiner.Data.FiatCurrency", "FiatCurrency")
                        .WithMany()
                        .HasForeignKey("FiatCurrencyId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Msv.AutoMiner.Data.CoinNetworkInfo", b =>
                {
                    b.HasOne("Msv.AutoMiner.Data.Coin", "Coin")
                        .WithMany()
                        .HasForeignKey("CoinId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Msv.AutoMiner.Data.CoinProfitability", b =>
                {
                    b.HasOne("Msv.AutoMiner.Data.Coin", "Coin")
                        .WithMany()
                        .HasForeignKey("CoinId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Msv.AutoMiner.Data.Pool", "Pool")
                        .WithMany()
                        .HasForeignKey("PoolId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Msv.AutoMiner.Data.ExchangeCurrency", b =>
                {
                    b.HasOne("Msv.AutoMiner.Data.Coin", "Coin")
                        .WithMany()
                        .HasForeignKey("CoinId");
                });

            modelBuilder.Entity("Msv.AutoMiner.Data.ExchangeMarketPrice", b =>
                {
                    b.HasOne("Msv.AutoMiner.Data.Exchange", "Exchange")
                        .WithMany()
                        .HasForeignKey("ExchangeType")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Msv.AutoMiner.Data.Coin", "SourceCoin")
                        .WithMany()
                        .HasForeignKey("SourceCoinId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Msv.AutoMiner.Data.Coin", "TargetCoin")
                        .WithMany()
                        .HasForeignKey("TargetCoinId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Msv.AutoMiner.Data.MinerVersion", b =>
                {
                    b.HasOne("Msv.AutoMiner.Data.Miner", "Miner")
                        .WithMany()
                        .HasForeignKey("MinerId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Msv.AutoMiner.Data.MultiCoinPoolCurrency", b =>
                {
                    b.HasOne("Msv.AutoMiner.Data.MultiCoinPool", "MultiCoinPool")
                        .WithMany()
                        .HasForeignKey("MultiCoinPoolId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Msv.AutoMiner.Data.Pool", b =>
                {
                    b.HasOne("Msv.AutoMiner.Data.Coin", "Coin")
                        .WithMany()
                        .HasForeignKey("CoinId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Msv.AutoMiner.Data.PoolAccountState", b =>
                {
                    b.HasOne("Msv.AutoMiner.Data.Pool", "Pool")
                        .WithMany()
                        .HasForeignKey("PoolId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Msv.AutoMiner.Data.PoolPayment", b =>
                {
                    b.HasOne("Msv.AutoMiner.Data.Pool", "Pool")
                        .WithMany()
                        .HasForeignKey("PoolId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Msv.AutoMiner.Data.Wallet", "Wallet")
                        .WithMany()
                        .HasForeignKey("WalletId");
                });

            modelBuilder.Entity("Msv.AutoMiner.Data.Wallet", b =>
                {
                    b.HasOne("Msv.AutoMiner.Data.Coin", "Coin")
                        .WithMany("Wallets")
                        .HasForeignKey("CoinId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Msv.AutoMiner.Data.Exchange", "Exchange")
                        .WithMany()
                        .HasForeignKey("ExchangeType");
                });

            modelBuilder.Entity("Msv.AutoMiner.Data.WalletBalance", b =>
                {
                    b.HasOne("Msv.AutoMiner.Data.Wallet", "Wallet")
                        .WithMany()
                        .HasForeignKey("WalletId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Msv.AutoMiner.Data.WalletOperation", b =>
                {
                    b.HasOne("Msv.AutoMiner.Data.Wallet", "Wallet")
                        .WithMany()
                        .HasForeignKey("WalletId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
