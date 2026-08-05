using GastroLeinefeldeAPI.Models;

namespace GastroLeinefeldeAPI.Services;

public class MenuService : IMenuService
{
    private readonly IMealRepository _repository;
    private readonly IWebsiteClient _websiteClient;
    private readonly IMenuParser _parser;
    private readonly ILogger<MenuService> _logger;

    public MenuService(
        IMealRepository repository,
        IWebsiteClient websiteClient,
        IMenuParser parser,
        ILogger<MenuService> logger)
    {
        _repository = repository;
        _websiteClient = websiteClient;
        _parser = parser;
        _logger = logger;
    }

    public async Task<ImportResult> ImportMenuAsync(string url)
    {
        var result = new ImportResult { Source = url, Timestamp = DateTime.UtcNow };
        
        try
        {
            _logger.LogInformation("Starte Import von {Url}", url);
            
            // 1. HTML laden
            var html = await _websiteClient.FetchHtmlAsync(url);
            
            // 2. Parsen
            var parsedMeals = await _parser.ParseMenuAsync(html);
            var mealList = parsedMeals.ToList();
            result.Total = mealList.Count;
            
            _logger.LogInformation("{Count} Gerichte vom Parser gefunden", mealList.Count);
            
            // 3. In DB speichern/aktualisieren
            foreach (var meal in mealList)
            {
                if (string.IsNullOrEmpty(meal.Name))
                    continue;
                
                var existing = await _repository.GetByHashAsync(meal.Hash);
                
                if (existing == null)
                {
                    meal.Source = url;
                    await _repository.AddAsync(meal);
                    result.New++;
                }
                else if (HasMealChanged(existing, meal))
                {
                    existing.Name = meal.Name;
                    existing.Price = meal.Price;
                    existing.Status = meal.Status;
                    existing.PreparationTime = meal.PreparationTime;
                    existing.Date = meal.Date;
                    existing.ImportedAt = DateTime.UtcNow;
                    existing.Source = url;
                    existing.IsActive = true;
                    
                    await _repository.UpdateAsync(existing);
                    result.Updated++;
                }
            }
            
            // 4. Alte Gerichte deaktivieren (nach 7 Tagen)
            var threshold = DateTime.UtcNow.AddDays(-7);
            await _repository.DeactivateOldMealsAsync(threshold);
            
            _logger.LogInformation("Import abgeschlossen: {New} neu, {Updated} aktualisiert", 
                result.New, result.Updated);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Import von {Url}", url);
            result.Errors.Add(ex.Message);
            throw;
        }
    }

    public async Task<IEnumerable<MealDto>> GetAllMealsAsync()
    {
        var meals = await _repository.GetAllAsync();
        return meals.Select(MapToDto);
    }

    public async Task<MealDto?> GetMealByIdAsync(int id)
    {
        var meal = await _repository.GetByIdAsync(id);
        return meal != null ? MapToDto(meal) : null;
    }

    public async Task<IEnumerable<MealDto>> GetActiveMealsAsync()
    {
        var meals = await _repository.GetActiveMealsAsync();
        return meals.Select(MapToDto);
    }

    public async Task<IEnumerable<MealDto>> GetMealsByCategoryAsync(string category)
    {
        var meals = await _repository.GetMealsByCategoryAsync(category);
        return meals.Select(MapToDto);
    }

    private bool HasMealChanged(Meal existing, Meal newMeal)
    {
        return existing.Name != newMeal.Name ||
               existing.Price != newMeal.Price ||
               existing.Status != newMeal.Status ||
               existing.PreparationTime != newMeal.PreparationTime;
    }

    private MealDto MapToDto(Meal meal)
    {
        return new MealDto
        {
            Id = meal.Id,
            Category = meal.Category,
            Name = meal.Name,
            Price = meal.Price,
            Status = meal.Status,
            PreparationTime = meal.PreparationTime,
            Date = meal.Date,
            ImportedAt = meal.ImportedAt,
            Source = meal.Source,
            IsActive = meal.IsActive
        };
    }
}