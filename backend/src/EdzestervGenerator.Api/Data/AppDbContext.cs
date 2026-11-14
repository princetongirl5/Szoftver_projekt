using EdzestervGenerator.Api.Data.Seed;
using EdzestervGenerator.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EdzestervGenerator.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<User, IdentityRole<int>, int>(options)
{
    public DbSet<Equipment> Equipment => Set<Equipment>();
    public DbSet<UserEquipment> UserEquipment => Set<UserEquipment>();
    public DbSet<Exercise> Exercises => Set<Exercise>();
    public DbSet<Workout> Workouts => Set<Workout>();
    public DbSet<WorkoutExercise> WorkoutExercises => Set<WorkoutExercise>();
    public DbSet<PlanPreference> PlanPreferences => Set<PlanPreference>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<User>().ToTable("Users");
        modelBuilder.Entity<Equipment>().HasData(EquipmentSeed.Items);
        modelBuilder.Entity<PlanPreference>(entity =>
        {
            entity.HasKey(x => x.UserId);
            entity.HasOne(x => x.User).WithOne(x => x.PlanPreference)
                .HasForeignKey<PlanPreference>(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<User>().HasIndex(x => x.NormalizedEmail).IsUnique();
    }
}
