using SeleniumDashboardApi.Data;
using Microsoft.EntityFrameworkCore;
using SeleniumDashboardApi.Hubs;
using SeleniumDashboardApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer(); // Voor Swagger
builder.Services.AddSwaggerGen();           // Voor Swagger UI
builder.Services.AddSignalR();
builder.Services.AddScoped<INotificationService, NotificationService>();

builder.Services.AddDbContext<TestRunDbContext>(options =>
    options.UseSqlServer("Server=localhost\\SQLEXPRESS;Database=SeleniumDashboard;Trusted_Connection=True;TrustServerCertificate=True;"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();        // Genereert swagger.json
    app.UseSwaggerUI();      // Activeert Swagger UI
}

// app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapHub<TestRunHub>("/testrunhub");

app.Run();
