namespace Msv.AutoMiner.FrontEnd.Models.Shared
{
    public class AlertModel : DialogModel
    {
        public AlertType Type { get; set; }
        public string Body { get; set; }
    }
}
