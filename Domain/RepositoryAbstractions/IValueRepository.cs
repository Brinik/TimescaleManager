using Domain.Entities;

namespace Domain.RepositoryAbstractions
{
    public interface IValueRepository : IRepository<TimescaleValue, long>
    {
        Task<List<TimescaleValue>> GetRangeAsync(string fileName, int amount = 1, bool bDescending = true);
    }
}
