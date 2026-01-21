using Domain.Entities;
using Domain.Specifications;

namespace Domain.RepositoryAbstractions
{
    public interface IResultRepository : IRepository<TimescaleResult, long>
    {
        Task<List<TimescaleResult>> GetFilteredAsync(ResultsSpecification resultsParams);
    }
}
