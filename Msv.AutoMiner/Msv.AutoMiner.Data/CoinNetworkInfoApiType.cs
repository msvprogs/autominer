﻿using Msv.AutoMiner.Common.Infrastructure;

namespace Msv.AutoMiner.Data
{
    public enum CoinNetworkInfoApiType : byte
    {
        [EnumCaption("JSON-RPC (local wallet)")]
        JsonRpc = 0,

        [EnumCaption("BChain.info")]
        BchainInfo = 1,

        [EnumCaption("ChainRadar.com")]
        ChainRadar = 2,

        [EnumCaption("Chainz.cryptoid.info")]
        ChainzCryptoid = 3,

        [EnumCaption("Insight block explorer")]
        Insight = 4,

        [EnumCaption("Iquidus block explorer")]
        Iquidus = 5,

        [EnumCaption("Iquidus block explorer (with POS difficulty)")]
        IquidusWithPos = 6,

        [EnumCaption("MinerGate.com")]
        MinerGate = 7,

        [EnumCaption("Open Ethereum based pool")]
        OpenEthereumPool = 8,

        [EnumCaption("ProHashing.com")]
        ProHashing = 9,

        [EnumCaption("TheBlockFactory pool")]
        TheBlockFactory = 10,

        [EnumCaption("TheCryptoChat.net")]
        TheCryptoChat = 11,

        [EnumCaption("Special (hardcoded)")]
        Special = 13,

        [EnumCaption("AltMix.org")]
        Altmix = 14,

        [EnumCaption("Special (hardcoded) with multi-algo")]
        SpecialMulti = 15
    }
}
