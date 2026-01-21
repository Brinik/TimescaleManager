using Domain.Entities;
using Domain.Specifications;
using TimescaleManager.DTO;

namespace TimescaleManager.ServiceAbstractions
{
    public interface IResultService
    {
        TimescaleResult CalculateResults(List<TimescaleValue> values);
        Task<List<TimescaleResultDTO>> GetResultsRangeAsync(ResultsSpecification resultParams);
        Task<List<TimescaleResultDTO>> GetAllResultsAsync();
    }
}
