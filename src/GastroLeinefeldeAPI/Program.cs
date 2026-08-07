using GastroLeinefeldeAPI.Data;
using GastroLeinefeldeAPI.Services;
using Microsoft.EntityFrameworkCore;
using Prometheus; // <-- Добавлен using

var builder = WebApplication.CreateBuilder(args);

// Конфигурация с приоритетом переменных окружения (для production)
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables(); // Переопределение через переменные окружения

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database – строка подключения из конфигурации (может быть переопределена через env)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Host=postgres;Database=gastro_menu;Username=postgres;Password=postgres";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// Services
builder.Services.AddHttpClient<IWebsiteClient, WebsiteClient>();
builder.Services.AddScoped<IMenuParser, MenuParser>();
builder.Services.AddScoped<IMealRepository, MealRepository>();
builder.Services.AddScoped<IMenuService, MenuService>();

// Health Checks – добавляем проверку БД
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>();

var app = builder.Build();

// Автоматическая миграция при запуске (только в production можно выполнять осторожно)
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.MigrateAsync();
}

// Middleware – порядок важен!
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // В production перенаправление на HTTPS (если Caddy не обрабатывает)
    app.UseHttpsRedirection();
}

// Добавляем сбор HTTP-метрик (количество запросов, длительность, статус коды)
app.UseHttpMetrics();

// Добавляем эндпоинт /metrics для Prometheus (по умолчанию)
app.UseMetricServer();

app.MapHealthChecks("/health");

app.UseAuthorization();
app.MapControllers();

await app.RunAsync();