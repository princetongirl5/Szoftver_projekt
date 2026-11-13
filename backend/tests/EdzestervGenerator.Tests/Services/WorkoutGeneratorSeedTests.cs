using EdzestervGenerator.Api.Data.Seed;
using EdzestervGenerator.Api.DTOs;
using EdzestervGenerator.Api.Models;
using EdzestervGenerator.Api.Services;

namespace EdzestervGenerator.Tests.Services;

// Runs the generator on the real seed (home and gym exercises), with many random seeds,
// so the checks do not depend on one lucky shuffle.
public sealed class WorkoutGeneratorSeedTests
{
    private static readonly int[] HomeEquipment = [1, 2, 3, 4, 5, 6, 7];

    private static readonly List<DayOfWeek> ThreeDays = [DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday];
    private static readonly List<DayOfWeek> FourDays = [DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Thursday, DayOfWeek.Friday];
    private static readonly List<DayOfWeek> SixDays =
    [
        DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
        DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday
    ];

    private static IReadOnlyList<GeneratedWorkoutDay> Run(TrainingLocation location, int[] equipment,
        List<DayOfWeek> days, int minutes, Difficulty difficulty, int seed)
    {
        var request = new GeneratePlanRequest(TrainingGoal.General, location, equipment.ToList(), days, minutes, difficulty);
        return new WorkoutGenerator().GenerateWeek(request, ExerciseSeed.Items, equipment, new Random(seed));
    }

    private static bool IsGymEquipment(Exercise e) => e.EquipmentId is >= 8 and <= 12;

    [Fact]
    public void Gym_weeks_use_the_gym_equipment_exercises()
    {
        var used = Enumerable.Range(1, 30).Any(seed =>
            Run(TrainingLocation.Gym, [], FourDays, 60, Difficulty.Hard, seed)
                .SelectMany(d => d.Exercises)
                .Any(e => IsGymEquipment(e.Exercise)));

        Assert.True(used);
    }

    [Fact]
    public void Home_with_all_home_equipment_never_uses_gym_equipment()
    {
        for (var seed = 1; seed <= 30; seed++)
        {
            var result = Run(TrainingLocation.Home, HomeEquipment, SixDays, 90, Difficulty.Hard, seed);

            Assert.DoesNotContain(result.SelectMany(d => d.Exercises), e => IsGymEquipment(e.Exercise));
        }
    }

    [Fact]
    public void Home_without_equipment_is_bodyweight_only()
    {
        for (var seed = 1; seed <= 30; seed++)
        {
            var result = Run(TrainingLocation.Home, [], FourDays, 60, Difficulty.Hard, seed);

            Assert.All(result.SelectMany(d => d.Exercises), e => Assert.Null(e.Exercise.EquipmentId));
        }
    }

    [Fact]
    public void Gym_days_have_the_full_exercise_count_without_repeats()
    {
        foreach (var days in new[] { ThreeDays, FourDays, SixDays })
        {
            for (var seed = 1; seed <= 10; seed++)
            {
                var result = Run(TrainingLocation.Gym, [], days, 90, Difficulty.Hard, seed);
                var training = result.Where(d => d.Name is not null).ToList();

                Assert.Equal(days.Count, training.Count);
                Assert.All(training, d =>
                {
                    Assert.Equal(7, d.Exercises.Count);
                    Assert.Equal(7, d.Exercises.Select(e => e.Exercise.Id).Distinct().Count());
                });
            }
        }
    }

    [Fact]
    public void Gym_easy_difficulty_stays_easy_and_still_fills_the_days()
    {
        for (var seed = 1; seed <= 10; seed++)
        {
            var result = Run(TrainingLocation.Gym, [], FourDays, 60, Difficulty.Easy, seed);
            var training = result.Where(d => d.Name is not null).ToList();

            Assert.All(training, d => Assert.Equal(5, d.Exercises.Count));
            Assert.All(training.SelectMany(d => d.Exercises), e => Assert.Equal(Difficulty.Easy, e.Exercise.Difficulty));
        }
    }

    [Fact]
    public void Gym_split_days_include_two_exercises_for_the_named_body_area()
    {
        for (var seed = 1; seed <= 10; seed++)
        {
            var result = Run(TrainingLocation.Gym, [], SixDays, 60, Difficulty.Hard, seed);

            Assert.All(result.Where(d => d.Name == "Felsőtest"), d =>
                Assert.True(d.Exercises.Count(e => e.Exercise.MuscleGroup == MuscleGroup.UpperBody) >= 2));
            Assert.All(result.Where(d => d.Name == "Alsótest"), d =>
                Assert.True(d.Exercises.Count(e => e.Exercise.MuscleGroup == MuscleGroup.LowerBody) >= 2));
        }
    }

    [Fact]
    public void Planned_weight_comes_from_the_seed_minimum_for_equipment_exercises()
    {
        var result = Run(TrainingLocation.Gym, [], FourDays, 60, Difficulty.Hard, 1);

        Assert.All(result.SelectMany(d => d.Exercises), e =>
            Assert.Equal(e.Exercise.EquipmentId is null ? null : e.Exercise.MinWeightKg, e.WeightKg));
    }
}
