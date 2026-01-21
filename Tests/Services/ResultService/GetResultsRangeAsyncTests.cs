using Domain.Entities;
using Domain.RepositoryAbstractions;
using Domain.Specifications;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text;
using TimescaleManager.DTO;
using TimescaleManager.Mappers;
using TimescaleManager.ServiceAbstractions;
using TimescaleManager.Services;

namespace Tests.Services.ResultServiceTests
{
    public class GetResultsRangeAsyncTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ILogger<ResultService>> _loggerMock;
        private readonly Mock<ITimescaleResultMapper> _mapperMock;
        private readonly ResultService _resultService;

        public GetResultsRangeAsyncTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _loggerMock = new Mock<ILogger<ResultService>>();
            _mapperMock = new Mock<ITimescaleResultMapper>();
            _resultService = new ResultService(
                _unitOfWorkMock.Object, 
                _loggerMock.Object,
                _mapperMock.Object);
        }

        [Fact]
        public async Task GetResultsRangeAsync_NoResults_ThrowsBadHttpRequestException()
        {
            // Arrange
            var resultParams = new ResultsSpecification
            {
            };

            var emptyResults = new List<TimescaleResult>();

            var uploadedResultsRepoMock = new Mock<IResultRepository>();
            _unitOfWorkMock
                .Setup(u => u.UploadedResults)
                .Returns(uploadedResultsRepoMock.Object);
            uploadedResultsRepoMock
                .Setup(r => r.GetFilteredAsync(It.IsAny<ResultsSpecification>()))
                .ReturnsAsync(emptyResults);
            

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BadHttpRequestException>(
                () => _resultService.GetResultsRangeAsync(resultParams));

            Assert.Equal("Результаты не найдены", exception.Message);

            _unitOfWorkMock.Verify(
                r => r.UploadedResults.GetFilteredAsync(resultParams),
                Times.Once);

            _mapperMock.Verify(
                m => m.MapToDTO(It.IsAny<TimescaleResult>()),
                Times.Never);
        }

        [Fact]
        public async Task GetResultsRangeAsync_WithNullSpecification_ThrowsArgumentNullException()
        {
            // Arrange
            ResultsSpecification nullParams = null!;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => _resultService.GetResultsRangeAsync(nullParams));
        }

        [Fact]
        public async Task GetResultsRangeAsync_WithValidParams_ReturnsResultDTOs()
        {
            // Arrange
            var resultParams = new ResultsSpecification
            {
                Name = "file.csv",
                MinDate = DateTime.Now.AddDays(-7),
                MaxDate = DateTime.Now,
                MinAvgExecutionTime = 0,
                MaxAvgExecutionTime = 1000,
                MinAvgValue = 0,
                MaxAvgValue = 1000
            };

            var testResults = new List<TimescaleResult>
            {
                new TimescaleResult
                {
                    Id = 0,
                    FileId = Guid.NewGuid(),
                    AvgValue = 100.5f,
                    MaxValue = 200.0f,
                    MinValue = 50.0f,
                    MedianValue = 95.5f
                },
                new TimescaleResult
                {
                    Id = 1,
                    FileId = Guid.NewGuid(),
                    AvgValue = 150.5f,
                    MaxValue = 250.0f,
                    MinValue = 75.0f,
                    MedianValue = 145.5f
                }
            };
            var expectedDTOs = new List<TimescaleResultDTO>
            {
                new TimescaleResultDTO
                {
                    AvgValue = testResults[0].AvgValue,
                    MaxValue = testResults[0].MaxValue,
                    MinValue = testResults[0].MinValue
                },
                new TimescaleResultDTO
                {
                    AvgValue = testResults[1].AvgValue,
                    MaxValue = testResults[1].MaxValue,
                    MinValue = testResults[1].MinValue
                }
            };

            _unitOfWorkMock
                .Setup(r => r.UploadedResults.GetFilteredAsync(resultParams))
                .ReturnsAsync(testResults);

            _mapperMock
                .Setup(m => m.MapToDTO(It.Is<TimescaleResult>(r => r.Id == testResults[0].Id)))
                .Returns(expectedDTOs[0]);

            _mapperMock
                .Setup(m => m.MapToDTO(It.Is<TimescaleResult>(r => r.Id == testResults[1].Id)))
                .Returns(expectedDTOs[1]);

            // Act
            var result = await _resultService.GetResultsRangeAsync(resultParams);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(expectedDTOs[0].AvgValue, result[0].AvgValue);
            Assert.Equal(expectedDTOs[1].AvgValue, result[1].AvgValue);

            _unitOfWorkMock.Verify(
                r => r.UploadedResults.GetFilteredAsync(resultParams),Times.Once);

            _mapperMock.Verify(
                m => m.MapToDTO(It.IsAny<TimescaleResult>()),
                Times.Exactly(2));
        }
    }
}
