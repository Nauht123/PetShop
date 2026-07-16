namespace PetShop.Helpers
{
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
        public int PageSize { get; set; }
    }

    public static class PaginationHelper
    {
        public static PagedResult<T> Paginate<T>(
            IQueryable<T> query, int page, int pageSize)
        {
            if (page < 1) page = 1;

            int totalItems = query.Count();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var items = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PagedResult<T>
            {
                Items = items,
                CurrentPage = page,
                TotalPages = Math.Max(totalPages, 1),
                TotalItems = totalItems,
                PageSize = pageSize
            };
        }
    }
}