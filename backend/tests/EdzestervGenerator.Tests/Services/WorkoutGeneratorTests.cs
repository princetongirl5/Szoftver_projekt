using EdzestervGenerator.Api.DTOs;
using EdzestervGenerator.Api.Models;
using EdzestervGenerator.Api.Services;

namespace EdzestervGenerator.Tests.Services;

public sealed class WorkoutGeneratorTests
{
    private readonly WorkoutGenerator _generator = new();

    [Fact]
    public void Home_keeps_bodyweight_and_selected_equipment_only()
    {
        var result = Generate(TrainingLocation.Home, [1], [Ex(1, "Push-up", MuscleGroup.UpperBody), Ex(2, "Dumbbell press", MuscleGroup.UpperBody, equipment: 1), Ex(3, "Pull-up", MuscleGroup.UpperBody, equipment: 3)]);
        Assert.DoesNotContain(result.SelectMany(d => d.Exercises), e => e.Exercise.Id == 3);
        Assert.Contains(result.SelectMany(d => d.Exercises), e => e.Exercise.Id == 1);
        Assert.Contains(result.SelectMany(d => d.Exercises), e => e.Exercise.Id == 2);
    }

    [Fact]
    public void Empty_home_equipment_returns_bodyweight_only()
    {
        var result = Generate(TrainingLocation.Home, [], [Ex(1, "Push-up", MuscleGroup.UpperBody), Ex(2, "Dumbbell press", MuscleGroup.UpperBody, equipment: 1)]);
        Assert.All(result.SelectMany(d => d.Exercises), e => Assert.Null(e.Exercise.EquipmentId));
    }

    [Fact]
    public void Gym_ignores_home_equipment_filter()
    {
        var result = Generate(TrainingLocation.Gym, [], [Ex(1, "Push-up", MuscleGroup.UpperBody), Ex(2, "Press", MuscleGroup.UpperBody, equipment: 1)]);
        Assert.Contains(result.SelectMany(d => d.Exercises), e => e.Exercise.EquipmentId == 1);
    }

    [Fact]
    public void Returns_selected_days_in_monday_first_order()
    {
        var result = Generate(TrainingLocation.Home, [], Catalogue(), days: [DayOfWeek.Monday, DayOfWeek.Thursday]);
        Assert.Equal(2, result.Count);
        Assert.Equal(DayOfWeek.Monday, result[0].Day);
        Assert.Equal(DayOfWeek.Thursday, result[1].Day);
    }

    [Fact]
    public void Session_duration_controls_exercise_count()
    {
        var result = Generate(TrainingLocation.Home, [], Catalogue(), minutes: 75);
        Assert.All(result.Where(d => d.Exercises.Count > 0), d => Assert.Equal(6, d.Exercises.Count));
    }

    [Fact]
    public void Goal_sets_expected_repetition_range()
    {
        var strength = Generate(TrainingLocation.Home, [], Catalogue(), goal: TrainingGoal.Strength);
        var endurance = Generate(TrainingLocation.Home, [], Catalogue(), goal: TrainingGoal.Endurance);
        Assert.All(strength.SelectMany(d => d.Exercises), e => Assert.Equal(5, e.Reps));
        Assert.All(endurance.SelectMany(d => d.Exercises), e => Assert.Equal(18, e.Reps));
    }

    [Fact]
    public void Difficulty_excludes_exercises_above_requested_level()
    {
        var result = Generate(TrainingLocation.Home, [], [Ex(1, "Easy", MuscleGroup.UpperBody), Ex(2, "Hard", MuscleGroup.UpperBody, Difficulty.Hard)], difficulty: Difficulty.Easy);
        Assert.DoesNotContain(result.SelectMany(d => d.Exercises), e => e.Exercise.Difficulty == Difficulty.Hard);
    }

    [Fact]
    public void Picks_staple_pushup_and_never_repeats_inside_a_workout()
    {
        var exercises = Catalogue().Where(e => e.Name != "Push-up").ToList();
        exercises.Add(Ex(100, "Fekvőtámasz", MuscleGroup.UpperBody));
        var result = Generate(TrainingLocation.Home, [], exercises, days: [DayOfWeek.Monday, DayOfWeek.Thursday]);
        Assert.Contains(result[0].Exercises, e => e.Exercise.Id == 100);
        Assert.All(result.Where(d => d.Exercises.Count > 0), d => Assert.Equal(d.Exercises.Count, d.Exercises.Select(e => e.Exercise.Id).Distinct().Count()));
    }

    [Fact]
    public void Duplicate_days_are_rejected()
    {
        var request = Request(days: [DayOfWeek.Monday, DayOfWeek.Monday]);
        Assert.Throws<ArgumentException>(() => _generator.GenerateWeek(request, Catalogue(), [], new Random(1)));
    }

    private IReadOnlyList<GeneratedWorkoutDay> Generate(TrainingLocation location, IReadOnlyList<int> equipment,
        IReadOnlyList<Exercise> exercises, TrainingGoal goal = TrainingGoal.General,
        Difficulty difficulty = Difficulty.Hard, int minutes = 60,
        List<DayOfWeek>? days = null) => _generator.GenerateWeek(
            Request(goal, location, equipment.ToList(), days ?? [DayOfWeek.Monday, DayOfWeek.Thursday], minutes, difficulty),
            exercises, equipment, new Random(7));

    private static GeneratePlanRequest Request(TrainingGoal goal = TrainingGoal.General,
        TrainingLocation location = TrainingLocation.Home, List<int>? equipment = null,
        List<DayOfWeek>? days = null, int minutes = 60, Difficulty difficulty = Difficulty.Hard) =>
        new(goal, location, equipment ?? [], days ?? [DayOfWeek.Monday, DayOfWeek.Thursday], minutes, difficulty);

    private static List<Exercise> Catalogue() =>
    [
        Ex(1, "Push-up", MuscleGroup.UpperBody), Ex(2, "Row", MuscleGroup.UpperBody),
        Ex(3, "Shoulder press", MuscleGroup.UpperBody), Ex(4, "Squat", MuscleGroup.LowerBody),
        Ex(5, "Lunge", MuscleGroup.LowerBody), Ex(6, "Bridge", MuscleGroup.LowerBody),
        Ex(7, "Plank", MuscleGroup.Core), Ex(8, "Dead bug", MuscleGroup.Core),
        Ex(9, "Burpee", MuscleGroup.FullBody), Ex(10, "Mobility", MuscleGroup.Mobility)
    ];

    private static Exercise Ex(int id, string name, MuscleGroup group, Difficulty difficulty = Difficulty.Easy, int? equipment = null) => new()
    {
        Id = id, Name = name, MuscleGroup = group, Difficulty = difficulty, EquipmentId = equipment
    };
}
