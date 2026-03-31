using Microsoft.EntityFrameworkCore;
using ProposalService.Services;
using System.Text.Json.Serialization;
using WorkService.Data;
using WorkService.Exceptions;
using WorkService.Repositories;
using WorkService.Services;

var builder = WebApplication.CreateBuilder(args);

//����������� ��������
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//����������� � ��
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

//DI ��� ������������ � ��������
builder.Services.AddScoped<TaskRepository>();
builder.Services.AddScoped<TaskService>();
builder.Services.AddScoped<NotificationServiceClient>();
builder.Services.AddScoped<ProposalServiceClient>();

builder.Services.AddHttpClient<TaskService>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5211/"); // WorkService
});

builder.Services.AddControllers()
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = null;
    options.JsonSerializerOptions.WriteIndented = true;
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddHttpClient<ProposalServiceClient>((sp, client) =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    client.BaseAddress = new Uri(config["Services:ProposalService"]);
});

builder.Services.AddHttpClient<NotificationServiceClient>((sp, client) =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    client.BaseAddress = new Uri(config["Services:NotificationService"]);
});
//�������� ����������
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

//Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseExceptionHandler();
app.UseStatusCodePages();
//app.UseHttpsRedirection();

app.MapControllers();

app.Run();