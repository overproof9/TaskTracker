using Microsoft.OpenApi.Models;
using TaskTracker.Infrastructure.Extensions;
using TaskTracker.Application.ServiceCollectionExtensions;
using FluentValidation;
using TaskTracker.Presentation.Validation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddValidatorsFromAssemblyContaining<CreateTaskRequestValidator>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Task Tracker API",
        Version = "v1",
        Description = "Test project with layered architecture"
    });
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;

builder.Services.AddInfrastructure(connectionString);
builder.Services.AddApplication();

var app = builder.Build();
app.MapControllers(); // или endpoints

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();

public partial class Program { }
