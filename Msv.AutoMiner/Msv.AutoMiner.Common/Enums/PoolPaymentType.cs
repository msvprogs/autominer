using Msv.AutoMiner.Common.Infrastructure;

namespace Msv.AutoMiner.Common.Enums
{
    public enum PoolPaymentType : byte
    {
        [EnumCaption("N/A")]
        Unknown = 0,

        [EnumCaption("Mining reward")]
        Reward = 1,

        [EnumCaption("Pool fee")]
        PoolFee = 2,

        [EnumCaption("Transaction fee")]
        TransactionFee = 3,

        [EnumCaption("Transfer to wallet")]
        TransferToWallet = 4,

        [EnumCaption("Donation to pool")]
        Donation = 5
    }
}
