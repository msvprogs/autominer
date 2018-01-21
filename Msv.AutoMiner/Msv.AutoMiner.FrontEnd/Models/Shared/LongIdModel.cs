namespace Msv.AutoMiner.FrontEnd.Models.Shared
{
    public class LongIdModel
    {
        public string Id { get; set; }
        public string Url { get; set; }
        public string Title { get; set; }

        public LongIdModel()
        { }

        public LongIdModel(string id)
            => Id = id;
    }
}