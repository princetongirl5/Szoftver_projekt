namespace EdzestervGenerator.Api.DTOs;

public enum DayStatus { Done, Partial, Missed, Rest, Planned }

public record StatsWeekDto(DateOnly WeekStart, int Completed, int Planned, IReadOnlyList<DayStatus> Days);

public record StatsDto(
    DateOnly From,
    DateOnly To,
    int Weeks,
    int CurrentStreakWeeks,
    int LongestStreakWeeks,
    int CompletedWorkouts,
    int PlannedWorkouts,
    int CompletionPercent,
    IReadOnlyList<StatsWeekDto> Log);
