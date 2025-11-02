using BookLending.Application.Interfaces;
using BookLending.Infrastructure.Data;
using BookLending.Infrastructure.Repositories;
using BookLending.Services;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using BookLending.Middleware;
using Serilog;

// Didn't consider Polly because there is no external HTTP calls in this service

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()                   // basic structured logging
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day) // optional
    .Enrich.FromLogContext()             // adds request info (Path, etc.)
    .MinimumLevel.Information()
    .CreateLogger();

builder.Host.UseSerilog();

// Controllers + ProblemDetails
builder.Services.AddControllers()    
    .ConfigureApiBehaviorOptions(options =>
    {
        // Disable automatic 400 response — we'll handle manually in controller
        options.SuppressModelStateInvalidFilter = true;
    });

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache();

// DbContext
builder.Services.AddDbContext<BookContext>(opts =>
    opts.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=books.db"));

// DI
builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<IBookService, BookService>();

// FluentValidation
builder.Services.AddFluentValidationAutoValidation(); // Enables automatic validation if wanted
builder.Services.AddValidatorsFromAssemblyContaining<BookLending.Validators.CreateBookDtoValidator>();

// Health checks
builder.Services.AddHealthChecks()
    .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy());

// Exception handler (ProblemDetails)
builder.Services.AddProblemDetails(); // .NET 8+ built-in extension

var app = builder.Build();

// Database init
using (var scope = app.Services.CreateScope())
{
    var ctx = scope.ServiceProvider.GetRequiredService<BookContext>();
    ctx.Database.EnsureCreated();
}

// Middlewares
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseRouting();
app.MapControllers();
app.MapHealthChecks("/health");

try
{
    Log.Information("Starting Book Lending API...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application startup failed!");
}
finally
{
    Log.CloseAndFlush();
}
