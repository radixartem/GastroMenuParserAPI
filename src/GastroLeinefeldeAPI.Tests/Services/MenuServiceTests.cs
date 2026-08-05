using GastroLeinefeldeAPI.Models;
using GastroLeinefeldeAPI.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;

namespace GastroLeinefeldeAPI.Tests.Services;

public class MenuServiceTests
{
    private readonly Mock<IMealRepository> _repositoryMock;
    private readonly Mock<IWebsiteClient> _websiteClientMock;
    private readonly Mock<IMenuParser> _parserMock;
    private readonly Mock<ILogger<MenuService>> _loggerMock;
    private readonly MenuService _service;

    public MenuServiceTests()
    {
        _repositoryMock = new Mock<IMealRepository>();
        _websiteClientMock = new Mock<IWebsiteClient>();
        _parserMock = new Mock<IMenuParser>();
        _loggerMock = new Mock<ILogger<MenuService>>();
        
        _service = new MenuService(
            _repositoryMock.Object,
            _websiteClientMock.Object,
            _parserMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task ImportMenuAsync_ShouldImportNewMeals()
    {
        // Arrange
        var html = "<html>Test</html>";
        var parsedMeals = new List<Meal>
        {
            new() 
            { 
                Name = "Gericht 1", 
                Category = "Angebot des Tages",
                Price = 9.90m,
                Hash = "hash1"
            },
            new() 
            { 
                Name = "Gericht 2", 
                Category = "Unsere Klassiker",
                Price = 8.40m,
                Hash = "hash2"
            }
        };

        _websiteClientMock
            .Setup(x => x.FetchHtmlAsync(It.IsAny<string>()))
            .ReturnsAsync(html);

        _parserMock
            .Setup(x => x.ParseMenuAsync(html))
            .ReturnsAsync(parsedMeals);

        _repositoryMock
            .Setup(x => x.GetByHashAsync(It.IsAny<string>()))
            .ReturnsAsync((Meal?)null);

        _repositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Meal>()))
            .ReturnsAsync((Meal m) => m);

        // Act
        var result = await _service.ImportMenuAsync("https://example.com");

        // Assert
        result.Total.Should().Be(2);
        result.New.Should().Be(2);
        result.Updated.Should().Be(0);
        
        _repositoryMock.Verify(x => x.AddAsync(It.IsAny<Meal>()), Times.Exactly(2));
    }

    [Fact]
    public async Task ImportMenuAsync_ShouldUpdateExistingMeals()
    {
        // Arrange
        var html = "<html>Test</html>";
        var parsedMeals = new List<Meal>
        {
            new() 
            { 
                Name = "Gericht 1 (updated)", 
                Category = "Angebot des Tages",
                Price = 10.90m,
                Hash = "hash1"
            }
        };

        var existingMeal = new Meal
        {
            Id = 1,
            Name = "Gericht 1",
            Category = "Angebot des Tages",
            Price = 9.90m,
            Hash = "hash1"
        };

        _websiteClientMock
            .Setup(x => x.FetchHtmlAsync(It.IsAny<string>()))
            .ReturnsAsync(html);

        _parserMock
            .Setup(x => x.ParseMenuAsync(html))
            .ReturnsAsync(parsedMeals);

        _repositoryMock
            .Setup(x => x.GetByHashAsync("hash1"))
            .ReturnsAsync(existingMeal);

        _repositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Meal>()))
            .ReturnsAsync((Meal m) => m);

        // Act
        var result = await _service.ImportMenuAsync("https://example.com");

        // Assert
        result.Total.Should().Be(1);
        result.New.Should().Be(0);
        result.Updated.Should().Be(1);
        
        _repositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Meal>()), Times.Once);
    }

    [Fact]
    public async Task GetAllMealsAsync_ShouldReturnAllMeals()
    {
        // Arrange
        var meals = new List<Meal>
        {
            new() { Id = 1, Name = "Gericht 1" },
            new() { Id = 2, Name = "Gericht 2" }
        };
        
        _repositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(meals);

        // Act
        var result = await _service.GetAllMealsAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllBeOfType<MealDto>();
    }

    [Fact]
    public async Task GetPagedMealsAsync_ShouldReturnPagedResult()
    {
        // Arrange
        var filter = new MealFilterDto { Page = 1, PageSize = 10 };
        var meals = new List<Meal>
        {
            new() { Id = 1, Name = "Gericht 1" }
        };
        
        var pagedResult = new PagedResult<Meal>
        {
            Items = meals,
            TotalCount = 1,
            Page = 1,
            PageSize = 10
        };
        
        _repositoryMock
            .Setup(x => x.GetPagedAsync(filter))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _service.GetPagedMealsAsync(filter);

        // Assert
        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);
        result.Page.Should().Be(1);
    }

    [Fact]
    public async Task CreateMealAsync_ShouldAddNewMeal()
    {
        // Arrange
        var dto = new CreateMealDto
        {
            Name = "Neues Gericht",
            Category = "Angebot des Tages",
            Price = 9.99m
        };
        
        var createdMeal = new Meal
        {
            Id = 1,
            Name = dto.Name,
            Category = dto.Category,
            Price = dto.Price
        };
        
        _repositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Meal>()))
            .ReturnsAsync(createdMeal);

        // Act
        var result = await _service.CreateMealAsync(dto);

        // Assert
        result.Id.Should().Be(1);
        result.Name.Should().Be("Neues Gericht");
        result.Price.Should().Be(9.99m);
    }

    [Fact]
    public async Task DeleteMealAsync_WithExistingId_ShouldReturnTrue()
    {
        // Arrange
        _repositoryMock
            .Setup(x => x.DeleteAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _service.DeleteMealAsync(1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteMealAsync_WithNonExistingId_ShouldReturnFalse()
    {
        // Arrange
        _repositoryMock
            .Setup(x => x.DeleteAsync(999))
            .ReturnsAsync(false);

        // Act
        var result = await _service.DeleteMealAsync(999);

        // Assert
        result.Should().BeFalse();
    }
}