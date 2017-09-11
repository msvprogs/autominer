-- 
-- Отключение внешних ключей
-- 
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;

-- 
-- Установить режим SQL (SQL mode)
-- 
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;

-- 
-- Установка кодировки, с использованием которой клиент будет посылать запросы на сервер
--
SET NAMES 'utf8';

-- 
-- Установка базы данных по умолчанию
--
USE autominer;

--
-- Описание для таблицы algorithmdatas
--
DROP TABLE IF EXISTS algorithmdatas;
CREATE TABLE algorithmdatas (
  Algorithm INT(11) NOT NULL,
  SpeedInHashes BIGINT(20) NOT NULL,
  Multiplier BIGINT(20) NOT NULL,
  Power DOUBLE NOT NULL,
  PRIMARY KEY (Algorithm)
)
ENGINE = INNODB
AVG_ROW_LENGTH = 496
CHARACTER SET utf8
COLLATE utf8_general_ci
ROW_FORMAT = DYNAMIC;

--
-- Описание для таблицы exchangeaccountbalances
--
DROP TABLE IF EXISTS exchangeaccountbalances;
CREATE TABLE exchangeaccountbalances (
  CoinId INT(11) NOT NULL,
  DateTime DATETIME NOT NULL,
  Exchange INT(11) NOT NULL,
  Balance DOUBLE NOT NULL,
  PendingBalance DOUBLE NOT NULL,
  BalanceOnOrders DOUBLE NOT NULL,
  PRIMARY KEY (CoinId, DateTime, Exchange)
)
ENGINE = INNODB
AVG_ROW_LENGTH = 79
CHARACTER SET latin1
COLLATE latin1_swedish_ci
ROW_FORMAT = DYNAMIC;

--
-- Описание для таблицы exchangeaccountoperations
--
DROP TABLE IF EXISTS exchangeaccountoperations;
CREATE TABLE exchangeaccountoperations (
  Id INT(11) NOT NULL AUTO_INCREMENT,
  DateTime DATETIME NOT NULL,
  Exchange INT(11) NOT NULL,
  CoinId INT(11) NOT NULL,
  Amount DOUBLE NOT NULL,
  PRIMARY KEY (Id)
)
ENGINE = INNODB
AUTO_INCREMENT = 1962
AVG_ROW_LENGTH = 61
CHARACTER SET latin1
COLLATE latin1_swedish_ci
ROW_FORMAT = DYNAMIC;

--
-- Описание для таблицы exchanges
--
DROP TABLE IF EXISTS exchanges;
CREATE TABLE exchanges (
  Type INT(11) NOT NULL,
  PublicKey VARCHAR(256) NOT NULL,
  PrivateKey VARBINARY(1024) NOT NULL,
  PRIMARY KEY (Type)
)
ENGINE = INNODB
AVG_ROW_LENGTH = 8192
CHARACTER SET latin1
COLLATE latin1_swedish_ci
ROW_FORMAT = DYNAMIC;

--
-- Описание для таблицы miners
--
DROP TABLE IF EXISTS miners;
CREATE TABLE miners (
  Id INT(11) NOT NULL AUTO_INCREMENT,
  Name VARCHAR(256) NOT NULL,
  FileName VARCHAR(512) NOT NULL,
  SecondaryFileName VARCHAR(512) DEFAULT NULL,
  ServerArgument VARCHAR(256) NOT NULL,
  PortArgument VARCHAR(256) DEFAULT NULL,
  UserArgument VARCHAR(256) NOT NULL,
  PasswordArgument VARCHAR(256) DEFAULT NULL,
  OtherArguments VARCHAR(512) DEFAULT NULL,
  LogFileArgument VARCHAR(256) DEFAULT NULL,
  ReadOutputFromLog TINYINT(1) NOT NULL,
  SpeedRegex VARCHAR(512) DEFAULT NULL,
  AlgorithmArgument VARCHAR(256) DEFAULT NULL,
  AlternativeServerArgument VARCHAR(256) DEFAULT NULL,
  AlternativeUserArgument VARCHAR(256) DEFAULT NULL,
  AlternativePasswordArgument VARCHAR(10) DEFAULT NULL,
  IntensityArgument VARCHAR(255) DEFAULT NULL,
  DifficultyMultiplierArgument VARCHAR(255) DEFAULT NULL,
  ValidShareRegex VARCHAR(255) DEFAULT NULL,
  OmitUrlSchema BIT(1) NOT NULL DEFAULT b'0',
  PRIMARY KEY (Id)
)
ENGINE = INNODB
AUTO_INCREMENT = 19
AVG_ROW_LENGTH = 1365
CHARACTER SET utf8
COLLATE utf8_general_ci
ROW_FORMAT = DYNAMIC;

--
-- Описание для таблицы poolaccountstates
--
DROP TABLE IF EXISTS poolaccountstates;
CREATE TABLE poolaccountstates (
  PoolId INT(11) NOT NULL,
  DateTime DATETIME NOT NULL,
  ConfirmedBalance DOUBLE NOT NULL,
  UnconfirmedBalance DOUBLE NOT NULL,
  HashRate BIGINT(20) NOT NULL,
  ValidShares INT(11) NOT NULL,
  InvalidShares INT(11) NOT NULL,
  PoolHashRate BIGINT(20) NOT NULL,
  PoolWorkers INT(11) NOT NULL,
  PoolLastBlock BIGINT(20) DEFAULT NULL,
  PRIMARY KEY (PoolId, DateTime)
)
ENGINE = INNODB
AVG_ROW_LENGTH = 109
CHARACTER SET latin1
COLLATE latin1_swedish_ci
ROW_FORMAT = DYNAMIC;

--
-- Описание для таблицы algorithmpairdatas
--
DROP TABLE IF EXISTS algorithmpairdatas;
CREATE TABLE algorithmpairdatas (
  Algorithm1 INT(11) NOT NULL,
  Algorithm2 INT(11) NOT NULL,
  SpeedInHashes1 BIGINT(20) NOT NULL,
  SpeedInHashes2 BIGINT(20) NOT NULL,
  MinerId INT(11) NOT NULL,
  IsActive BIT(1) NOT NULL,
  CONSTRAINT FK_algorithmpairdatas_miners FOREIGN KEY (MinerId)
    REFERENCES miners(Id) ON DELETE CASCADE ON UPDATE NO ACTION
)
ENGINE = INNODB
AVG_ROW_LENGTH = 5461
CHARACTER SET latin1
COLLATE latin1_swedish_ci
ROW_FORMAT = DYNAMIC;

--
-- Описание для таблицы mineralgorithmvalues
--
DROP TABLE IF EXISTS mineralgorithmvalues;
CREATE TABLE mineralgorithmvalues (
  Algorithm INT(11) NOT NULL,
  MinerId INT(11) NOT NULL,
  Value TEXT NOT NULL,
  PRIMARY KEY (Algorithm, MinerId),
  CONSTRAINT FK_mineralgorithmvalues_miners FOREIGN KEY (MinerId)
    REFERENCES miners(Id) ON DELETE CASCADE ON UPDATE NO ACTION
)
ENGINE = INNODB
AVG_ROW_LENGTH = 496
CHARACTER SET latin1
COLLATE latin1_swedish_ci
ROW_FORMAT = DYNAMIC;

--
-- Описание для таблицы minerspeeds
--
DROP TABLE IF EXISTS minerspeeds;
CREATE TABLE minerspeeds (
  Id INT(11) NOT NULL AUTO_INCREMENT,
  MinerId INT(11) NOT NULL,
  DateTime DATETIME NOT NULL,
  SpeedHashesPerSecond BIGINT(20) NOT NULL,
  PRIMARY KEY (Id),
  INDEX IX_MinerId (MinerId),
  CONSTRAINT FK_MinerSpeeds_Miners_MinerId FOREIGN KEY (MinerId)
    REFERENCES miners(Id) ON DELETE CASCADE ON UPDATE CASCADE
)
ENGINE = INNODB
AUTO_INCREMENT = 1
CHARACTER SET utf8
COLLATE utf8_general_ci
ROW_FORMAT = DYNAMIC;

--
-- Описание для таблицы pools
--
DROP TABLE IF EXISTS pools;
CREATE TABLE pools (
  Id INT(11) NOT NULL AUTO_INCREMENT,
  Name LONGTEXT NOT NULL,
  Address LONGTEXT NOT NULL,
  Port INT(11) NOT NULL,
  WorkerLogin LONGTEXT NOT NULL,
  WorkerPassword LONGTEXT DEFAULT NULL,
  IsAnonymous TINYINT(1) NOT NULL,
  MinerId INT(11) NOT NULL,
  LogFile LONGTEXT DEFAULT NULL,
  FeeRatio DECIMAL(18, 2) NOT NULL DEFAULT 0.00,
  ApiProtocol INT(11) NOT NULL DEFAULT 0,
  PoolUserId INT(11) DEFAULT NULL,
  ApiKey VARCHAR(256) DEFAULT NULL,
  ApiUrl VARCHAR(512) DEFAULT NULL,
  ResponsesStoppedDate DATETIME DEFAULT NULL,
  Intensity DECIMAL(10, 2) DEFAULT NULL,
  DifficultyMultiplier INT(11) DEFAULT NULL,
  ApiPoolName VARCHAR(255) DEFAULT NULL,
  Activity TINYINT(1) NOT NULL DEFAULT 0,
  Protocol TINYINT(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (Id),
  INDEX IX_MinerId (MinerId),
  CONSTRAINT FK_Pools_Miners_MinerId FOREIGN KEY (MinerId)
    REFERENCES miners(Id) ON DELETE CASCADE ON UPDATE CASCADE
)
ENGINE = INNODB
AUTO_INCREMENT = 85
AVG_ROW_LENGTH = 227
CHARACTER SET utf8
COLLATE utf8_general_ci
ROW_FORMAT = DYNAMIC;

--
-- Описание для таблицы coins
--
DROP TABLE IF EXISTS coins;
CREATE TABLE coins (
  Id INT(11) NOT NULL AUTO_INCREMENT,
  Name LONGTEXT NOT NULL,
  CurrencySymbol LONGTEXT NOT NULL,
  Algorithm INT(11) NOT NULL,
  Difficulty DOUBLE NOT NULL DEFAULT 0,
  NetHashRate BIGINT(20) NOT NULL DEFAULT 0,
  BlockReward DOUBLE NOT NULL DEFAULT 0,
  BlockTimeSeconds DOUBLE NOT NULL DEFAULT 0,
  Height BIGINT(20) NOT NULL,
  StatsUpdated DATETIME DEFAULT NULL,
  Exchange INT(11) NOT NULL,
  Wallet LONGTEXT NOT NULL,
  PoolId INT(11) DEFAULT NULL,
  ProfitByAskPrice BIT(1) NOT NULL DEFAULT b'0',
  SolsPerDiff INT(11) DEFAULT NULL,
  Activity TINYINT(1) NOT NULL DEFAULT 0,
  UseLocalNetworkInfo BIT(1) NOT NULL DEFAULT b'0',
  PRIMARY KEY (Id),
  INDEX IX_PoolId (PoolId),
  CONSTRAINT FK_Coins_Pools_PoolId FOREIGN KEY (PoolId)
    REFERENCES pools(Id) ON DELETE CASCADE ON UPDATE CASCADE
)
ENGINE = INNODB
AUTO_INCREMENT = 78
AVG_ROW_LENGTH = 221
CHARACTER SET utf8
COLLATE utf8_general_ci
ROW_FORMAT = DYNAMIC;

--
-- Описание для таблицы poolpayments
--
DROP TABLE IF EXISTS poolpayments;
CREATE TABLE poolpayments (
  PoolId INT(11) NOT NULL,
  DateTime DATETIME NOT NULL,
  Amount DOUBLE NOT NULL,
  Transaction VARCHAR(512) DEFAULT NULL,
  PRIMARY KEY (PoolId, DateTime),
  CONSTRAINT FK_poolpayments_pools FOREIGN KEY (PoolId)
    REFERENCES pools(Id) ON DELETE CASCADE ON UPDATE NO ACTION
)
ENGINE = INNODB
AVG_ROW_LENGTH = 154
CHARACTER SET latin1
COLLATE latin1_swedish_ci
ROW_FORMAT = DYNAMIC;

--
-- Описание для таблицы coinbtcprices
--
DROP TABLE IF EXISTS coinbtcprices;
CREATE TABLE coinbtcprices (
  CoinId INT(11) NOT NULL,
  DateTime DATETIME NOT NULL,
  HighestBid DOUBLE NOT NULL,
  LowestAsk DOUBLE NOT NULL,
  PRIMARY KEY (CoinId, DateTime),
  CONSTRAINT FK_coinbtcvalues_CoinId FOREIGN KEY (CoinId)
    REFERENCES coins(Id) ON DELETE NO ACTION ON UPDATE RESTRICT
)
ENGINE = INNODB
AVG_ROW_LENGTH = 103
CHARACTER SET latin1
COLLATE latin1_swedish_ci
ROW_FORMAT = DYNAMIC;

--
-- Описание для таблицы coinprofitabilities
--
DROP TABLE IF EXISTS coinprofitabilities;
CREATE TABLE coinprofitabilities (
  Id INT(11) NOT NULL AUTO_INCREMENT,
  CoinId INT(11) NOT NULL,
  DateTime DATETIME NOT NULL,
  CoinsPerDay DOUBLE NOT NULL,
  BtcPerDay DOUBLE NOT NULL,
  UsdPerDay DECIMAL(27, 4) NOT NULL,
  PRIMARY KEY (Id),
  INDEX IX_CoinId (CoinId),
  CONSTRAINT FK_CoinProfitabilities_Coins_CoinId FOREIGN KEY (CoinId)
    REFERENCES coins(Id) ON DELETE CASCADE ON UPDATE CASCADE
)
ENGINE = INNODB
AUTO_INCREMENT = 136516
AVG_ROW_LENGTH = 67
CHARACTER SET utf8
COLLATE utf8_general_ci
ROW_FORMAT = DYNAMIC;

--
-- Описание для таблицы miningchangeevents
--
DROP TABLE IF EXISTS miningchangeevents;
CREATE TABLE miningchangeevents (
  Id INT(11) NOT NULL AUTO_INCREMENT,
  DateTime DATETIME NOT NULL,
  FromCoinId INT(11) DEFAULT NULL,
  ToCoinId INT(11) NOT NULL,
  SourceProfitability DECIMAL(18, 2) DEFAULT NULL,
  TargetProfitability DECIMAL(18, 2) NOT NULL,
  PRIMARY KEY (Id),
  INDEX IX_FromCoinId (FromCoinId),
  INDEX IX_ToCoinId (ToCoinId),
  CONSTRAINT FK_MiningChangeEvents_Coins_FromCoinId FOREIGN KEY (FromCoinId)
    REFERENCES coins(Id) ON DELETE RESTRICT ON UPDATE RESTRICT,
  CONSTRAINT FK_MiningChangeEvents_Coins_ToCoinId FOREIGN KEY (ToCoinId)
    REFERENCES coins(Id) ON DELETE CASCADE ON UPDATE CASCADE
)
ENGINE = INNODB
AUTO_INCREMENT = 3183
AVG_ROW_LENGTH = 65
CHARACTER SET utf8
COLLATE utf8_general_ci
ROW_FORMAT = DYNAMIC;

--
-- Описание для представления currentaccountstate
--
DROP VIEW IF EXISTS currentaccountstate CASCADE;
CREATE OR REPLACE 
	DEFINER = 'root'@'localhost'
VIEW currentaccountstate
AS
	select distinct `c`.`Name` AS `Name`,`b`.`DateTime` AS `DateTime`,`b`.`Exchange` AS `Exchange`,`b`.`Balance` AS `Balance`,`b`.`PendingBalance` AS `PendingBalance`,`b`.`BalanceOnOrders` AS `BalanceOnOrders` from ((`autominer_test`.`exchangeaccountbalances` `b` join (select `autominer_test`.`exchangeaccountbalances`.`CoinId` AS `CoinId`,`autominer_test`.`exchangeaccountbalances`.`Exchange` AS `Exchange`,max(`autominer_test`.`exchangeaccountbalances`.`DateTime`) AS `DateTime` from `autominer_test`.`exchangeaccountbalances` group by `autominer_test`.`exchangeaccountbalances`.`CoinId`,`autominer_test`.`exchangeaccountbalances`.`Exchange`) `b2` on(((`b`.`CoinId` = `b2`.`CoinId`) and (`b`.`DateTime` = `b2`.`DateTime`)))) join `autominer_test`.`coins` `c` on((`b`.`CoinId` = `c`.`Id`))) order by `c`.`Name`;

--
-- Описание для представления currentpoolstate
--
DROP VIEW IF EXISTS currentpoolstate CASCADE;
CREATE OR REPLACE 
	DEFINER = 'root'@'localhost'
VIEW currentpoolstate
AS
	select `p`.`Name` AS `Name`,`d`.`DateTime` AS `DateTime`,`pas`.`ConfirmedBalance` AS `ConfirmedBalance`,`pas`.`UnconfirmedBalance` AS `UnconfirmedBalance`,`pas`.`HashRate` AS `HashRate`,`pas`.`PoolWorkers` AS `PoolWorkers`,`pas`.`PoolHashRate` AS `PoolHashRate` from ((((select `autominer_test`.`poolaccountstates`.`PoolId` AS `PoolId`,max(`autominer_test`.`poolaccountstates`.`DateTime`) AS `DateTime` from `autominer_test`.`poolaccountstates` group by `autominer_test`.`poolaccountstates`.`PoolId`)) `d` join `autominer_test`.`poolaccountstates` `pas` on(((`d`.`PoolId` = `pas`.`PoolId`) and (`d`.`DateTime` = `pas`.`DateTime`)))) join `autominer_test`.`pools` `p` on((`d`.`PoolId` = `p`.`Id`))) order by `p`.`Name`;

--
-- Описание для представления lastdayactivity
--
DROP VIEW IF EXISTS lastdayactivity CASCADE;
CREATE OR REPLACE 
	DEFINER = 'root'@'localhost'
VIEW lastdayactivity
AS
	select `m`.`DateTime` AS `DateTime`,ifnull(`c`.`Name`,'<start>') AS `FromCoin`,`c2`.`Name` AS `ToCoin` from ((`miningchangeevents` `m` left join `coins` `c` on((`m`.`FromCoinId` = `c`.`Id`))) left join `coins` `c2` on((`m`.`ToCoinId` = `c2`.`Id`))) where (`m`.`DateTime` > (now() - interval 1 day)) order by `m`.`DateTime`;

--
-- Описание для представления lastoperations
--
DROP VIEW IF EXISTS lastoperations CASCADE;
CREATE OR REPLACE 
	DEFINER = 'root'@'localhost'
VIEW lastoperations
AS
	select `o`.`DateTime` AS `DateTime`,`c`.`CurrencySymbol` AS `CurrencySymbol`,`o`.`Amount` AS `Amount` from (`exchangeaccountoperations` `o` join `coins` `c` on((`c`.`Id` = `o`.`CoinId`))) order by `o`.`DateTime` desc limit 20;

--
-- Описание для представления profitabilitytable
--
DROP VIEW IF EXISTS profitabilitytable CASCADE;
CREATE OR REPLACE 
	DEFINER = 'root'@'%'
VIEW profitabilitytable
AS
	select `c`.`Name` AS `Name`,`p`.`DateTime` AS `Updated`,`p`.`CoinsPerDay` AS `CoinsPerDay`,`p`.`BtcPerDay` AS `BtcPerDay`,`p`.`UsdPerDay` AS `UsdPerDay` from ((`autominer_test`.`coinprofitabilities` `p` join (select `autominer_test`.`coinprofitabilities`.`CoinId` AS `CoinId`,max(`autominer_test`.`coinprofitabilities`.`DateTime`) AS `DateTime` from `autominer_test`.`coinprofitabilities` group by `autominer_test`.`coinprofitabilities`.`CoinId`) `dt` on(((`p`.`CoinId` = `dt`.`CoinId`) and (`p`.`DateTime` = `dt`.`DateTime`)))) join `autominer_test`.`coins` `c` on((`c`.`Id` = `p`.`CoinId`))) order by `p`.`BtcPerDay` desc;

-- 
-- Восстановить предыдущий режим SQL (SQL mode)
-- 
/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;

-- 
-- Включение внешних ключей
-- 
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;