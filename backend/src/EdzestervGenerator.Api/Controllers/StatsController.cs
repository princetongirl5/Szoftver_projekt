using EdzestervGenerator.Api.Data;
using EdzestervGenerator.Api.DTOs;
using EdzestervGenerator.Api.Models;
using EdzestervGenerator.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EdzestervGenerator.Api.Controllers;

[ApiController, Authorize, Route("api/stats")]
public sealed class StatsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<StatsDto>> Get(int? weeks, CancellationToken cancellationToken)
    {
        if (weeks is < 1 or > 104) { ModelState.AddModelError("weeks", LocalizationText.Validation("A weeks értéke 1 és 104 között lehet.", "weeks must be between 1 and 104.")); return ValidationProblem(); }
        var userId = RequestUser.Id(User);
        if (userId is null) return Unauthorized();
        var workouts = await db.Workouts.Where(w => w.UserId == userId && w.Activity == Activity.Generated)
            .Select(w => new { w.Date, Count = w.Exercises.Count, Done = w.Exercises.Count(e => e.IsDone) }).ToListAsync(cancellationToken);
        var zone = BudapestTimeZone();
        var days = workouts.Select(w => new WorkoutDay(ToLocalDate(w.Date, zone), w.Count, w.Done)).ToList();
        return StatsCalculator.Calculate(days, ToLocalDate(DateTime.UtcNow, zone), weeks);
    }

    [HttpGet("exercises")]
    public async Task<ActionResult<IReadOnlyList<ExerciseStatsDto>>> GetExercises(CancellationToken cancellationToken)
    {
        var userId = RequestUser.Id(User);
        if (userId is null) return Unauthorized();
        var rows = await db.WorkoutExercises.AsNoTracking()
            .Where(e => e.Workout.UserId == userId && e.Workout.Activity == Activity.Generated)
            .GroupBy(e => new { e.ExerciseId, e.Exercise.Name, e.Exercise.NameEn })
            .Select(g => new { g.Key.ExerciseId, g.Key.Name, g.Key.NameEn, Planned = g.Count(), Done = g.Count(x => x.IsDone) })
            .OrderBy(x => x.Name).ToListAsync(cancellationToken);
        return Ok(rows.Select(x => new ExerciseStatsDto(x.ExerciseId, LocalizationText.ExerciseName(x.Name, x.NameEn), x.Planned, x.Done)).ToArray());
    }

    [HttpGet("exercises/{exerciseId:int}")]
    public async Task<ActionResult<IReadOnlyList<ExerciseHistoryDto>>> GetExerciseHistory(int exerciseId, CancellationToken cancellationToken)
    {
        var userId = RequestUser.Id(User);
        if (userId is null) return Unauthorized();
        var exists = await db.WorkoutExercises.AnyAsync(e => e.ExerciseId == exerciseId && e.Workout.UserId == userId && e.Workout.Activity == Activity.Generated, cancellationToken);
        if (!exists) return NotFound();
        var rows = await db.WorkoutExercises.AsNoTracking()
            .Where(e => e.ExerciseId == exerciseId && e.Workout.UserId == userId && e.Workout.Activity == Activity.Generated)
            .OrderBy(e => e.Workout.Date).ThenBy(e => e.Order)
            .Select(e => new { e.Workout.Date, e.Sets, e.Reps, e.WeightKg, e.ActualReps, e.ActualWeightKg, e.IsDone })
            .ToListAsync(cancellationToken);
        return Ok(rows.Select(e => new ExerciseHistoryDto(ToBudapestDate(e.Date), e.Sets, e.Reps, e.WeightKg, e.ActualReps, e.ActualWeightKg, e.IsDone)).ToArray());
    }

    private static DateOnly ToLocalDate(DateTime utc, TimeZoneInfo zone) => DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), zone));
    private static DateTime ToBudapestDate(DateTime utc) => DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), BudapestTimeZone())).ToDateTime(TimeOnly.MinValue);
    private static TimeZoneInfo BudapestTimeZone()
    {
        try { return TimeZoneInfo.FindSystemTimeZoneById("Europe/Budapest"); }
        catch (TimeZoneNotFoundException) { return TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time"); }
    }
}
