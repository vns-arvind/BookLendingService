using BookLending.Application.Interfaces;
using BookLending.Infrastructure.Data;
using BookLending.Infrastructure.Repositories;
using BookLending.Middleware;
using BookLending.Services;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Polly;
using Serilog;

// --- Setup Serilog ---
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .Enrich.FromLogContext()
    .MinimumLevel.Information()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

// --- Controllers + ProblemDetails ---
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        // Let ASP.NET Core automatically return ValidationProblemDetails
        options.InvalidModelStateResponseFactory = context =>
        {
            var problemDetails = new ValidationProblemDetails(context.ModelState)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "One or more validation errors occurred.",
                Detail = "See the errors property for details."
            };
            return new BadRequestObjectResult(problemDetails);
        };
    });

// --- Swagger ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache();

// --- DbContext ---
builder.Services.AddDbContext<BookContext>(opts =>
    opts.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=books.db"));

// --- Dependency Injection ---
builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<IBookService, BookService>();

// --- FluentValidation ---
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<BookLending.Validators.CreateBookDtoValidator>();

// --- Health Checks ---
builder.Services.AddHealthChecks()
    .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy());

// --- ProblemDetails Middleware (.NET 8) ---
builder.Services.AddProblemDetails();

var app = builder.Build();

// --- Database Init ---
using (var scope = app.Services.CreateScope())
{
    var ctx = scope.ServiceProvider.GetRequiredService<BookContext>();
    var policy = Policy
    .Handle<SqliteException>()
    .Or<DbUpdateException>()
    .WaitAndRetry(3, retry => TimeSpan.FromSeconds(5));
    ctx.Database.EnsureCreated();
}

// --- Middlewares ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging(); // Log requests
app.UseExceptionHandler();       // Use built-in ProblemDetails for unhandled exceptions
app.UseMiddleware<ExceptionHandlingMiddleware>(); // (Optional) keep if you have domain-specific exceptions

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
