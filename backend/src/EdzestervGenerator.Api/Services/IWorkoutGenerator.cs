using EdzestervGenerator.Api.DTOs;
using EdzestervGenerator.Api.Models;

namespace EdzestervGenerator.Api.Services;

public interface IWorkoutGenerator
{
    IReadOnlyList<GeneratedWorkoutDay> GenerateWeek(
        GeneratePlanRequest request,
        IReadOnlyList<Exercise> exercises,
        IReadOnlyList<int> equipmentIds,
        Random random);
}

public sealed record PlannedExercise(Exercise Exercise, int Sets, int Reps, double? WeightKg);
public sealed record GeneratedWorkoutDay(DayOfWeek Day, string? Name, IReadOnlyList<PlannedExercise> Exercises);
