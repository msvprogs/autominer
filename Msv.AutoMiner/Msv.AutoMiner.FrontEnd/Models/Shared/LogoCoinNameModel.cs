namespace Msv.AutoMiner.FrontEnd.Models.Shared
{
    public class LogoCoinNameModel
    {
        public string Name { get; }
        public string Symbol { get; }
        public byte[] Logo { get; }

        public LogoCoinNameModel(string name, string symbol, byte[] logo)
        {
            Name = name;
            Symbol = symbol;
            Logo = logo;
        }
    }
}
