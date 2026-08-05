using GastroLeinefeldeAPI.Controllers;
using GastroLeinefeldeAPI.Models;
using GastroLeinefeldeAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;

namespace GastroLeinefeldeAPI.Tests.Controllers;

public class MenuControllerTests
{
    private readonly Mock<IMenuService> _menuServiceMock;
    private readonly MenuController _controller;

    public MenuControllerTests()
    {
        _menuServiceMock = new Mock<IMenuService>();
        var loggerMock = new Mock<ILogger<MenuController>>();
        _controller = new MenuController(_menuServiceMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task ImportMenu_ShouldReturnOk_WithImportResult()
    {
        // Arrange
        var expectedResult = new ImportResult 
        { 
            Total = 5, 
            New = 3, 
            Updated = 2, 
            Source = "https://example.com" 
        };
        
        _menuServiceMock
            .Setup(x => x.ImportMenuAsync(It.IsAny<string>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.ImportMenu();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var resultValue = okResult.Value.Should().BeOfType<ImportResult>().Subject;
        resultValue.Total.Should().Be(5);
        resultValue.New.Should().Be(3);
        resultValue.Updated.Should().Be(2);
    }

    [Fact]
    public async Task GetAllMeals_ShouldReturnListOfMeals()
    {
        // Arrange
        var meals = new List<MealDto>
        {
            new() { Id = 1, Name = "Gericht 1", Category = "Angebot des Tages" },
            new() { Id = 2, Name = "Gericht 2", Category = "Unsere Klassiker" }
        };
        
        _menuServiceMock
            .Setup(x => x.GetAllMealsAsync())
            .ReturnsAsync(meals);

        // Act
        var result = await _controller.GetAllMeals();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var resultValue = okResult.Value.Should().BeAssignableTo<IEnumerable<MealDto>>().Subject;
        resultValue.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetMealById_WithExistingId_ShouldReturnMeal()
    {
        // Arrange
        var meal = new MealDto { Id = 1, Name = "Gericht 1" };
        _menuServiceMock
            .Setup(x => x.GetMealByIdAsync(1))
            .ReturnsAsync(meal);

        // Act
        var result = await _controller.GetMealById(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var resultValue = okResult.Value.Should().BeOfType<MealDto>().Subject;
        resultValue.Id.Should().Be(1);
    }

    [Fact]
    public async Task GetMealById_WithNonExistingId_ShouldReturnNotFound()
    {
        // Arrange
        _menuServiceMock
            .Setup(x => x.GetMealByIdAsync(999))
            .ReturnsAsync((MealDto?)null);

        // Act
        var result = await _controller.GetMealById(999);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task CreateMeal_ShouldReturnCreated()
    {
        // Arrange
        var dto = new CreateMealDto
        {
            Name = "Neues Gericht",
            Category = "Angebot des Tages",
            Price = 9.99m
        };
        
        var created = new MealDto 
        { 
            Id = 1, 
            Name = dto.Name, 
            Category = dto.Category, 
            Price = dto.Price 
        };
        
        _menuServiceMock
            .Setup(x => x.CreateMealAsync(dto))
            .ReturnsAsync(created);

        // Act
        var result = await _controller.CreateMeal(dto);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        var resultValue = createdResult.Value.Should().BeOfType<MealDto>().Subject;
        resultValue.Name.Should().Be("Neues Gericht");
    }

    [Fact]
    public async Task DeleteMeal_WithExistingId_ShouldReturnNoContent()
    {
        // Arrange
        _menuServiceMock
            .Setup(x => x.DeleteMealAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteMeal(1);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteMeal_WithNonExistingId_ShouldReturnNotFound()
    {
        // Arrange
        _menuServiceMock
            .Setup(x => x.DeleteMealAsync(999))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteMeal(999);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }
}