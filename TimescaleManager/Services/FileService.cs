using CsvHelper;
using System.Diagnostics;
using System.Globalization;
using TimescaleManager.Customs;
using TimescaleManager.DTO;
using Domain.Entities;
using TimescaleManager.Models;
using Domain.RepositoryAbstractions;
using TimescaleManager.ServiceAbstractions;

namespace TimescaleManager.Services
{
    public class FileService : IFileService
    {
        private static readonly string[] _allowedExtensions = { ".csv" };

        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<FileService> _logger;
        private readonly IResultService _resultService;
        public FileService(IUnitOfWork unitOfWork, ILogger<FileService> logger, IResultService resultService) 
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _resultService = resultService;
        }
        public async Task<ICollection<TimescaleFileDTO>> GetFilesAsync() 
        {
            ICollection<TimescaleFile> entities = await _unitOfWork.UploadedFiles.GetAsync(CancellationToken.None);
            List<TimescaleFileDTO> fileDTOs = new();
            foreach (var entity in entities) 
            {
                TimescaleFileDTO fileDTO = new() 
                {
                    Name = entity.Name
                };
                fileDTOs.Add(fileDTO);
            }
            return fileDTOs;
        }
        public async Task UploadFileAsync(IFormFile file)
        {
            try
            {
                _logger.LogInformation("Начинаем загрузку файла");
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                //Добавление файла и его данных
                await _unitOfWork.BeginTransactionAsync();
                var validationResult = ValidateCsvFile(file);
                if (!validationResult.isValid)
                {
                    _logger.LogWarning("Невалидный файл");
                    throw new BadHttpRequestException(validationResult.errorMessage);
                }

                var uploadedFile = new TimescaleFile
                {
                    Name = file.FileName
                };
                uploadedFile = await _unitOfWork.UploadedFiles.AddAsync(uploadedFile);
                await _unitOfWork.SaveChangesAsync();


                var timescaleValues = new List<TimescaleValue>();
                await ParseFileAsync(file, uploadedFile.Id, timescaleValues);
                if (!timescaleValues.Any())
                {
                    _logger.LogWarning("Файл пустой");
                    throw new BadHttpRequestException("Файл не может быть пустым");
                }

                //Подсчитывание результатов
                TimescaleResult result = _resultService.CalculateResults(timescaleValues);
                result.FileId = uploadedFile.Id;
                await _unitOfWork.UploadedResults.AddAsync(result);
                await _unitOfWork.SaveChangesAsync();

                //Сохранение в БД пакетами
                const int batchSize = 5000;
                var batches = (int)Math.Ceiling((double)timescaleValues.Count / batchSize);

                for (int i = 0; i < batches; i++)
                {
                    var batch = timescaleValues
                        .Skip(i * batchSize)
                        .Take(batchSize)
                        .ToList();

                    await _unitOfWork.UploadedValues.AddRangeAsync(batch);
                    await _unitOfWork.SaveChangesAsync();
                    _unitOfWork.DetachAllEntities();
                }

                stopwatch.Stop();
                TimeSpan elapsed = stopwatch.Elapsed;
                _logger.LogInformation("Файл загружен за {elapsed} секунд", elapsed);

                await _unitOfWork.CommitTransactionAsync();
                _logger.LogInformation("Файл {Name} успешно обработан. Строк: {Count}",
                    uploadedFile.Name, timescaleValues.Count);
            }
            catch (BadHttpRequestException) 
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Ошибка при загрузке CSV файла {FileName}", file.FileName);
                throw;
            }
        }

        /// <summary>
        /// Валидация формата файла (Валидация данных проводится в методе ParseFileAsync)
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public (bool isValid, string errorMessage) ValidateCsvFile(IFormFile file)
        {
            if (file == null)
                return (false, "Файл не выбран");

            if (file.Length == 0)
                return (false, "Файл пустой");

            var extension = Path.GetExtension(file.FileName);
            if (string.IsNullOrEmpty(extension))
                return (false, "Файл не имеет расширения");

            if (!_allowedExtensions.Contains(extension.ToLowerInvariant()))
                return (false, "Разрешены только файлы .csv");

            if (file.FileName.Contains("..") || file.FileName.Contains("/") || file.FileName.Contains("\\"))
                return (false, "Некорректное имя файла");

            return (true, string.Empty);
        }

        /// <summary>
        /// Попытаться парсить список значений для сохранения в БД
        /// </summary>
        /// <param name="file">файл в памяти</param>
        /// <param name="fileId">Id файла в БД</param>
        /// <exception cref="BadHttpRequestException"></exception>
        private async Task ParseFileAsync(IFormFile file, Guid fileId, List<TimescaleValue> outValues)
        {
            var timescaleValues = new List<TimescaleValue>();
            using (var reader = new StreamReader(file.OpenReadStream()))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Context.TypeConverterCache.AddConverter<double>(new CustomDoubleConverter());
                try
                {
                    var records = csv.GetRecordsAsync<TimescaleValueCsvRecord>();
                    await foreach (var record in records)
                    {

                        var value = new TimescaleValue()
                        {
                            ExecutionTime = record.ExecutionTime,
                            Value = record.Value,
                            FileId = fileId
                        };
                        //Пытаемся привести формат даты
                        if (DateTime.TryParse(record.Date,
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                            out DateTime parsedDate))
                        {
                            value.Date = parsedDate.ToUniversalTime();
                            outValues.Add(value);
                        }
                        else if (DateTime.TryParseExact(record.Date, "yyyy-MM-dd'T'HH-mm-ss.ffff'Z'",
                                 CultureInfo.InvariantCulture,
                                 DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                                 out DateTime parsedDateExact))
                        {
                            value.Date = parsedDateExact.ToUniversalTime();
                            outValues.Add(value);
                        }
                        else
                        {
                            int row = csv.CurrentIndex + 1;
                            _logger.LogWarning("Не удалось распарсить дату: {Date}", record.Date);
                            throw new BadHttpRequestException($"Не удалось распарсить дату в строке {row}");
                        }

                        //Валидация значений
                        if (outValues.Count > 10000)
                        {
                            _logger.LogWarning("Длина файла не может превышать 10 000 строк");
                            throw new BadHttpRequestException("Длина файла не может превышать 10 000 строк");
                        }
                        if (value.Date <= DateTime.Parse("01.01.2000") || value.Date >= DateTime.Now)
                        {
                            int row = csv.CurrentIndex + 1;
                            _logger.LogWarning("Невалидная дата: {Date} в строчке {index}", value.Date, row);
                            throw new BadHttpRequestException($"Дата невалидна в строке {row}");
                        }
                        if (value.ExecutionTime < 0)
                        {
                            int row = csv.CurrentIndex + 1;
                            _logger.LogWarning("Время исполнения меньше нуля: {ExecutionTime}", value.ExecutionTime);
                            throw new BadHttpRequestException($"Время исполнения меньше нуля в строке {row}");
                        }
                        if (value.Value < 0)
                        {
                            int row = csv.CurrentIndex + 1;
                            _logger.LogWarning("Значение показателя меньше нуля: {Value}", value.Value);
                            throw new BadHttpRequestException($"Значение показателя меньше нуля в строке {row}");
                        }
                    }
                }
                catch (CsvHelper.MissingFieldException ex)
                {
                    int row = csv.CurrentIndex + 1;
                    _logger.LogWarning("Файл не может содержать строки с отсутствующими значениями");
                    throw new BadHttpRequestException($"Файл не может содержать строки с отсутствующими значениями. Строка {row}", ex);
                }
                catch (CsvHelper.TypeConversion.TypeConverterException ex)
                {
                    int row = csv.CurrentIndex + 1;
                    _logger.LogWarning("Не удалось привести тип значения в строке {index}", row);
                    throw new BadHttpRequestException($"Не удалось привести тип значения. Строка {row}", ex);
                }
            }
        }
    }
}
