using TimescaleManager.DTO;
using Domain.Entities;

namespace TimescaleManager.Mappers
{
    public interface ITimescaleResultMapper 
    {
        TimescaleResultDTO MapToDTO(TimescaleResult model);
    }
    public class TimescaleResultMapper : ITimescaleResultMapper
    {
        public TimescaleResultDTO MapToDTO(TimescaleResult model)
        {
            TimescaleResultDTO dto = new();
            dto.DateDelta = model.DateDelta;
            dto.MinDate = model.MinDate;
            dto.AvgExecutionTime = model.AvgExecutionTime;
            dto.AvgValue = model.AvgValue;
            dto.MedianValue = model.MedianValue;
            dto.MaxValue = model.MaxValue;
            dto.MinValue = model.MinValue;
            return dto;
        }
    }
}
