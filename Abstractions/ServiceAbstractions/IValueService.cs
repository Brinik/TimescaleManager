
using Domain.DTO;
using Domain.Entities;

namespace Domain.ServiceAbstractions
{
    public interface IValueService
    {
        Task<ICollection<TimescaleValueDTO>> GetValuesAsync();
        Task<ICollection<TimescaleValueDTO>> GetLastTenAsync(string fileName);
    }
}
