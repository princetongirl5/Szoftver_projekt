using EdzestervGenerator.Api.Data.Seed;
using EdzestervGenerator.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace EdzestervGenerator.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Equipment> Equipment => Set<Equipment>();
    public DbSet<UserEquipment> UserEquipment => Set<UserEquipment>();
    public DbSet<Exercise> Exercises => Set<Exercise>();
    public DbSet<Workout> Workouts => Set<Workout>();
    public DbSet<WorkoutExercise> WorkoutExercises => Set<WorkoutExercise>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Equipment>().HasData(EquipmentSeed.Items);
    }
}
