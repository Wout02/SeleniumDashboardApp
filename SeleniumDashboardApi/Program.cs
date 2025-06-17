using SeleniumDashboardApi.Data;
using Microsoft.EntityFrameworkCore;
using SeleniumDashboardApi.Hubs;
using SeleniumDashboardApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
builder.Services.AddScoped<INotificationService, NotificationService>();

// Database configuratie - ALTIJD PostgreSQL
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

if (!string.IsNullOrEmpty(databaseUrl))
{
    // Production: PostgreSQL op Railway
    Console.WriteLine("Using PostgreSQL database (Production)");

    // Parse Railway DATABASE_URL format: postgres://user:pass@host:port/db
    var databaseUri = new Uri(databaseUrl);
    var userInfo = databaseUri.UserInfo.Split(':');

    var connectionString = new Npgsql.NpgsqlConnectionStringBuilder
    {
        Host = databaseUri.Host,
        Port = databaseUri.Port,
        Username = userInfo[0],
        Password = userInfo[1],
        Database = databaseUri.LocalPath.TrimStart('/'),
        SslMode = Npgsql.SslMode.Require,
        TrustServerCertificate = true
    }.ToString();

    Console.WriteLine($"PostgreSQL connection string: {connectionString}");

    builder.Services.AddDbContext<TestRunDbContext>(options =>
        options.UseNpgsql(connectionString));
}
else
{
    // Development: Ook PostgreSQL (tijdelijk voor migrations)
    Console.WriteLine("Using PostgreSQL database (Development)");
    var connectionString = "Host=localhost;Database=seleniumdashboard_dev;Username=postgres;Password=password;";
    builder.Services.AddDbContext<TestRunDbContext>(options =>
        options.UseNpgsql(connectionString));
}

// CORS configuratie
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Port configuratie voor Railway
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://*:{port}");

var app = builder.Build();

// Database migraties uitvoeren bij startup
try
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<TestRunDbContext>();

        Console.WriteLine("Running database migrations...");
        context.Database.Migrate();
        Console.WriteLine("Database migrations completed successfully");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Database migration failed: {ex.Message}");
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // Ook Swagger in production voor testen
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "SeleniumDashboard API V1");
        c.RoutePrefix = "swagger";
    });
}

app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();
app.MapHub<TestRunHub>("/testrunhub");

// Health check endpoint
app.MapGet("/health", () => "API is running!");

Console.WriteLine($"Starting application on port {port}");
app.Run();