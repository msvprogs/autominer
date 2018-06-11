using System.Diagnostics;

namespace Msv.AutoMiner.NetworkInfo.Data
{
    [DebuggerDisplay("Ins: {InValues.Length}, Outs: {OutValues.Length}, Fee: {Fee}")]
    public class TransactionInfo
    {
        public double[] InValues { get; set; }
        public double[] OutValues { get; set; }
        public double? Fee { get; set; }
        public bool IsCoinbase { get; set; }
    }
}
