namespace Msv.AutoMiner.Common.Notifiers
{
    public interface ITelegramNotifierStorage
    {
        int[] GetReceiverIds(string[] userWhiteList);
    }
}
