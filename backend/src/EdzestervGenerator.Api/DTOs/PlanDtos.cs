using System.ComponentModel.DataAnnotations;
using EdzestervGenerator.Api.Models;

namespace EdzestervGenerator.Api.DTOs;

public enum TrainingGoal { Strength, Hypertrophy, Endurance, General }
public enum TrainingLocation { Gym, Home }

public sealed record GeneratePlanRequest(
    [EnumDataType(typeof(TrainingGoal))] TrainingGoal Goal,
    [EnumDataType(typeof(TrainingLocation))] TrainingLocation Location,
    [Required] List<int> EquipmentIds,
    [Required] List<DayOfWeek> Days,
    [Range(45, 90)] int SessionMinutes,
    [EnumDataType(typeof(Difficulty))] Difficulty Difficulty);

public sealed record PlanExerciseDto(int Id, string Name, string MuscleGroup, int Sets, int Reps, double? WeightKg, bool IsDone);
public sealed record PlanDayDto(DayOfWeek Day, string? WorkoutName, IReadOnlyList<PlanExerciseDto> Exercises);
public sealed record WeekPlanDto(IReadOnlyList<PlanDayDto> Days);
public sealed record UpdatePlanExerciseRequest(bool IsDone);
