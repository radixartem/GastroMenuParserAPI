using GastroLeinefeldeAPI.Models;
using GastroLeinefeldeAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace GastroLeinefeldeAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class MenuController : ControllerBase
{
    private readonly IMenuService _menuService;
    private readonly ILogger<MenuController> _logger;

    public MenuController(IMenuService menuService, ILogger<MenuController> logger)
    {
        _menuService = menuService;
        _logger = logger;
    }

    [HttpPost("import")]
    public async Task<IActionResult> ImportMenu([FromQuery] string? url = null)
    {
        try
        {
            var targetUrl = url ?? "https://essen-auf-raedern-eichsfeld.de/tagesangebot";
            _logger.LogInformation("Import gestartet mit URL: {Url}", targetUrl);
            
            var result = await _menuService.ImportMenuAsync(targetUrl);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Import");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAllMeals()
    {
        var meals = await _menuService.GetAllMealsAsync();
        return Ok(meals);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetMealById(int id)
    {
        var meal = await _menuService.GetMealByIdAsync(id);
        if (meal == null)
            return NotFound($"Gericht mit ID {id} nicht gefunden");
        
        return Ok(meal);
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActiveMeals()
    {
        var meals = await _menuService.GetActiveMealsAsync();
        return Ok(meals);
    }

    [HttpGet("category/{category}")]
    public async Task<IActionResult> GetMealsByCategory(string category)
    {
        var meals = await _menuService.GetMealsByCategoryAsync(category);
        return Ok(meals);
    }
}