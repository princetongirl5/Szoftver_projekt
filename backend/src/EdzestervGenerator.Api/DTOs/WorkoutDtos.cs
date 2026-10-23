using System.ComponentModel.DataAnnotations;
using EdzestervGenerator.Api.Models;

namespace EdzestervGenerator.Api.DTOs;

public record WorkoutDto(int Id, DateTime Date, Activity Activity, int DurationMinutes, Intensity Intensity, string? Note);

// An external activity added in the calendar, e.g. football for 90 minutes.
public record CreateWorkoutRequest(
    DateTime Date,
    [EnumDataType(typeof(Activity))] Activity Activity,
    [Range(1, 600)] int DurationMinutes,
    [EnumDataType(typeof(Intensity))] Intensity Intensity,
    [MaxLength(200)] string? Note);
