namespace QLCafeHV.Models.ViewModels
{
    public class PageViewModel<T>
    {
        public IEnumerable<T> Items { get; set; } = new List<T>();
        public int PageNumber { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
    }
}
