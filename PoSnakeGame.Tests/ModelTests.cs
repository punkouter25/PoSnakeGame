using PoSnakeGame.Core.Models;
using System;
using System.Drawing;
using Xunit;

namespace PoSnakeGame.Tests;

public class ModelTests
{
    [Fact]
    public void Position_Addition_Works_Correctly()
    {
        // Arrange
        var position1 = new Position(5, 5);
        var position2 = new Position(3, 2);
        
        // Act
        var result = position1 + position2;
        
        // Assert
        Assert.Equal(8, result.X);
        Assert.Equal(7, result.Y);
    }
    
    [Fact]
    public void Position_FromDirection_Returns_Correct_Values()
    {
        // Arrange & Act & Assert
        Assert.Equal(new Position(0, -1), Position.FromDirection(Direction.Up));
        Assert.Equal(new Position(0, 1), Position.FromDirection(Direction.Down));
        Assert.Equal(new Position(-1, 0), Position.FromDirection(Direction.Left));
        Assert.Equal(new Position(1, 0), Position.FromDirection(Direction.Right));
    }
    
    [Fact]
    public void Position_FromDirection_Throws_On_Invalid_Direction()
    {
        // Arrange
        Direction invalidDirection = (Direction)99;
        
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Position.FromDirection(invalidDirection));
    }
    
    [Fact]
    public void Snake_Grow_Increases_Length()
    {
        // Arrange
        var snake = new Snake(new Position(5, 5), Direction.Right, Color.Green, SnakeType.Human);
        int initialLength = snake.Length;
        
        // Act
        snake.Grow();
        
        // Assert
        Assert.Equal(initialLength + 1, snake.Length);
    }
    
    [Fact]
    public void Snake_AddPoints_Increases_Score()
    {
        // Arrange
        var snake = new Snake(new Position(5, 5), Direction.Right, Color.Green, SnakeType.Human);
        int initialScore = snake.Score;
        int pointsToAdd = 10;
        
        // Act
        snake.AddPoints(pointsToAdd);
        
        // Assert
        Assert.Equal(initialScore + pointsToAdd, snake.Score);
    }
    
    [Fact]
    public void Snake_Move_Adds_New_Head_Position()
    {
        // Arrange
        var startPosition = new Position(5, 5);
        var snake = new Snake(startPosition, Direction.Right, Color.Green, SnakeType.Human);
        var newHeadPosition = new Position(6, 5);
        
        // Act
        snake.Move(newHeadPosition);
        
        // Assert
        Assert.Equal(newHeadPosition, snake.Segments[0]);
        Assert.Single(snake.Segments); // Length = 1, so should have only one segment
    }
    
    [Fact]
    public void Snake_Move_Maintains_Length_When_Growing()
    {
        // Arrange
        var startPosition = new Position(5, 5);
        var snake = new Snake(startPosition, Direction.Right, Color.Green, SnakeType.Human);
        snake.Grow(); // Length = 2
        
        // Act
        snake.Move(new Position(6, 5)); // Move right
        snake.Move(new Position(7, 5)); // Move right again
        
        // Assert
        Assert.Equal(2, snake.Segments.Count); // Should maintain length of 2
        Assert.Equal(new Position(7, 5), snake.Segments[0]); // Head position
        Assert.Equal(new Position(6, 5), snake.Segments[1]); // Tail position
    }
    
    [Fact]
    public void Arena_IsOutOfBounds_Returns_Correct_Results()
    {
        // Arrange
        var arena = new Arena(10, 10);
        
        // Act & Assert
        Assert.False(arena.IsOutOfBounds(new Position(5, 5))); // Valid position
        Assert.True(arena.IsOutOfBounds(new Position(-1, 5))); // Out of left boundary
        Assert.True(arena.IsOutOfBounds(new Position(10, 5))); // Out of right boundary (10 is out because it's 0-based)
        Assert.True(arena.IsOutOfBounds(new Position(5, -1))); // Out of top boundary
        Assert.True(arena.IsOutOfBounds(new Position(5, 10))); // Out of bottom boundary
    }
}