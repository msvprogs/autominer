using System;

namespace Msv.AutoMiner.Service.Data
{
    public class PoolInfo
    {
        public PoolAccountInfo AccountInfo { get; }
        public PoolState State { get; }
        public PoolPaymentData[] PaymentsData { get; }

        public PoolInfo(PoolAccountInfo accountInfo, PoolState state, PoolPaymentData[] paymentsData)
        {
            if (accountInfo == null)
                throw new ArgumentNullException(nameof(accountInfo));
            if (state == null)
                throw new ArgumentNullException(nameof(state));
            if (paymentsData == null)
                throw new ArgumentNullException(nameof(paymentsData));

            AccountInfo = accountInfo;
            State = state;
            PaymentsData = paymentsData;
        }
    }
}
