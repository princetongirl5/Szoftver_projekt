using EdzestervGenerator.Api.Data;
using EdzestervGenerator.Api.DTOs;
using EdzestervGenerator.Api.Models;
using EdzestervGenerator.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EdzestervGenerator.Api.Controllers;

[ApiController]
[Route("api/plans")]
public sealed class PlansController(AppDbContext db, IWorkoutGenerator generator) : ControllerBase
{
    private const int DefaultUserId = 1;

    [HttpPost("generate")]
    public async Task<ActionResult<WeekPlanDto>> Generate(GeneratePlanRequest request)
    {
        if (request.EquipmentIds is null)
        {
            ModelState.AddModelError("equipmentIds", "Az equipmentIds mező kötelező.");
            return ValidationProblem();
        }
        if (request.Days is null || request.Days.Count is < 2 or > 6
            || request.Days.Distinct().Count() != request.Days.Count
            || request.Days.Any(day => !Enum.IsDefined(day)))
        {
            ModelState.AddModelError("days", "2 és 6 közötti, különböző napot válassz.");
            return ValidationProblem();
        }
        if (request.SessionMinutes is not (45 or 60 or 75 or 90))
        {
            ModelState.AddModelError("sessionMinutes", "Az edzés hossza 45, 60, 75 vagy 90 perc lehet.");
            return ValidationProblem();
        }

        var requestedEquipment = request.EquipmentIds.Distinct().ToArray();
        if (request.Location == TrainingLocation.Home)
        {
            var knownIds = await db.Equipment.Where(e => requestedEquipment.Contains(e.Id)).Select(e => e.Id).ToListAsync();
            var unknownIds = requestedEquipment.Except(knownIds).ToArray();
            if (unknownIds.Length > 0)
            {
                ModelState.AddModelError("equipmentIds", $"Ismeretlen eszközazonosító: {unknownIds[0]}.");
                return ValidationProblem();
            }
        }

        var exercises = await db.Exercises.AsNoTracking().ToListAsync();
        var generated = generator.GenerateWeek(request, exercises, requestedEquipment, Random.Shared);
        if (generated.All(d => d.Exercises.Count == 0))
        {
            ModelState.AddModelError("exercises", "A megadott feltételekkel nem található gyakorlat. Válassz több eszközt vagy enyhébb nehézséget.");
            return ValidationProblem();
        }

        var (weekStartUtc, weekEndUtc, timeZone) = CurrentWeekUtc(DateTime.UtcNow);
        var oldPlans = await db.Workouts
            .Where(w => w.UserId == DefaultUserId && w.Activity == Activity.Generated && w.Date >= weekStartUtc && w.Date < weekEndUtc)
            .ToListAsync();
        db.Workouts.RemoveRange(oldPlans);

        foreach (var day in generated.Where(day => day.Name is not null))
        {
            var localDate = DateOnly.FromDateTime(LocalDateForDay(weekStartUtc, day.Day, timeZone));
            var localMidnight = localDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified);
            var dateUtc = TimeZoneInfo.ConvertTimeToUtc(localMidnight, timeZone);
            var workout = new Workout
            {
                UserId = DefaultUserId,
                Date = dateUtc,
                Activity = Activity.Generated,
                DurationMinutes = request.SessionMinutes,
                Intensity = request.Difficulty switch { Difficulty.Easy => Intensity.Low, Difficulty.Medium => Intensity.Medium, _ => Intensity.High }
            };
            foreach (var (item, index) in day.Exercises.Select((e, i) => (e, i)))
            {
                workout.Exercises.Add(new WorkoutExercise
                {
                    ExerciseId = item.Exercise.Id,
                    Order = index + 1,
                    Sets = item.Sets,
                    Reps = item.Reps,
                    WeightKg = item.WeightKg,
                    IsDone = false
                });
            }
            db.Workouts.Add(workout);
        }

        if (!await db.Users.AnyAsync(u => u.Id == DefaultUserId))
        {
            db.Users.Add(new User { Id = DefaultUserId, Name = "Alapértelmezett felhasználó", Email = "demo@local.invalid", CreatedAt = DateTime.UtcNow });
        }

        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetCurrent), null, await ReadCurrentPlan());
    }

    [HttpGet("current")]
    public async Task<ActionResult<WeekPlanDto>> GetCurrent()
    {
        var (fromUtc, toUtc, _) = CurrentWeekUtc(DateTime.UtcNow);
        var workouts = await db.Workouts
            .Where(w => w.UserId == DefaultUserId && w.Activity == Activity.Generated && w.Date >= fromUtc && w.Date < toUtc)
            .Include(w => w.Exercises).ThenInclude(e => e.Exercise)
            .OrderBy(w => w.Date).ToListAsync();
        if (workouts.Count == 0) return NotFound();

        var byDay = workouts.ToDictionary(w => ToBudapestDay(w.Date));
        var result = Enum.GetValues<DayOfWeek>().OrderBy(WeekOrder).Select(day =>
        {
            if (!byDay.TryGetValue(day, out var workout)) return new PlanDayDto(day, null, []);
            var exercises = workout.Exercises.OrderBy(e => e.Order)
                .Select(e => new PlanExerciseDto(e.Id, e.Exercise.Name, e.Exercise.MuscleGroup.ToString(), e.Sets, e.Reps, e.WeightKg, e.IsDone)).ToArray();
            var name = NameFrom(workout, exercises);
            return new PlanDayDto(day, name, exercises);
        }).ToArray();
        return Ok(new WeekPlanDto(result));
    }

    [HttpPatch("exercises/{id:int}")]
    public async Task<IActionResult> MarkExercise(int id, UpdatePlanExerciseRequest request)
    {
        var item = await db.WorkoutExercises
            .Include(e => e.Workout)
            .FirstOrDefaultAsync(e => e.Id == id && e.Workout.UserId == DefaultUserId && e.Workout.Activity == Activity.Generated);
        if (item is null) return NotFound();
        item.IsDone = request.IsDone;
        await db.SaveChangesAsync();
        return NoContent();
    }

    private async Task<WeekPlanDto> ReadCurrentPlan()
    {
        var (fromUtc, toUtc, _) = CurrentWeekUtc(DateTime.UtcNow);
        var workouts = await db.Workouts
            .Where(w => w.UserId == DefaultUserId && w.Activity == Activity.Generated && w.Date >= fromUtc && w.Date < toUtc)
            .Include(w => w.Exercises).ThenInclude(e => e.Exercise)
            .OrderBy(w => w.Date).ToListAsync();
        var byDay = workouts.ToDictionary(w => ToBudapestDay(w.Date));
        var days = Enum.GetValues<DayOfWeek>().OrderBy(WeekOrder).Select(day =>
        {
            if (!byDay.TryGetValue(day, out var workout)) return new PlanDayDto(day, null, []);
            var items = workout.Exercises.OrderBy(e => e.Order).Select(e => new PlanExerciseDto(e.Id, e.Exercise.Name,
                e.Exercise.MuscleGroup.ToString(), e.Sets, e.Reps, e.WeightKg, e.IsDone)).ToArray();
            return new PlanDayDto(day, NameFrom(workout, items), items);
        }).ToArray();
        return new WeekPlanDto(days);
    }

    private static string NameFrom(Workout workout, IReadOnlyList<PlanExerciseDto> exercises)
    {
        var groups = exercises.Select(e => e.MuscleGroup).Distinct().ToArray();
        if (groups.Contains(nameof(MuscleGroup.FullBody)) || (groups.Contains(nameof(MuscleGroup.UpperBody)) && groups.Contains(nameof(MuscleGroup.LowerBody)))) return "Teljes test";
        if (groups.Contains(nameof(MuscleGroup.UpperBody))) return "Felsőtest";
        if (groups.Contains(nameof(MuscleGroup.LowerBody))) return "Alsótest";
        return "Edzés";
    }

    private static (DateTime StartUtc, DateTime EndUtc, TimeZoneInfo Zone) CurrentWeekUtc(DateTime nowUtc)
    {
        TimeZoneInfo zone;
        try { zone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Budapest"); }
        catch (TimeZoneNotFoundException) { zone = TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time"); }
        var localNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(nowUtc, DateTimeKind.Utc), zone);
        var monday = DateOnly.FromDateTime(localNow).AddDays(-WeekOrder(localNow.DayOfWeek));
        var startLocal = monday.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified);
        var endLocal = monday.AddDays(7).ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified);
        return (TimeZoneInfo.ConvertTimeToUtc(startLocal, zone), TimeZoneInfo.ConvertTimeToUtc(endLocal, zone), zone);
    }

    private static DateTime LocalDateForDay(DateTime weekStartUtc, DayOfWeek day, TimeZoneInfo zone)
    {
        var mondayLocal = TimeZoneInfo.ConvertTimeFromUtc(weekStartUtc, zone).Date;
        return mondayLocal.AddDays(WeekOrder(day));
    }

    private static DayOfWeek ToBudapestDay(DateTime utc)
    {
        TimeZoneInfo zone;
        try { zone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Budapest"); }
        catch (TimeZoneNotFoundException) { zone = TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time"); }
        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), zone).DayOfWeek;
    }
    private static int WeekOrder(DayOfWeek day) => ((int)day + 6) % 7;
}
