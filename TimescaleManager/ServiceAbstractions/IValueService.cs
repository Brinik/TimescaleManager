
using TimescaleManager.DTO;

namespace TimescaleManager.ServiceAbstractions
{
    public interface IValueService
    {
        Task<List<TimescaleValueDTO>> GetValuesAsync();
        Task<List<TimescaleValueDTO>> GetLastTenAsync(string fileName);
    }
}
