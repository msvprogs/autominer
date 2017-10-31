namespace Msv.AutoMiner.FrontEnd.Models.Shared
{
    public class PaginationModel<T> : PaginationModel
    {
        public string Title { get; set; }

        public T[] CurrentPageItems { get; set; }
    }

    public class PaginationModel
    {
        public int CurrentPage { get; set; }

        public int TotalPages { get; set; }

        public int StartEndPages { get; } = 3;

        public int PagesAroundActive { get; } = 1;
    }
}
