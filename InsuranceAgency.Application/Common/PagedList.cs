using System;
using System.Collections.Generic;

namespace InsuranceAgency.Application.Common
{
    public class PagedList<T>
    {
        public IReadOnlyCollection<T> Items { get; }
        public int Page { get; }
        public int PageSize { get; }
        public int TotalCount { get; }

        public PagedList(IEnumerable<T> items, int page, int pageSize, int totalCount)
        {
            Items = new List<T>(items);
            Page = page;
            PageSize = pageSize;
            TotalCount = totalCount;
        }
    }
}
