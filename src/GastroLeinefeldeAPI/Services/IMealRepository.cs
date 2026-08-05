using GastroLeinefeldeAPI.Models;

namespace GastroLeinefeldeAPI.Services;

public interface IMealRepository
{
    Task<Meal?> GetByIdAsync(int id);
    Task<IEnumerable<Meal>> GetAllAsync();
    Task<Meal> AddAsync(Meal meal);
    Task<Meal> UpdateAsync(Meal meal);
    Task<Meal?> GetByHashAsync(string hash);
    Task<IEnumerable<Meal>> GetActiveMealsAsync();
    Task<IEnumerable<Meal>> GetMealsByCategoryAsync(string category);
    Task<int> GetTotalCountAsync();
    Task DeactivateOldMealsAsync(DateTime threshold);
}