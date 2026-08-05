using GastroLeinefeldeAPI.Data;
using GastroLeinefeldeAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace GastroLeinefeldeAPI.Services;

public class MealRepository : IMealRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<MealRepository> _logger;

    public MealRepository(AppDbContext context, ILogger<MealRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Meal?> GetByIdAsync(int id)
    {
        return await _context.Meals.FindAsync(id);
    }

    public async Task<IEnumerable<Meal>> GetAllAsync()
    {
        return await _context.Meals
            .OrderByDescending(m => m.ImportedAt)
            .ToListAsync();
    }

    public async Task<Meal> AddAsync(Meal meal)
    {
        meal.ImportedAt = DateTime.UtcNow;
        meal.Hash = ComputeHash(meal);
        
        await _context.Meals.AddAsync(meal);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Neues Gericht hinzugefügt: {Name}", meal.Name);
        return meal;
    }

    public async Task<Meal> UpdateAsync(Meal meal)
    {
        meal.Hash = ComputeHash(meal);
        _context.Meals.Update(meal);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Gericht aktualisiert: {Name}", meal.Name);
        return meal;
    }

    public async Task<Meal?> GetByHashAsync(string hash)
    {
        return await _context.Meals.FirstOrDefaultAsync(m => m.Hash == hash);
    }

    public async Task<IEnumerable<Meal>> GetActiveMealsAsync()
    {
        return await _context.Meals
            .Where(m => m.IsActive)
            .OrderByDescending(m => m.ImportedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Meal>> GetMealsByCategoryAsync(string category)
    {
        return await _context.Meals
            .Where(m => m.Category == category && m.IsActive)
            .OrderByDescending(m => m.ImportedAt)
            .ToListAsync();
    }

    public async Task<int> GetTotalCountAsync()
    {
        return await _context.Meals.CountAsync();
    }

    public async Task DeactivateOldMealsAsync(DateTime threshold)
    {
        var oldMeals = await _context.Meals
            .Where(m => m.IsActive && m.ImportedAt < threshold)
            .ToListAsync();

        foreach (var meal in oldMeals)
        {
            meal.IsActive = false;
        }

        if (oldMeals.Any())
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation("{Count} alte Gerichte deaktiviert", oldMeals.Count);
        }
    }

    private string ComputeHash(Meal meal)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var input = $"{meal.Category}|{meal.Name}|{meal.Price}|{meal.Status}|{meal.PreparationTime}";
        var bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes);
    }
}