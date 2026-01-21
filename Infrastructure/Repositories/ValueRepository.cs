using Domain.Entities;
using Domain.RepositoryAbstractions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class ValueRepository : Repository<TimescaleValue, long>, IValueRepository
    {
        public ValueRepository(DatabaseContext context) : base(context)
        {
        }

        public async Task<List<TimescaleValue>> GetRangeAsync(string fileName, int amount = 1, bool bDescending = true)
        {
            var query = Get(v => v.File!.Name == fileName);
            IQueryable<TimescaleValue> result;
            if (bDescending) 
            {
                result = query.OrderByDescending(v => v.Date);
            }
            else 
            {
                result = query.OrderBy(v => v.Date);
            }
            result = result.Take(amount)
            .OrderBy(v => v.Date);
            return await result.ToListAsync();
        }
    }
}
