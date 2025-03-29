using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using PoSnakeGame.Api.Controllers;
using PoSnakeGame.Infrastructure.Services;
using PoSnakeGame.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace PoSnakeGame.Tests
{
    public class HighScoresControllerTests
    {
        private readonly Mock<ITableStorageService> _mockTableService;
        private readonly Mock<ILogger<HighScoresController>> _mockLogger;
        private readonly HighScoresController _controller;

        public HighScoresControllerTests()
        {
            _mockTableService = new Mock<ITableStorageService>();
            _mockLogger = new Mock<ILogger<HighScoresController>>();
            _controller = new HighScoresController(_mockLogger.Object, _mockTableService.Object);
        }

        [Fact]
        public async Task GetHighScores_ReturnsOkResult_WithListOfHighScores()
        {
            // Arrange
            var fakeScores = new List<HighScore>
            {
                new HighScore { Initials = "AAA", Score = 100, Date = DateTime.UtcNow },
                new HighScore { Initials = "BBB", Score = 90, Date = DateTime.UtcNow }
            };
            _mockTableService.Setup(service => service.GetTopScoresAsync(10))
                             .ReturnsAsync(fakeScores);

            // Act
            var result = await _controller.GetHighScores();

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<HighScore>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnedScores = Assert.IsAssignableFrom<IEnumerable<HighScore>>(okResult.Value);
            Assert.Equal(2, returnedScores.Count());
            _mockTableService.Verify(s => s.GetTopScoresAsync(10), Times.Once); // Verify service was called
        }

        [Fact]
        public async Task GetHighScores_ReturnsStatusCode500_WhenServiceThrowsException()
        {
            // Arrange
            _mockTableService.Setup(service => service.GetTopScoresAsync(10))
                             .ThrowsAsync(new Exception("Table storage error"));

            // Act
            var result = await _controller.GetHighScores();

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<HighScore>>>(result);
            var statusCodeResult = Assert.IsType<ObjectResult>(actionResult.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Equal("Internal server error retrieving high scores.", statusCodeResult.Value);
        }

        // === SaveHighScore Tests ===

        [Fact]
        public async Task SaveHighScore_ReturnsOkResult_WithValidHighScore()
        {
            // Arrange
            var newScore = new HighScore { Initials = "TST", Score = 150, Date = DateTime.UtcNow };
            // Setup mock to complete successfully
            _mockTableService.Setup(service => service.SaveHighScoreAsync(It.IsAny<HighScore>()))
                             .Returns(Task.CompletedTask); 

            // Act
            var result = await _controller.SaveHighScore(newScore);

            // Assert
            var actionResult = Assert.IsType<ActionResult<HighScore>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnedScore = Assert.IsType<HighScore>(okResult.Value);
            Assert.Equal(newScore.Initials, returnedScore.Initials);
            Assert.Equal(newScore.Score, returnedScore.Score);
            _mockTableService.Verify(s => s.SaveHighScoreAsync(It.Is<HighScore>(hs => hs.Initials == "TST" && hs.Score == 150)), Times.Once);
        }

        [Fact]
        public async Task SaveHighScore_ReturnsBadRequest_WithNullHighScore()
        {
            // Arrange
            HighScore? newScore = null;

            // Act
            var result = await _controller.SaveHighScore(newScore!); // Use null-forgiving operator for test

            // Assert
            var actionResult = Assert.IsType<ActionResult<HighScore>>(result);
            Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        }

        [Fact]
        public async Task SaveHighScore_ReturnsStatusCode500_WhenServiceThrowsException()
        {
            // Arrange
            var newScore = new HighScore { Initials = "ERR", Score = 50, Date = DateTime.UtcNow };
            _mockTableService.Setup(service => service.SaveHighScoreAsync(It.IsAny<HighScore>()))
                             .ThrowsAsync(new Exception("Table storage error"));

            // Act
            var result = await _controller.SaveHighScore(newScore);

            // Assert
            var actionResult = Assert.IsType<ActionResult<HighScore>>(result);
            var statusCodeResult = Assert.IsType<ObjectResult>(actionResult.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Equal("Internal server error saving high score.", statusCodeResult.Value);
        }

        // === IsHighScore Tests ===

        [Theory]
        [InlineData(100, true)]  // Score is high enough
        [InlineData(50, false)] // Score is not high enough
        public async Task IsHighScore_ReturnsCorrectBooleanResult(int scoreToCheck, bool expectedResult)
        {
            // Arrange
            _mockTableService.Setup(service => service.IsHighScore(scoreToCheck))
                             .ReturnsAsync(expectedResult);

            // Act
            var result = await _controller.IsHighScore(scoreToCheck);

            // Assert
            var actionResult = Assert.IsType<ActionResult<bool>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            Assert.Equal(expectedResult, okResult.Value);
            _mockTableService.Verify(s => s.IsHighScore(scoreToCheck), Times.Once);
        }

        [Fact]
        public async Task IsHighScore_ReturnsStatusCode500_WhenServiceThrowsException()
        {
            // Arrange
            int scoreToCheck = 120;
             _mockTableService.Setup(service => service.IsHighScore(scoreToCheck))
                             .ThrowsAsync(new Exception("Table storage error"));

            // Act
            var result = await _controller.IsHighScore(scoreToCheck);

            // Assert
            var actionResult = Assert.IsType<ActionResult<bool>>(result);
            var statusCodeResult = Assert.IsType<ObjectResult>(actionResult.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Equal("Internal server error checking high score.", statusCodeResult.Value);
        }
    }
}
