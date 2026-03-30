using Microsoft.EntityFrameworkCore;
using ProposalService.Services;
using System.Text.Json.Serialization;
using WorkService.Data;
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
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<ServiceProposal>();

builder.Services.AddControllers()
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = null;
    options.JsonSerializerOptions.WriteIndented = true;
});

builder.WebHost.UseUrls("http://0.0.0.0:8080");

//�������� ����������
var app = builder.Build();

//Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.MapControllers();

app.Run();