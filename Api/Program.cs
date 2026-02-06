using Application.Services;
using Application.Services.VanTai;
using Core.Sql;
using Core.Sql.Config;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// =============================================================================
// CONFIGURATION
// Load connection strings and app settings from appsettings.json
// =============================================================================

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

// =============================================================================
// SERVICES REGISTRATION
// =============================================================================

// -----------------------------------------------------------------------------
// SSH Tunnel Service (MUST be registered BEFORE SQL Service)
// Creates SSH tunnel on startup, forwards localhost:LocalPort -> Remote MySQL
// Configured via appsettings.json "SshTunnel" section
// -----------------------------------------------------------------------------
builder.Services.AddSshTunnel(builder.Configuration);

// -----------------------------------------------------------------------------
// Core.Sql Library Services
// Register ISqlExecuteService from the Core.Sql library (Scoped lifetime)
// This service handles all database operations: paging, CRUD, batch, etc.
// -----------------------------------------------------------------------------
builder.Services.AddSqlExecuteService();

// -----------------------------------------------------------------------------
// MemoryCache for Stored Procedure Configuration
// Singleton: loaded once, cached for entire application lifetime
// Refresh can be triggered manually via IProcedureConfigProvider.RefreshCacheAsync()
// -----------------------------------------------------------------------------
builder.Services.AddMemoryCache();

// -----------------------------------------------------------------------------
// Procedure Configuration Provider (Dynamic SP Resolution)
// Singleton: SP name lookups are cached to avoid database hits on every call
// Maps FunctionKey -> ProcedureName from Sys_ProcedureConfig table
// -----------------------------------------------------------------------------
builder.Services.AddSingleton<IProcedureConfigProvider, ProcedureConfigProvider>();

// -----------------------------------------------------------------------------
// Application Services (Business Logic Layer)
// Register IProductService as Scoped - one instance per HTTP request
// ProductService depends on ISqlExecuteService for database operations
// -----------------------------------------------------------------------------
builder.Services.AddScoped<IProductService, ProductService>();

// -----------------------------------------------------------------------------
// VanTai (Transportation) Services
// Entity CRUD Services with integrated business operations
// -----------------------------------------------------------------------------
builder.Services.AddScoped<IBaoCaoService, BaoCaoService>();
builder.Services.AddScoped<ITaiXeService, TaiXeService>();
builder.Services.AddScoped<IXeService, XeService>();
builder.Services.AddScoped<IChuyenXeService, ChuyenXeService>();  // Includes: HoanThanhAsync, HuyChuyen
builder.Services.AddScoped<IKhachHangService, KhachHangService>();
builder.Services.AddScoped<ITuyenDuongService, TuyenDuongService>();
builder.Services.AddScoped<IVeService, VeService>();              // Includes: DatVeAsync, HuyVeAsync, GetByChuyenAsync
builder.Services.AddScoped<IMaintenanceService, MaintenanceService>();

// NOTE: Removed duplicate services:
// - ITripService, TripService (merged into IChuyenXeService)
// - ITicketService, TicketService (merged into IVeService)
// - ISearchService, SearchService (duplicate of entity search endpoints)

// -----------------------------------------------------------------------------
// ASP.NET Core MVC/API Services
// -----------------------------------------------------------------------------
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Configure JSON serialization for API responses
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.WriteIndented = builder.Environment.IsDevelopment();
    });

// -----------------------------------------------------------------------------
// API Documentation (Swagger/OpenAPI)
// -----------------------------------------------------------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Hệ Thống Quản Lý Vận Tải & Sản Phẩm",
        Version = "v1",
        Description = "API quản lý vận tải hành khách đường dài và quản lý sản phẩm - sử dụng Core.Sql library.",
        Contact = new OpenApiContact
        {
            Name = "Development Team",
            Email = "dev@example.com"
        }
    });

    // Include XML comments for API documentation
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// -----------------------------------------------------------------------------
// CORS Policy (for development/testing)
// -----------------------------------------------------------------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// -----------------------------------------------------------------------------
// Health Checks (optional but recommended)
// -----------------------------------------------------------------------------
builder.Services.AddHealthChecks();

// =============================================================================
// LOGGING CONFIGURATION
// =============================================================================

builder.Logging
    .ClearProviders()
    .AddConsole()
    .AddDebug();

if (builder.Environment.IsDevelopment())
{
    builder.Logging.SetMinimumLevel(LogLevel.Debug);
}
else
{
    builder.Logging.SetMinimumLevel(LogLevel.Information);
}

// =============================================================================
// BUILD APPLICATION
// =============================================================================

var app = builder.Build();

// =============================================================================
// MIDDLEWARE PIPELINE
// =============================================================================

// Enable Swagger in Development and SshTunnel environments
var enableSwagger = app.Environment.IsDevelopment() ||
                    app.Environment.EnvironmentName.Equals("SshTunnel", StringComparison.OrdinalIgnoreCase);

if (enableSwagger)
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Quản Lý Vận Tải API v1");
        options.RoutePrefix = "swagger"; // Swagger at /swagger URL
    });
}
else
{
    // Production error handling
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

// Security middleware - skip HTTPS redirect in development environments
if (!enableSwagger)
{
    app.UseHttpsRedirection();
}
;

// CORS
app.UseCors("AllowAll");

// Routing
app.UseRouting();

// Authorization (if needed in future)
app.UseAuthorization();

// Map controllers
app.MapControllers();

// Health check endpoint
app.MapHealthChecks("/health");

// =============================================================================
// RUN APPLICATION
// =============================================================================

app.Logger.LogInformation("Starting Quản Lý Vận Tải API...");
app.Logger.LogInformation("Environment: {Environment}", app.Environment.EnvironmentName);
app.Logger.LogInformation("Swagger UI: {Url}", enableSwagger ? "/swagger" : "Disabled");

app.Run();
