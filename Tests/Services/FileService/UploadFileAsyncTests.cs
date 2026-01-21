using Domain.Entities;
using Domain.RepositoryAbstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text;
using TimescaleManager.ServiceAbstractions;
using TimescaleManager.Services;

namespace Tests.Services.FileServiceTests
{
    public class UploadFileAsyncTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ILogger<FileService>> _loggerMock;
        private readonly Mock<IResultService> _resultServiceMock;
        private readonly FileService _fileService;

        public UploadFileAsyncTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _loggerMock = new Mock<ILogger<FileService>>();
            _resultServiceMock = new Mock<IResultService>();
            _fileService = new FileService(
                _unitOfWorkMock.Object,
                _loggerMock.Object,
                _resultServiceMock.Object
            );
        }

        [Fact]
        public async Task UploadFileAsync_ValidFile_SuccessfullyUploads()
        {
            // Arrange
            var fileContent = "Date,ExecutionTime,Value\n" +
                            "2024-01-01T10:00:00Z,100,50.5\n" +
                            "2024-01-01T11:00:00Z,200,60.5\n";
            var file = CreateFormFile(fileContent, "test.csv");
            var fileId = Guid.NewGuid();

            var uploadedFile = new TimescaleFile { Id = fileId, Name = "test.csv" };
            var timescaleValues = new List<TimescaleValue>();
            var result = new TimescaleResult { FileId = fileId };

            // Mock репозиториев
            var uploadedFilesRepoMock = new Mock<IFileRepository>();
            var uploadedValuesRepoMock = new Mock<IValueRepository>();
            var uploadedResultsRepoMock = new Mock<IResultRepository>();

            _unitOfWorkMock
                .Setup(u => u.UploadedFiles)
                .Returns(uploadedFilesRepoMock.Object);
            _unitOfWorkMock
                .Setup(u => u.UploadedValues)
                .Returns(uploadedValuesRepoMock.Object);
            _unitOfWorkMock
                .Setup(u => u.UploadedResults)
                .Returns(uploadedResultsRepoMock.Object);

            uploadedFilesRepoMock
                .Setup(r => r.AddAsync(It.IsAny<TimescaleFile>()))
                .Callback((TimescaleFile f) => f.Id = fileId)
                .ReturnsAsync(uploadedFile);

            uploadedValuesRepoMock
                .Setup(r => r.AddRangeAsync(It.IsAny<List<TimescaleValue>>()))
                .Returns(Task.CompletedTask);

            uploadedResultsRepoMock
                .Setup(r => r.AddAsync(It.IsAny<TimescaleResult>()))
                .ReturnsAsync(result);

            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(0);

            _unitOfWorkMock.Setup(u => u.BeginTransactionAsync())
                .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(u => u.CommitTransactionAsync())
                .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(u => u.DetachAllEntities())
                .Verifiable();

            _resultServiceMock
                .Setup(r => r.CalculateResults(It.IsAny<List<TimescaleValue>>()))
                .Returns(result);

            // Act
            await _fileService.UploadFileAsync(file);

            // Assert
            uploadedFilesRepoMock.Verify(r =>
                r.AddAsync(It.Is<TimescaleFile>(f => f.Name == "test.csv")), Times.Once);

            uploadedValuesRepoMock.Verify(r =>
                r.AddRangeAsync(It.IsAny<List<TimescaleValue>>()), Times.AtLeastOnce);

            uploadedResultsRepoMock.Verify(r =>
                r.AddAsync(It.Is<TimescaleResult>(r => r.FileId == fileId)), Times.Once);

            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.AtLeastOnce);
            _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(), Times.Once);
            _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(), Times.Never);
        }

        [Fact]
        public async Task UploadFileAsync_EmptyFile_ThrowsBadHttpRequestException()
        {
            // Arrange
            var fileContent = "Date,ExecutionTime,Value\n";
            var file = CreateFormFile(fileContent, "empty.csv");
            var timescaleFile = new TimescaleFile()
            {
                Id = Guid.NewGuid(),
                Name = file.FileName
            };

            var uploadedFilesRepoMock = new Mock<IFileRepository>();
            _unitOfWorkMock
                .Setup(u => u.UploadedFiles)
                .Returns(uploadedFilesRepoMock.Object);

            uploadedFilesRepoMock
                .Setup(r => r.AddAsync(It.IsAny<TimescaleFile>()))
                .ReturnsAsync(timescaleFile);

            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(0);

            _unitOfWorkMock.Setup(u => u.BeginTransactionAsync())
                .Returns(Task.CompletedTask);

            // Act & Assert
            await Assert.ThrowsAsync<BadHttpRequestException>(() =>
                _fileService.UploadFileAsync(file));

            _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(), Times.Once);
        }

        [Theory]
        [InlineData("invalid-date,100,50.5", "Не удалось распарсить дату")]
        [InlineData("2024-01-01T10:00:00Z,-100,50.5", "Время исполнения меньше нуля")]
        [InlineData("2024-01-01T10:00:00Z,100,-50.5", "Значение показателя меньше нуля")]
        public async Task UploadFileAsync_InvalidData_ThrowsBadHttpRequestException(
            string csvLine, string expectedError)
        {
            // Arrange
            var fileContent = $"Date,ExecutionTime,Value\n{csvLine}";
            var file = CreateFormFile(fileContent, "invalid.csv");
            var timescaleFile = new TimescaleFile()
            {
                Id = Guid.NewGuid(),
                Name = file.FileName
            };

            var uploadedFilesRepoMock = new Mock<IFileRepository>();
            _unitOfWorkMock
                .Setup(u => u.UploadedFiles)
                .Returns(uploadedFilesRepoMock.Object);

            uploadedFilesRepoMock
                .Setup(r => r.AddAsync(It.IsAny<TimescaleFile>()))
                .ReturnsAsync(timescaleFile);

            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            _unitOfWorkMock.Setup(u => u.BeginTransactionAsync())
                .Returns(Task.CompletedTask);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BadHttpRequestException>(() =>
                _fileService.UploadFileAsync(file));

            Assert.Contains(expectedError, exception.Message);
            _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(), Times.Once);
        }

        [Fact]
        public async Task UploadFileAsync_FileTooLarge_ThrowsBadHttpRequestException()
        {
            // Arrange
            var csvBuilder = new StringBuilder();
            csvBuilder.AppendLine("Date,ExecutionTime,Value");

            // Создаем более 10000 строк
            for (int i = 0; i < 10001; i++)
            {
                csvBuilder.AppendLine($"2024-01-01T10:00:00Z,{i},50.5");
            }

            var file = CreateFormFile(csvBuilder.ToString(), "large.csv");
            var timescaleFile = new TimescaleFile()
            {
                Id = Guid.NewGuid(),
                Name = file.FileName
            };

            var uploadedFilesRepoMock = new Mock<IFileRepository>();
            _unitOfWorkMock
                .Setup(u => u.UploadedFiles)
                .Returns(uploadedFilesRepoMock.Object);

            uploadedFilesRepoMock
                .Setup(r => r.AddAsync(It.IsAny<TimescaleFile>()))
                .ReturnsAsync(timescaleFile);

            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            _unitOfWorkMock.Setup(u => u.BeginTransactionAsync())
                .Returns(Task.CompletedTask);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BadHttpRequestException>(() =>
                _fileService.UploadFileAsync(file));

            Assert.Contains("Длина файла не может превышать 10 000 строк", exception.Message);
            _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(), Times.Once);
        }

        [Fact]
        public async Task UploadFileAsync_InvalidDateFormat_ThrowsBadHttpRequestException()
        {
            // Arrange
            var fileContent = "Date,ExecutionTime,Value\n" +
                            "01-01-2024 10:00:00:00,100,50.5\n";
            var file = CreateFormFile(fileContent, "invalid.csv");
            var timescaleFile = new TimescaleFile()
            {
                Id = Guid.NewGuid(),
                Name = file.FileName
            };

            var uploadedFilesRepoMock = new Mock<IFileRepository>();
            _unitOfWorkMock
                .Setup(u => u.UploadedFiles)
                .Returns(uploadedFilesRepoMock.Object);

            var uploadedResultsMock = new Mock<IResultRepository>();
            _unitOfWorkMock
                .Setup(u => u.UploadedResults)
                .Returns(uploadedResultsMock.Object);

            var uploadedValuesMock = new Mock<IValueRepository>();
            _unitOfWorkMock
                .Setup(u => u.UploadedValues)
                .Returns(uploadedValuesMock.Object);

            uploadedFilesRepoMock
                .Setup(r => r.AddAsync(It.IsAny<TimescaleFile>()))
                .ReturnsAsync(timescaleFile);

            _resultServiceMock
                .Setup(r => r.CalculateResults(It.IsAny<List<TimescaleValue>>()))
                .Returns(new TimescaleResult() { FileId = timescaleFile.Id, File = timescaleFile });

            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            _unitOfWorkMock.Setup(u => u.BeginTransactionAsync())
                .Returns(Task.CompletedTask);

            // Act & Assert
            await Assert.ThrowsAsync<BadHttpRequestException>(() =>
                _fileService.UploadFileAsync(file));

            _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(), Times.Once);
        }

        [Fact]
        public async Task UploadFileAsync_SaveChangesFails_RollsBackTransaction()
        {
            // Arrange
            var fileContent = "Date,ExecutionTime,Value\n" +
                            "2024-01-01T10:00:00Z,100,50.5\n";
            var file = CreateFormFile(fileContent, "test.csv");
            var timescaleFile = new TimescaleFile()
            {
                Id = Guid.NewGuid(),
                Name = file.FileName
            };

            var uploadedFilesRepoMock = new Mock<IFileRepository>();
            _unitOfWorkMock
                .Setup(u => u.UploadedFiles)
                .Returns(uploadedFilesRepoMock.Object);

            uploadedFilesRepoMock
                .Setup(r => r.AddAsync(It.IsAny<TimescaleFile>()))
                .ReturnsAsync(timescaleFile);

            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .ThrowsAsync(new Exception("Database error"));

            _unitOfWorkMock.Setup(u => u.BeginTransactionAsync())
                .Returns(Task.CompletedTask);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _fileService.UploadFileAsync(file));

            _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(), Times.Once);
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Ошибка при загрузке CSV файла")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task UploadFileAsync_MissingFields_ThrowsBadHttpRequestException()
        {
            // Arrange
            var fileContent = "Date,ExecutionTime\n" + // Missing Value column
                            "2024-01-01T10:00:00Z,100\n";
            var file = CreateFormFile(fileContent, "missing_fields.csv");
            var timescaleFile = new TimescaleFile()
            {
                Id = Guid.NewGuid(),
                Name = file.FileName
            };

            var uploadedFilesRepoMock = new Mock<IFileRepository>();
            _unitOfWorkMock
                .Setup(u => u.UploadedFiles)
                .Returns(uploadedFilesRepoMock.Object);

            uploadedFilesRepoMock
                .Setup(r => r.AddAsync(It.IsAny<TimescaleFile>()))
                .ReturnsAsync(timescaleFile);

            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            _unitOfWorkMock.Setup(u => u.BeginTransactionAsync())
                .Returns(Task.CompletedTask);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BadHttpRequestException>(() =>
                _fileService.UploadFileAsync(file));

            Assert.Contains("Файл не может содержать строки с отсутствующими значениями", exception.Message);
            _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(), Times.Once);
        }

        [Fact]
        public async Task UploadFileAsync_InvalidValueType_ThrowsBadHttpRequestException()
        {
            // Arrange
            var fileContent = "Date,ExecutionTime,Value\n" +
                            "2024-01-01T10:00:00Z,not-a-number,50.5\n";
            var file = CreateFormFile(fileContent, "invalid_type.csv");
            var timescaleFile = new TimescaleFile()
            {
                Id = Guid.NewGuid(),
                Name = file.FileName
            };

            var uploadedFilesRepoMock = new Mock<IFileRepository>();
            _unitOfWorkMock
                .Setup(u => u.UploadedFiles)
                .Returns(uploadedFilesRepoMock.Object);

            var uploadedResultsMock = new Mock<IResultRepository>();
            _unitOfWorkMock
                .Setup(u => u.UploadedResults)
                .Returns(uploadedResultsMock.Object);

            var uploadedValuesMock = new Mock<IValueRepository>();
            _unitOfWorkMock
                .Setup(u => u.UploadedValues)
                .Returns(uploadedValuesMock.Object);

            uploadedFilesRepoMock
                .Setup(r => r.AddAsync(It.IsAny<TimescaleFile>()))
                .ReturnsAsync(timescaleFile);

            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            _unitOfWorkMock.Setup(u => u.BeginTransactionAsync())
                .Returns(Task.CompletedTask);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BadHttpRequestException>(() =>
                _fileService.UploadFileAsync(file));

            Assert.Contains("Не удалось привести тип значения", exception.Message);
            _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(), Times.Once);
        }
        private IFormFile CreateFormFile(string content, string fileName)
        {
            var bytes = Encoding.UTF8.GetBytes(content);
            var stream = new MemoryStream(bytes);

            return new FormFile(stream, 0, bytes.Length, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "text/csv"
            };
        }
    }
}
