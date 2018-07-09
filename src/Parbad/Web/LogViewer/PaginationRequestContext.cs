namespace Parbad.Web.LogViewer
{
    internal class PaginationRequestContext
    {
        public PaginationRequestContext(int pageNumber, int pageSize)
        {
            if (pageNumber <= 0)
            {
                pageNumber = 1;
            }

            if (pageSize <= 0)
            {
                pageSize = 10;
            }
            else if (pageSize > 100)
            {
                pageSize = 100;
            }

            PageNumber = pageNumber;
            PageSize = pageSize;
        }

        public int PageNumber { get; }

        public int PageSize { get; }
    }
}