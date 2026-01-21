using TimescaleManager.DTO;
using Domain.Entities;
using TimescaleManager.Mappers;
using Domain.RepositoryAbstractions;
using TimescaleManager.ServiceAbstractions;
using Domain.Specifications;

namespace TimescaleManager.Services
{
    public class ResultService : IResultService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ResultService> _logger;
        private readonly ITimescaleResultMapper _mapper;
        public ResultService(IUnitOfWork unitOfWork, ILogger<ResultService> logger, ITimescaleResultMapper mapper) 
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = mapper;
        }

        public TimescaleResult CalculateResults(List<TimescaleValue> values)
        {
            if (values == null || values.Count == 0)
            {
                _logger.LogError("Список значений пустой или null");
                throw new ArgumentNullException("Список значений пустой");
            }

            // Инициализируем переменные для первого элемента
            var firstValue = values[0];

            DateTime minDate = firstValue.Date;
            DateTime maxDate = firstValue.Date;
            double sumExecutionTime = firstValue.ExecutionTime;
            double sumValue = firstValue.Value;
            double maxValue = firstValue.Value;
            double minValue = firstValue.Value;

            // Для медианы собираем все значения Value
            var valueList = new List<double>(values.Count);
            valueList.Add(firstValue.Value);

            // Проходим по остальным элементам (начиная со второго)
            for (int i = 1; i < values.Count; i++)
            {
                var current = values[i];

                // Обновляем min/max даты
                if (current.Date < minDate)
                    minDate = current.Date;
                if (current.Date > maxDate)
                    maxDate = current.Date;

                // Суммируем для средних значений
                sumExecutionTime += current.ExecutionTime;
                sumValue += current.Value;

                // Обновляем min/max Value
                if (current.Value > maxValue)
                    maxValue = current.Value;
                if (current.Value < minValue)
                    minValue = current.Value;

                // Добавляем значение для медианы
                valueList.Add(current.Value);
            }

            double medianValue;
            valueList.Sort();
            int count = valueList.Count;

            if (count % 2 == 0)
            {
                // Четное количество элементов
                medianValue = (valueList[count / 2 - 1] + valueList[count / 2]) / 2;
            }
            else
            {
                // Нечетное количество элементов
                medianValue = valueList[count / 2];
            }
            return new TimescaleResult
            {
                DateDelta = (double)(maxDate - minDate).TotalSeconds,
                MinDate = minDate,
                AvgExecutionTime = (double)(sumExecutionTime / values.Count),
                AvgValue = (double)(sumValue / values.Count),
                MedianValue = medianValue,
                MaxValue = maxValue,
                MinValue = minValue
            };
        }

        public async Task<List<TimescaleResultDTO>> GetResultsRangeAsync(ResultsSpecification resultParams)
        {
            if (resultParams == null) 
            {
                throw new ArgumentNullException(nameof(resultParams));
            }
            ICollection<TimescaleResult> entities = await _unitOfWork.UploadedResults.GetFilteredAsync(resultParams);
            if (entities.Count == 0) 
            {
                throw new BadHttpRequestException("Результаты не найдены");
            }
            List<TimescaleResultDTO> resultDTOs = new();
            foreach (var entity in entities)
            {
                resultDTOs.Add(_mapper.MapToDTO(entity));
            }
            return resultDTOs;
        }

        public async Task<List<TimescaleResultDTO>> GetAllResultsAsync()
        {
            ICollection<TimescaleResult> entities = await _unitOfWork.UploadedResults.GetAsync(CancellationToken.None);
            List<TimescaleResultDTO> resultDTOs = new();
            foreach (var entity in entities)
            {
                resultDTOs.Add(_mapper.MapToDTO(entity));
            }
            return resultDTOs;
        }
    }
}
