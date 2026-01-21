using Domain.Entities;
using Domain.RepositoryAbstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using TimescaleManager.DTO;
using TimescaleManager.Mappers;
using TimescaleManager.Services;

namespace Tests.Services.ValueServiceTests
{
    public class GetLastTenAsyncTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ILogger<ValueService>> _loggerMock;
        private readonly Mock<ITimescaleValueMapper> _mapperMock;
        private readonly ValueService _valueService;

        public GetLastTenAsyncTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _loggerMock = new Mock<ILogger<ValueService>>();
            _mapperMock = new Mock<ITimescaleValueMapper>();
            _valueService = new ValueService(
                _unitOfWorkMock.Object,
                _loggerMock.Object,
                _mapperMock.Object);
        }
        [Fact]
        public async Task GetLastTenAsyncTests_NoResults_ThrowsBadHttpRequestException()
        {
            // Arrange
            var emptyResults = new List<TimescaleValue>();
            string fileName = "some file.csv";

            var uploadedValuesMock = new Mock<IValueRepository>();
            _unitOfWorkMock
                .Setup(u => u.UploadedValues)
                .Returns(uploadedValuesMock.Object);
            uploadedValuesMock
                .Setup(r => r.GetRangeAsync(It.IsAny<string>(), 10, true))
                .ReturnsAsync(emptyResults);


            // Act & Assert
            var exception = await Assert.ThrowsAsync<BadHttpRequestException>(
                () => _valueService.GetLastTenAsync(fileName));

            Assert.Equal("Файл 'some file.csv' не найден.", exception.Message);

            _unitOfWorkMock.Verify(
                r => r.UploadedValues.GetRangeAsync(fileName, 10, true),
                Times.Once);

            _mapperMock.Verify(
                m => m.MapToDTO(It.IsAny<TimescaleValue>()),
                Times.Never);
        }

        [Fact]
        public async Task GetLastTenAsyncTests_WithEmptyFileName_ThrowsArgumentNullException()
        {
            // Arrange
            string emptyFileName = string.Empty;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => _valueService.GetLastTenAsync(emptyFileName));
        }

        [Fact]
        public async Task GetLastTenAsyncTests_Valid_ReturnsValueDTOs()
        {
            // Arrange
            string fileName = "file name.csv";
            var testValues = new List<TimescaleValue>
            {
                new TimescaleValue
                {
                    Id = 0,
                    FileId = Guid.NewGuid(),
                    Date = DateTime.Now,
                    ExecutionTime = 50,
                    Value = 30
                },
                new TimescaleValue
                {
                    Id = 1,
                    FileId = Guid.NewGuid(),
                    Date = DateTime.Now,
                    ExecutionTime = 100,
                    Value = 60
                }
            };
            var expectedDTOs = new List<TimescaleValueDTO>
            {
                new TimescaleValueDTO
                {
                    Date = testValues[0].Date,
                    ExecutionTime = testValues[0].ExecutionTime,
                    Value = testValues[0].Value
                },
                new TimescaleValueDTO
                {
                    Date = testValues[1].Date,
                    ExecutionTime = testValues[1].ExecutionTime,
                    Value = testValues[1].Value
                }
            };

            _unitOfWorkMock
                .Setup(r => r.UploadedValues.GetRangeAsync(fileName, 10, true))
                .ReturnsAsync(testValues);

            _mapperMock
                .Setup(m => m.MapToDTO(It.Is<TimescaleValue>(r => r.Id == testValues[0].Id)))
                .Returns(expectedDTOs[0]);

            _mapperMock
                .Setup(m => m.MapToDTO(It.Is<TimescaleValue>(r => r.Id == testValues[1].Id)))
                .Returns(expectedDTOs[1]);

            // Act
            var result = await _valueService.GetLastTenAsync(fileName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(expectedDTOs[0].Value, result[0].Value);
            Assert.Equal(expectedDTOs[1].Value, result[1].Value);

            _unitOfWorkMock.Verify(
                r => r.UploadedValues.GetRangeAsync(fileName, 10, true), Times.Once);

            _mapperMock.Verify(
                m => m.MapToDTO(It.IsAny<TimescaleValue>()),
                Times.Exactly(2));
        }
    }
}
