using Domain.Entities;

namespace Domain.RepositoryAbstractions
{
    public struct ResultsParams
    {
        public string Name { get; set; }
        public DateTime MinDate { get; set; }
        public DateTime MaxDate { get; set; }
        public float MinAvgValue { get; set; }
        public float MaxAvgValue { get; set; }
        public float MinAvgExecutionTime { get; set; }
        public float MaxAvgExecutionTime { get; set; }
    }
    public interface IResultRepository : IRepository<TimescaleResult, long>
    {
        Task<List<TimescaleResult>> GetFilteredAsync(ResultsParams resultsParams);
    }
}
