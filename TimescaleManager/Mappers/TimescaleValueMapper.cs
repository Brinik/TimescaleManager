using TimescaleManager.DTO;
using Domain.Entities;

namespace TimescaleManager.Mappers
{
    public interface ITimescaleValueMapper 
    {
        TimescaleValueDTO MapToDTO(TimescaleValue model);
    }
    public class TimescaleValueMapper : ITimescaleValueMapper
    {
        public TimescaleValueDTO MapToDTO(TimescaleValue model) 
        {
            TimescaleValueDTO dto = new();
            dto.Date = model.Date;
            dto.ExecutionTime = model.ExecutionTime;
            dto.Value = model.Value;
            return dto;
        }
    }
}
