using TimescaleManager.DTO;
using Domain.Entities;
using TimescaleManager.Mappers;
using Domain.RepositoryAbstractions;
using TimescaleManager.ServiceAbstractions;

namespace TimescaleManager.Services
{
    public class ValueService : IValueService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ValueService> _logger;
        private readonly ITimescaleValueMapper _mapper;
        public ValueService(IUnitOfWork unitOfWork, ILogger<ValueService> logger, ITimescaleValueMapper timescaleValueMapper)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = timescaleValueMapper;
        }
        public async Task<List<TimescaleValueDTO>> GetValuesAsync()
        {
            ICollection<TimescaleValue> entities = await _unitOfWork.UploadedValues.GetAsync(CancellationToken.None);
            List<TimescaleValueDTO> valueDTOs = new();
            foreach (var entity in entities)
            {
                valueDTOs.Add(_mapper.MapToDTO(entity));
            }
            return valueDTOs;
        }

        public async Task<List<TimescaleValueDTO>> GetLastTenAsync(string fileName) 
        {
            if (string.IsNullOrEmpty(fileName)) 
            {
                throw new ArgumentNullException(nameof(fileName));
            }
            ICollection<TimescaleValue> entities = await _unitOfWork.UploadedValues.GetRangeAsync(fileName, 10);
            if (!entities.Any()) 
            {
                string message = "Файл '" + fileName + "' не найден.";
                throw new BadHttpRequestException(message);
            }
            List<TimescaleValueDTO> valueDTOs = new();
            foreach (var entity in entities)
            {
                valueDTOs.Add(_mapper.MapToDTO(entity));
            }
            return valueDTOs;
        }
    }
}
