using GS1.Core.Interfaces;
using GS1.Infrastructure.Data;
using GS1.Infrastructure.Repositories;
using GS1.Infrastructure.Services;
using GS1.WebAPI.Middleware;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// ========== Serilog Configuration ==========
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/gs1-api-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// ========== Database Configuration ==========
builder.Services.AddDbContext<GS1DbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null);
        });
});

// ========== Dependency Injection ==========
// Repositories
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IWorkOrderRepository, WorkOrderRepository>();
builder.Services.AddScoped<ISerialNumberRepository, SerialNumberRepository>();
builder.Services.AddScoped<ISSCCRepository, SSCCRepository>();

// Services
builder.Services.AddScoped<IGS1GeneratorService, GS1GeneratorService>();
builder.Services.AddScoped<ISerializationService, SerializationService>();
builder.Services.AddScoped<IAggregationService, AggregationService>();

// ========== API Configuration ==========
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "GS1 Serialization API",
        Version = "v1",
        Description = "GS1 L3 Serilizasyon ve Agregasyon Sistemi API'si",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "GS1 Serialization System",
            Email = "info@gs1serialization.com"
        }
    });
});

// ========== CORS Configuration ==========
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// ========== Middleware Pipeline ==========
app.UseExceptionHandling();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "GS1 Serialization API v1");
        options.RoutePrefix = string.Empty; // Swagger UI at root
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseSerilogRequestLogging();

app.MapControllers();

// ========== Database Migration (Development) ==========
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<GS1DbContext>();
    
    try
    {
        // Veritabanı migration'larını uygula
        context.Database.Migrate();
        Log.Information("Veritabanı migration'ları başarıyla uygulandı");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Veritabanı migration hatası");
    }
}

Log.Information("GS1 Serialization API başlatıldı");

app.Run();
