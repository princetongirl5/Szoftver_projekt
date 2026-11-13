using EdzestervGenerator.Api.Services;
using EdzestervGenerator.Api.Data.Seed;
using System.Text.Json.Serialization;
using EdzestervGenerator.Api.Data;
using EdzestervGenerator.Api.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Enums are sent as text in JSON, e.g. "Football" instead of 1.
builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<IWorkoutGenerator, WorkoutGenerator>();

var app = builder.Build();

// Create or update the database on startup.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    // The seed is the source of truth: add missing exercises and refresh the existing ones,
    // so older dev databases also get the English names and the new gym exercises.
    var existing = db.Exercises.ToDictionary(e => e.Id);
    foreach (var item in ExerciseSeed.Items)
    {
        if (existing.TryGetValue(item.Id, out var current))
        {
            current.Name = item.Name;
            current.NameEn = item.NameEn;
            current.MuscleGroup = item.MuscleGroup;
            current.Difficulty = item.Difficulty;
            current.EquipmentId = item.EquipmentId;
            current.MinWeightKg = item.MinWeightKg;
        }
        else
        {
            db.Exercises.Add(new Exercise
            {
                Id = item.Id,
                Name = item.Name,
                NameEn = item.NameEn,
                MuscleGroup = item.MuscleGroup,
                Difficulty = item.Difficulty,
                EquipmentId = item.EquipmentId,
                MinWeightKg = item.MinWeightKg
            });
        }
    }
    db.SaveChanges();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
