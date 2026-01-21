using Domain.Entities;
using Domain.RepositoryAbstractions;
using Domain.Specifications;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class ResultRepository : Repository<TimescaleResult, long>, IResultRepository
    {
        public ResultRepository(DatabaseContext context) : base(context)
        {
        }
        public async Task<List<TimescaleResult>> GetFilteredAsync(ResultsSpecification resultsParams)
        {
            var query = Get();
            if (!string.IsNullOrWhiteSpace(resultsParams.Name)) 
            {
                query = query.Where(r => r.File!.Name == resultsParams.Name);
            }
            if (resultsParams.MinDate <= resultsParams.MaxDate)
            {
                query = query.Where(r => r.MinDate >= resultsParams.MinDate && r.MinDate <= resultsParams.MaxDate);
            }
            if (resultsParams.MinAvgValue < resultsParams.MaxAvgValue)
            {
                query = query.Where(r => r.AvgValue >= resultsParams.MinAvgValue
                && r.AvgValue <= resultsParams.MaxAvgValue);
            }
            if (resultsParams.MinAvgExecutionTime < resultsParams.MaxAvgExecutionTime) 
            {
                query = query.Where(r => r.AvgExecutionTime >= resultsParams.MinAvgExecutionTime 
                && r.AvgExecutionTime <= resultsParams.MaxAvgExecutionTime);
            }
            return await query.ToListAsync();
        }
    }
}
