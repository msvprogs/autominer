using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Msv.AutoMiner.FrontEnd.Infrastructure
{
    public class PaginationController : Controller
    {
        private const int PageItems = 25;

        protected IQueryable<T> GetCurrentPageItems<T>(IQueryable<T> query, int page)
            => query.Skip(PageItems * (page - 1))
                .Take(PageItems);

        protected async Task<int> CountTotalPages<T>(IQueryable<T> query)
            => (int) Math.Ceiling(await query.CountAsync() / (double) PageItems);
    }
}
