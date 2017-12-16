namespace Msv.AutoMiner.FrontEnd.Models.Shared
{
    public class LogoCoinNameModel
    {
        public string Name { get; }
        public byte[] Logo { get; }

        public LogoCoinNameModel(string name, byte[] logo)
        {
            Name = name;
            Logo = logo;
        }
    }
}
