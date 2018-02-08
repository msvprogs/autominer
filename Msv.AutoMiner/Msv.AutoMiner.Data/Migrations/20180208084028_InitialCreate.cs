using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Msv.AutoMiner.Data.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApiKeys",
                columns: table => new
                {
                    Key = table.Column<string>(maxLength: 128, nullable: false),
                    Expires = table.Column<DateTime>(nullable: true),
                    Type = table.Column<byte>(nullable: false),
                    UsagesLeft = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiKeys", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "Exchanges",
                columns: table => new
                {
                    Type = table.Column<int>(nullable: false),
                    Activity = table.Column<byte>(nullable: false),
                    PrivateKey = table.Column<string>(maxLength: 256, nullable: true),
                    PublicKey = table.Column<string>(maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exchanges", x => x.Type);
                });

            migrationBuilder.CreateTable(
                name: "FiatCurrencies",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(maxLength: 32, nullable: false),
                    Symbol = table.Column<string>(maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FiatCurrencies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Miners",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Activity = table.Column<byte>(nullable: false),
                    Name = table.Column<string>(maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Miners", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RigCommands",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Created = table.Column<DateTime>(nullable: false),
                    RigId = table.Column<int>(nullable: false),
                    Sent = table.Column<DateTime>(nullable: true),
                    Type = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RigCommands", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RigHeartbeats",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ContentsJson = table.Column<string>(maxLength: 32768, nullable: true),
                    Received = table.Column<DateTime>(nullable: false),
                    RemoteAddress = table.Column<string>(maxLength: 64, nullable: true),
                    RigId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RigHeartbeats", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RigMiningStates",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CoinId = table.Column<Guid>(nullable: false),
                    DateTime = table.Column<DateTime>(nullable: false),
                    HashRate = table.Column<long>(nullable: false),
                    InvalidShares = table.Column<int>(nullable: false),
                    RigId = table.Column<int>(nullable: false),
                    ValidShares = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RigMiningStates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Rigs",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Activity = table.Column<byte>(nullable: false),
                    ClientCertificateSerial = table.Column<byte[]>(maxLength: 256, nullable: true),
                    ClientCertificateThumbprint = table.Column<byte[]>(maxLength: 256, nullable: true),
                    Created = table.Column<DateTime>(nullable: false),
                    DifficultyAggregationType = table.Column<byte>(nullable: false),
                    Name = table.Column<string>(maxLength: 64, nullable: false),
                    PriceAggregationType = table.Column<byte>(nullable: false),
                    RegistrationPassword = table.Column<string>(maxLength: 32, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rigs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TelegramUsers",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserName = table.Column<string>(maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TelegramUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserLogins",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    DateTime = table.Column<DateTime>(nullable: false),
                    RemoteAddress = table.Column<string>(maxLength: 64, nullable: false),
                    UserId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLogins", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Activity = table.Column<byte>(nullable: false),
                    Login = table.Column<string>(maxLength: 64, nullable: false),
                    Name = table.Column<string>(maxLength: 64, nullable: true),
                    PasswordHash = table.Column<byte[]>(maxLength: 32, nullable: true),
                    Role = table.Column<int>(nullable: false),
                    Salt = table.Column<byte[]>(maxLength: 32, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CoinAlgorithms",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Activity = table.Column<byte>(nullable: false),
                    AdditionalArguments = table.Column<string>(maxLength: 128, nullable: true),
                    AlgorithmArgument = table.Column<string>(maxLength: 32, nullable: true),
                    Intensity = table.Column<double>(nullable: true),
                    KnownValue = table.Column<int>(nullable: true),
                    MinerId = table.Column<int>(nullable: true),
                    Name = table.Column<string>(maxLength: 64, nullable: false),
                    ProfitabilityFormulaType = table.Column<byte>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoinAlgorithms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CoinAlgorithms_Miners_MinerId",
                        column: x => x.MinerId,
                        principalTable: "Miners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MinerVersions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AdditionalArguments = table.Column<string>(maxLength: 128, nullable: true),
                    AlgorithmArgument = table.Column<string>(maxLength: 32, nullable: true),
                    ApiPort = table.Column<int>(nullable: true),
                    ApiPortArgument = table.Column<string>(maxLength: 32, nullable: true),
                    ApiType = table.Column<byte>(nullable: false),
                    BenchmarkArgument = table.Column<string>(maxLength: 32, nullable: true),
                    BenchmarkResultRegex = table.Column<string>(maxLength: 256, nullable: true),
                    ExeFilePath = table.Column<string>(maxLength: 512, nullable: true),
                    ExeSecondaryFilePath = table.Column<string>(maxLength: 512, nullable: true),
                    IntensityArgument = table.Column<string>(maxLength: 32, nullable: true),
                    InvalidShareRegex = table.Column<string>(maxLength: 256, nullable: true),
                    MinerId = table.Column<int>(nullable: false),
                    OmitUrlSchema = table.Column<bool>(nullable: false),
                    PasswordArgument = table.Column<string>(maxLength: 32, nullable: true),
                    Platform = table.Column<byte>(nullable: false),
                    PortArgument = table.Column<string>(maxLength: 32, nullable: true),
                    ServerArgument = table.Column<string>(maxLength: 32, nullable: true),
                    SpeedRegex = table.Column<string>(maxLength: 256, nullable: true),
                    Uploaded = table.Column<DateTime>(nullable: false),
                    UserArgument = table.Column<string>(maxLength: 32, nullable: true),
                    ValidShareRegex = table.Column<string>(maxLength: 256, nullable: true),
                    Version = table.Column<string>(maxLength: 32, nullable: true),
                    ZipPath = table.Column<string>(maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MinerVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MinerVersions_Miners_MinerId",
                        column: x => x.MinerId,
                        principalTable: "Miners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Coins",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Activity = table.Column<byte>(nullable: false),
                    AddressFormat = table.Column<byte>(nullable: false),
                    AddressPrefixes = table.Column<string>(maxLength: 64, nullable: true),
                    AlgorithmId = table.Column<Guid>(nullable: false),
                    GetDifficultyFromLastPoWBlock = table.Column<bool>(nullable: false),
                    LogoImageBytes = table.Column<byte[]>(maxLength: 32768, nullable: true),
                    MaxTarget = table.Column<string>(maxLength: 128, nullable: true),
                    Name = table.Column<string>(maxLength: 256, nullable: false),
                    NetworkInfoApiName = table.Column<string>(maxLength: 64, nullable: true),
                    NetworkInfoApiType = table.Column<byte>(nullable: false),
                    NetworkInfoApiUrl = table.Column<string>(maxLength: 512, nullable: true),
                    NodeHost = table.Column<string>(maxLength: 512, nullable: true),
                    NodeLogin = table.Column<string>(maxLength: 64, nullable: true),
                    NodePassword = table.Column<string>(maxLength: 64, nullable: true),
                    NodePort = table.Column<int>(nullable: false),
                    RewardCalculationJavaScript = table.Column<string>(maxLength: 16384, nullable: true),
                    Symbol = table.Column<string>(maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Coins", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Coins_CoinAlgorithms_AlgorithmId",
                        column: x => x.AlgorithmId,
                        principalTable: "CoinAlgorithms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CoinFiatValues",
                columns: table => new
                {
                    CoinId = table.Column<Guid>(nullable: false),
                    FiatCurrencyId = table.Column<int>(nullable: false),
                    DateTime = table.Column<DateTime>(nullable: false),
                    Source = table.Column<byte>(nullable: false),
                    Value = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoinFiatValues", x => new { x.CoinId, x.FiatCurrencyId, x.DateTime, x.Source });
                    table.ForeignKey(
                        name: "FK_CoinFiatValues_Coins_CoinId",
                        column: x => x.CoinId,
                        principalTable: "Coins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CoinFiatValues_FiatCurrencies_FiatCurrencyId",
                        column: x => x.FiatCurrencyId,
                        principalTable: "FiatCurrencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CoinNetworkInfos",
                columns: table => new
                {
                    CoinId = table.Column<Guid>(nullable: false),
                    Created = table.Column<DateTime>(nullable: false),
                    BlockReward = table.Column<double>(nullable: false),
                    BlockTimeSeconds = table.Column<double>(nullable: false),
                    Difficulty = table.Column<double>(nullable: false),
                    Height = table.Column<long>(nullable: false),
                    LastBlockTime = table.Column<DateTime>(nullable: true),
                    MasternodeCount = table.Column<int>(nullable: true),
                    NetHashRate = table.Column<double>(nullable: false),
                    TotalSupply = table.Column<double>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoinNetworkInfos", x => new { x.CoinId, x.Created });
                    table.ForeignKey(
                        name: "FK_CoinNetworkInfos_Coins_CoinId",
                        column: x => x.CoinId,
                        principalTable: "Coins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExchangeCoins",
                columns: table => new
                {
                    CoinId = table.Column<Guid>(nullable: false),
                    Exchange = table.Column<int>(nullable: false),
                    DateTime = table.Column<DateTime>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    MinWithdrawAmount = table.Column<double>(nullable: false),
                    WithdrawalFee = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExchangeCoins", x => new { x.CoinId, x.Exchange });
                    table.ForeignKey(
                        name: "FK_ExchangeCoins_Coins_CoinId",
                        column: x => x.CoinId,
                        principalTable: "Coins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExchangeMarketPrices",
                columns: table => new
                {
                    SourceCoinId = table.Column<Guid>(nullable: false),
                    TargetCoinId = table.Column<Guid>(nullable: false),
                    Exchange = table.Column<int>(nullable: false),
                    DateTime = table.Column<DateTime>(nullable: false),
                    BuyFeePercent = table.Column<double>(nullable: false),
                    HighestBid = table.Column<double>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    LastDayHigh = table.Column<double>(nullable: false),
                    LastDayLow = table.Column<double>(nullable: false),
                    LastDayVolume = table.Column<double>(nullable: false),
                    LastPrice = table.Column<double>(nullable: false),
                    LowestAsk = table.Column<double>(nullable: false),
                    SellFeePercent = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExchangeMarketPrices", x => new { x.SourceCoinId, x.TargetCoinId, x.Exchange, x.DateTime });
                    table.ForeignKey(
                        name: "FK_ExchangeMarketPrices_Exchanges_Exchange",
                        column: x => x.Exchange,
                        principalTable: "Exchanges",
                        principalColumn: "Type",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExchangeMarketPrices_Coins_SourceCoinId",
                        column: x => x.SourceCoinId,
                        principalTable: "Coins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExchangeMarketPrices_Coins_TargetCoinId",
                        column: x => x.TargetCoinId,
                        principalTable: "Coins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Pools",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Activity = table.Column<byte>(nullable: false),
                    ApiKey = table.Column<string>(maxLength: 256, nullable: true),
                    ApiPoolName = table.Column<string>(maxLength: 64, nullable: true),
                    ApiProtocol = table.Column<int>(nullable: false),
                    ApiUrl = table.Column<string>(maxLength: 256, nullable: true),
                    Availability = table.Column<byte>(nullable: false),
                    CoinId = table.Column<Guid>(nullable: false),
                    FeeRatio = table.Column<double>(nullable: false),
                    Host = table.Column<string>(maxLength: 128, nullable: false),
                    IsAnonymous = table.Column<bool>(nullable: false),
                    Name = table.Column<string>(maxLength: 128, nullable: false),
                    PoolUserId = table.Column<int>(nullable: true),
                    Port = table.Column<int>(nullable: false),
                    Priority = table.Column<int>(nullable: false),
                    Protocol = table.Column<byte>(nullable: false),
                    ResponsesStoppedDate = table.Column<DateTime>(nullable: true),
                    TimeZoneCorrectionHours = table.Column<double>(nullable: false),
                    UseBtcWallet = table.Column<bool>(nullable: false),
                    WorkerLogin = table.Column<string>(maxLength: 128, nullable: true),
                    WorkerPassword = table.Column<string>(maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pools", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pools_Coins_CoinId",
                        column: x => x.CoinId,
                        principalTable: "Coins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Wallets",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Activity = table.Column<byte>(nullable: false),
                    Address = table.Column<string>(maxLength: 256, nullable: true),
                    CoinId = table.Column<Guid>(nullable: false),
                    Created = table.Column<DateTime>(nullable: false),
                    ExchangeType = table.Column<int>(nullable: true),
                    IsMiningTarget = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wallets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Wallets_Coins_CoinId",
                        column: x => x.CoinId,
                        principalTable: "Coins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Wallets_Exchanges_ExchangeType",
                        column: x => x.ExchangeType,
                        principalTable: "Exchanges",
                        principalColumn: "Type",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CoinProfitabilities",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    BtcPerDay = table.Column<double>(nullable: false),
                    CoinId = table.Column<Guid>(nullable: false),
                    CoinsPerDay = table.Column<double>(nullable: false),
                    ElectricityCost = table.Column<double>(nullable: false),
                    PoolId = table.Column<int>(nullable: false),
                    Requested = table.Column<DateTime>(nullable: false),
                    RigId = table.Column<int>(nullable: false),
                    UsdPerDay = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoinProfitabilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CoinProfitabilities_Coins_CoinId",
                        column: x => x.CoinId,
                        principalTable: "Coins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CoinProfitabilities_Pools_PoolId",
                        column: x => x.PoolId,
                        principalTable: "Pools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PoolAccountStates",
                columns: table => new
                {
                    PoolId = table.Column<int>(nullable: false),
                    DateTime = table.Column<DateTime>(nullable: false),
                    ConfirmedBalance = table.Column<double>(nullable: false),
                    HashRate = table.Column<long>(nullable: false),
                    InvalidShares = table.Column<int>(nullable: false),
                    PoolHashRate = table.Column<long>(nullable: false),
                    PoolLastBlock = table.Column<long>(nullable: true),
                    PoolWorkers = table.Column<int>(nullable: false),
                    UnconfirmedBalance = table.Column<double>(nullable: false),
                    ValidShares = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PoolAccountStates", x => new { x.PoolId, x.DateTime });
                    table.ForeignKey(
                        name: "FK_PoolAccountStates_Pools_PoolId",
                        column: x => x.PoolId,
                        principalTable: "Pools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PoolPayments",
                columns: table => new
                {
                    PoolId = table.Column<int>(nullable: false),
                    ExternalId = table.Column<string>(maxLength: 64, nullable: false),
                    Amount = table.Column<double>(nullable: false),
                    BlockHash = table.Column<string>(maxLength: 256, nullable: true),
                    CoinAddress = table.Column<string>(maxLength: 256, nullable: true),
                    DateTime = table.Column<DateTime>(nullable: false),
                    Transaction = table.Column<string>(maxLength: 256, nullable: true),
                    Type = table.Column<byte>(nullable: false),
                    WalletId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PoolPayments", x => new { x.PoolId, x.ExternalId });
                    table.ForeignKey(
                        name: "FK_PoolPayments_Pools_PoolId",
                        column: x => x.PoolId,
                        principalTable: "Pools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PoolPayments_Wallets_WalletId",
                        column: x => x.WalletId,
                        principalTable: "Wallets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WalletBalances",
                columns: table => new
                {
                    WalletId = table.Column<int>(nullable: false),
                    DateTime = table.Column<DateTime>(nullable: false),
                    Balance = table.Column<double>(nullable: false),
                    BlockedBalance = table.Column<double>(nullable: false),
                    UnconfirmedBalance = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WalletBalances", x => new { x.WalletId, x.DateTime });
                    table.ForeignKey(
                        name: "FK_WalletBalances_Wallets_WalletId",
                        column: x => x.WalletId,
                        principalTable: "Wallets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WalletOperations",
                columns: table => new
                {
                    WalletId = table.Column<int>(nullable: false),
                    ExternalId = table.Column<string>(maxLength: 64, nullable: false),
                    Amount = table.Column<double>(nullable: false),
                    DateTime = table.Column<DateTime>(nullable: false),
                    TargetAddress = table.Column<string>(maxLength: 256, nullable: true),
                    Transaction = table.Column<string>(maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WalletOperations", x => new { x.WalletId, x.ExternalId });
                    table.ForeignKey(
                        name: "FK_WalletOperations_Wallets_WalletId",
                        column: x => x.WalletId,
                        principalTable: "Wallets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CoinAlgorithms_MinerId",
                table: "CoinAlgorithms",
                column: "MinerId");

            migrationBuilder.CreateIndex(
                name: "IX_CoinFiatValues_FiatCurrencyId",
                table: "CoinFiatValues",
                column: "FiatCurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_CoinProfitabilities_CoinId",
                table: "CoinProfitabilities",
                column: "CoinId");

            migrationBuilder.CreateIndex(
                name: "IX_CoinProfitabilities_PoolId",
                table: "CoinProfitabilities",
                column: "PoolId");

            migrationBuilder.CreateIndex(
                name: "IX_Coins_AlgorithmId",
                table: "Coins",
                column: "AlgorithmId");

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeMarketPrices_Exchange",
                table: "ExchangeMarketPrices",
                column: "Exchange");

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeMarketPrices_TargetCoinId",
                table: "ExchangeMarketPrices",
                column: "TargetCoinId");

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeMarketPrices_SourceCoinId_TargetCoinId_Exchange_DateTime",
                table: "ExchangeMarketPrices",
                columns: new[] { "SourceCoinId", "TargetCoinId", "Exchange", "DateTime" });

            migrationBuilder.CreateIndex(
                name: "IX_MinerVersions_MinerId",
                table: "MinerVersions",
                column: "MinerId");

            migrationBuilder.CreateIndex(
                name: "IX_PoolPayments_WalletId",
                table: "PoolPayments",
                column: "WalletId");

            migrationBuilder.CreateIndex(
                name: "IX_Pools_CoinId",
                table: "Pools",
                column: "CoinId");

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_CoinId",
                table: "Wallets",
                column: "CoinId");

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_ExchangeType",
                table: "Wallets",
                column: "ExchangeType");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApiKeys");

            migrationBuilder.DropTable(
                name: "CoinFiatValues");

            migrationBuilder.DropTable(
                name: "CoinNetworkInfos");

            migrationBuilder.DropTable(
                name: "CoinProfitabilities");

            migrationBuilder.DropTable(
                name: "ExchangeCoins");

            migrationBuilder.DropTable(
                name: "ExchangeMarketPrices");

            migrationBuilder.DropTable(
                name: "MinerVersions");

            migrationBuilder.DropTable(
                name: "PoolAccountStates");

            migrationBuilder.DropTable(
                name: "PoolPayments");

            migrationBuilder.DropTable(
                name: "RigCommands");

            migrationBuilder.DropTable(
                name: "RigHeartbeats");

            migrationBuilder.DropTable(
                name: "RigMiningStates");

            migrationBuilder.DropTable(
                name: "Rigs");

            migrationBuilder.DropTable(
                name: "TelegramUsers");

            migrationBuilder.DropTable(
                name: "UserLogins");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "WalletBalances");

            migrationBuilder.DropTable(
                name: "WalletOperations");

            migrationBuilder.DropTable(
                name: "FiatCurrencies");

            migrationBuilder.DropTable(
                name: "Pools");

            migrationBuilder.DropTable(
                name: "Wallets");

            migrationBuilder.DropTable(
                name: "Coins");

            migrationBuilder.DropTable(
                name: "Exchanges");

            migrationBuilder.DropTable(
                name: "CoinAlgorithms");

            migrationBuilder.DropTable(
                name: "Miners");
        }
    }
}
