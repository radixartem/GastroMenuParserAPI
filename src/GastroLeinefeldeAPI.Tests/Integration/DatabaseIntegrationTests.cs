using GastroLeinefeldeAPI.Data;
using GastroLeinefeldeAPI.Models;
using GastroLeinefeldeAPI.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;
using FluentAssertions;

namespace GastroLeinefeldeAPI.Tests.Integration;

public class DatabaseIntegrationTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly MealRepository _repository;

    public DatabaseIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _context = new AppDbContext(options);
        _repository = new MealRepository(_context, 
            new Microsoft.Extensions.Logging.Abstractions.NullLogger<MealRepository>());
    }

    [Fact]
    public async Task AddAsync_ShouldAddMealToDatabase()
    {
        // Arrange
        var meal = new Meal
        {
            Name = "Test Gericht",
            Category = "Angebot des Tages",
            Price = 9.99m,
            Status = "Angebot"
        };

        // Act
        var result = await _repository.AddAsync(meal);

        // Assert
        result.Id.Should().BeGreaterThan(0);
        result.Name.Should().Be("Test Gericht");
        
        var saved = await _context.Meals.FindAsync(result.Id);
        saved.Should().NotBeNull();
        saved!.Name.Should().Be("Test Gericht");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnMeal()
    {
        // Arrange
        var meal = new Meal
        {
            Name = "Test Gericht",
            Category = "Angebot des Tages",
            Price = 9.99m
        };
        
        var added = await _repository.AddAsync(meal);

        // Act
        var result = await _repository.GetByIdAsync(added.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(added.Id);
        result.Name.Should().Be("Test Gericht");
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateMeal()
    {
        // Arrange
        var meal = new Meal
        {
            Name = "Test Gericht",
            Category = "Angebot des Tages",
            Price = 9.99m
        };
        
        var added = await _repository.AddAsync(meal);
        added.Name = "Aktualisiertes Gericht";
        added.Price = 12.99m;

        // Act
        var result = await _repository.UpdateAsync(added);

        // Assert
        result.Name.Should().Be("Aktualisiertes Gericht");
        result.Price.Should().Be(12.99m);
        
        var saved = await _context.Meals.FindAsync(added.Id);
        saved!.Name.Should().Be("Aktualisiertes Gericht");
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveMeal()
    {
        // Arrange
        var meal = new Meal
        {
            Name = "Test Gericht",
            Category = "Angebot des Tages",
            Price = 9.99m
        };
        
        var added = await _repository.AddAsync(meal);

        // Act
        var result = await _repository.DeleteAsync(added.Id);

        // Assert
        result.Should().BeTrue();
        
        var saved = await _context.Meals.FindAsync(added.Id);
        saved.Should().BeNull();
    }

    [Fact]
    public async Task GetPagedAsync_ShouldReturnCorrectPage()
    {
        // Arrange
        for (int i = 1; i <= 25; i++)
        {
            await _repository.AddAsync(new Meal
            {
                Name = $"Gericht {i}",
                Category = "Angebot des Tages",
                Price = 10.00m + i
            });
        }

        var filter = new MealFilterDto
        {
            Page = 2,
            PageSize = 10,
            SortBy = "Name",
            SortDescending = false
        };

        // Act
        var result = await _repository.GetPagedAsync(filter);

        // Assert
        result.Items.Should().HaveCount(10);
        result.TotalCount.Should().Be(25);
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(10);
        result.Items.First().Name.Should().Be("Gericht 11");
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}