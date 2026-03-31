using Microsoft.EntityFrameworkCore;
using ProposalService.Services;
using System.Text.Json.Serialization;
using WorkService.Data;
using WorkService.Exceptions;
using WorkService.Repositories;
using WorkService.Services;

var builder = WebApplication.CreateBuilder(args);

// Регистрация сервисов
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = null;
        options.JsonSerializerOptions.WriteIndented = true;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Подключение к БД
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Репозитории
builder.Services.AddScoped<TaskRepository>();

// Сервисы с HttpClient
builder.Services.AddHttpClient<TaskService>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5211/"); // WorkService
});

builder.Services.AddHttpClient<ProposalServiceClient>((sp, client) =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var url = config["ExternalServices:WorkService"]; 
    if (string.IsNullOrEmpty(url))
        throw new InvalidOperationException("WorkService URL is not configured.");

    client.BaseAddress = new Uri(url);
});

builder.Services.AddHttpClient<NotificationServiceClient>((sp, client) =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var url = config["ExternalServices:NotificationService"]; 
    if (string.IsNullOrEmpty(url))
        throw new InvalidOperationException("NotificationService URL is not configured.");

    client.BaseAddress = new Uri(url);
});

// Глобальный обработчик исключений
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Создание приложения
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();
app.UseStatusCodePages();
// app.UseHttpsRedirection();

app.MapControllers();

app.Run();