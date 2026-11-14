using EdzestervGenerator.Api.Data;
using EdzestervGenerator.Api.DTOs;
using EdzestervGenerator.Api.Models;
using EdzestervGenerator.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EdzestervGenerator.Api.Controllers;

[ApiController, Authorize, Route("api/plans")]
public sealed class PlansController(AppDbContext db, IWorkoutGenerator generator) : ControllerBase
{
    [HttpPost("generate")]
    public async Task<ActionResult<WeekPlanDto>> Generate(GeneratePlanRequest request, CancellationToken cancellationToken)
    {
        var userId = RequestUser.Id(User);
        if (userId is null) return Unauthorized();
        if (request.EquipmentIds is null || request.EquipmentIds.Any(id => id <= 0))
            return ValidationProblem("equipmentIds", LocalizationText.Validation("Érvénytelen eszközlista.", "Invalid equipment list."));
        if (request.Days is null || request.Days.Count is < 2 or > 6 || request.Days.Distinct().Count() != request.Days.Count || request.Days.Any(day => !Enum.IsDefined(day)))
            return ValidationProblem("days", LocalizationText.Validation("2 és 6 közötti, különböző napot válassz.", "Choose between 2 and 6 distinct days."));
        if (request.SessionMinutes is not (45 or 60 or 75 or 90))
            return ValidationProblem("sessionMinutes", LocalizationText.Validation("Az edzés hossza 45, 60, 75 vagy 90 perc lehet.", "Session duration must be 45, 60, 75 or 90 minutes."));
        if (!Enum.IsDefined(request.Goal) || !Enum.IsDefined(request.Location) || !Enum.IsDefined(request.Difficulty))
            return ValidationProblem("request", LocalizationText.Validation("A kérés érvénytelen értéket tartalmaz.", "The request contains an invalid value."));

        var requestedEquipment = request.EquipmentIds.Distinct().ToArray();
        var equipment = await db.Equipment.Where(e => requestedEquipment.Contains(e.Id)).ToListAsync(cancellationToken);
        if (equipment.Count != requestedEquipment.Length)
            return ValidationProblem("equipmentIds", LocalizationText.Validation("Ismeretlen eszközazonosító.", "Unknown equipment ID."));
        if (request.Location == TrainingLocation.Home && equipment.Any(e => e.Location != EquipmentLocation.Home))
            return ValidationProblem("equipmentIds", LocalizationText.Validation("Otthoni tervhez csak otthoni eszköz választható.", "Only home equipment can be selected for a home plan."));

        var exercises = await db.Exercises.AsNoTracking().ToListAsync(cancellationToken);
        IReadOnlyList<GeneratedWorkoutDay> generated;
        try { generated = generator.GenerateWeek(request, exercises, requestedEquipment, Random.Shared); }
        catch (ArgumentException ex) { return ValidationProblem("request", ex.Message); }
        if (generated.All(d => d.Exercises.Count == 0))
            return ValidationProblem("exercises", LocalizationText.Validation("A megadott feltételekkel nem található gyakorlat.", "No exercises match the selected criteria."));

        var (weekStartUtc, weekEndUtc, zone) = CurrentWeekUtc(DateTime.UtcNow);
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var oldPlans = await db.Workouts.Where(w => w.UserId == userId && w.Activity == Activity.Generated && w.Date >= weekStartUtc && w.Date < weekEndUtc).ToListAsync(cancellationToken);
        db.Workouts.RemoveRange(oldPlans);

        foreach (var day in generated.Where(day => day.Name is not null))
        {
            var localDate = DateOnly.FromDateTime(LocalDateForDay(weekStartUtc, day.Day, zone));
            var dateUtc = TimeZoneInfo.ConvertTimeToUtc(localDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified), zone);
            var workout = new Workout { UserId = userId.Value, Date = dateUtc, Activity = Activity.Generated,
                DurationMinutes = request.SessionMinutes,
                Intensity = request.Difficulty switch { Difficulty.Easy => Intensity.Low, Difficulty.Medium => Intensity.Medium, _ => Intensity.High } };
            foreach (var (item, index) in day.Exercises.Select((e, i) => (e, i)))
                workout.Exercises.Add(new WorkoutExercise { ExerciseId = item.Exercise.Id, Order = index + 1, Sets = item.Sets, Reps = item.Reps, WeightKg = item.WeightKg, IsDone = false });
            db.Workouts.Add(workout);
        }

        var preference = await db.PlanPreferences.SingleOrDefaultAsync(p => p.UserId == userId, cancellationToken);
        if (preference is null)
        {
            preference = new PlanPreference { UserId = userId.Value };
            db.PlanPreferences.Add(preference);
        }
        preference.Goal = request.Goal;
        preference.Location = request.Location;
        preference.EquipmentIds = requestedEquipment.ToList();
        preference.Days = request.Days.Distinct().ToList();
        preference.SessionMinutes = request.SessionMinutes;
        preference.Difficulty = request.Difficulty;
        preference.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return CreatedAtAction(nameof(GetCurrent), null, await ReadCurrentPlan(userId.Value, cancellationToken));
    }

    [HttpGet("current")]
    public async Task<ActionResult<WeekPlanDto>> GetCurrent(CancellationToken cancellationToken)
    {
        var userId = RequestUser.Id(User);
        if (userId is null) return Unauthorized();
        var plan = await ReadCurrentPlan(userId.Value, cancellationToken);
        if (plan.Days.All(d => d.Exercises.Count == 0)) return NotFound();
        return Ok(plan);
    }

    [HttpGet("preferences")]
    public async Task<ActionResult<GeneratePlanRequest>> GetPreferences(CancellationToken cancellationToken)
    {
        var userId = RequestUser.Id(User);
        if (userId is null) return Unauthorized();
        var p = await db.PlanPreferences.AsNoTracking().SingleOrDefaultAsync(x => x.UserId == userId, cancellationToken);
        return p is null ? NotFound() : Ok(new GeneratePlanRequest(p.Goal, p.Location, p.EquipmentIds, p.Days, p.SessionMinutes, p.Difficulty));
    }

    [HttpPatch("exercises/{id:int}")]
    public async Task<IActionResult> MarkExercise(int id, UpdatePlanExerciseRequest request, CancellationToken cancellationToken)
    {
        var userId = RequestUser.Id(User);
        if (userId is null) return Unauthorized();
        if (request.ActualReps is < 0 or > 1000 || request.ActualWeightKg is < 0 or > 2000)
            return ValidationProblem("actualValues", LocalizationText.Validation("A tényleges ismétlés vagy súly érvénytelen.", "Actual reps or weight is out of range."));
        var item = await db.WorkoutExercises.Include(e => e.Workout)
            .FirstOrDefaultAsync(e => e.Id == id && e.Workout.UserId == userId && e.Workout.Activity == Activity.Generated, cancellationToken);
        if (item is null) return NotFound();
        item.IsDone = request.IsDone;
        if (request.ActualReps.HasValue) item.ActualReps = request.ActualReps;
        if (request.ActualWeightKg.HasValue) item.ActualWeightKg = request.ActualWeightKg;
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private async Task<WeekPlanDto> ReadCurrentPlan(int userId, CancellationToken cancellationToken)
    {
        var (fromUtc, toUtc, _) = CurrentWeekUtc(DateTime.UtcNow);
        var workouts = await db.Workouts.Where(w => w.UserId == userId && w.Activity == Activity.Generated && w.Date >= fromUtc && w.Date < toUtc)
            .Include(w => w.Exercises).ThenInclude(e => e.Exercise).OrderBy(w => w.Date).ToListAsync(cancellationToken);
        var byDay = workouts.ToDictionary(w => ToBudapestDay(w.Date));
        var days = Enum.GetValues<DayOfWeek>().OrderBy(WeekOrder).Select(day =>
        {
            if (!byDay.TryGetValue(day, out var workout)) return new PlanDayDto(day, null, []);
            var items = workout.Exercises.OrderBy(e => e.Order).Select(e => new PlanExerciseDto(e.Id,
                LocalizationText.ExerciseName(e.Exercise.Name, e.Exercise.NameEn), e.Exercise.MuscleGroup.ToString(), e.Sets, e.Reps, e.WeightKg, e.IsDone, e.ActualReps, e.ActualWeightKg)).ToArray();
            return new PlanDayDto(day, LocalizedWorkoutName(workout, items), items);
        }).ToArray();
        return new WeekPlanDto(days);
    }

    private static string LocalizedWorkoutName(Workout workout, IReadOnlyList<PlanExerciseDto> exercises)
    {
        var groups = exercises.Select(e => e.MuscleGroup).Distinct().ToArray();
        var hu = groups.Contains(nameof(MuscleGroup.FullBody)) || (groups.Contains(nameof(MuscleGroup.UpperBody)) && groups.Contains(nameof(MuscleGroup.LowerBody))) ? "Teljes test" :
            groups.Contains(nameof(MuscleGroup.UpperBody)) ? "Felsőtest" : groups.Contains(nameof(MuscleGroup.LowerBody)) ? "Alsótest" : "Edzés";
        return LocalizationText.WorkoutName(hu);
    }
    private static (DateTime StartUtc, DateTime EndUtc, TimeZoneInfo Zone) CurrentWeekUtc(DateTime nowUtc)
    {
        var zone = BudapestTimeZone();
        var localNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(nowUtc, DateTimeKind.Utc), zone);
        var monday = DateOnly.FromDateTime(localNow).AddDays(-WeekOrder(localNow.DayOfWeek));
        return (TimeZoneInfo.ConvertTimeToUtc(monday.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified), zone),
            TimeZoneInfo.ConvertTimeToUtc(monday.AddDays(7).ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified), zone), zone);
    }
    private static DateTime LocalDateForDay(DateTime weekStartUtc, DayOfWeek day, TimeZoneInfo zone) => TimeZoneInfo.ConvertTimeFromUtc(weekStartUtc, zone).Date.AddDays(WeekOrder(day));
    private static DayOfWeek ToBudapestDay(DateTime utc) => TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), BudapestTimeZone()).DayOfWeek;
    private static TimeZoneInfo BudapestTimeZone()
    {
        try { return TimeZoneInfo.FindSystemTimeZoneById("Europe/Budapest"); }
        catch (TimeZoneNotFoundException) { return TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time"); }
    }
    private static int WeekOrder(DayOfWeek day) => ((int)day + 6) % 7;
    private ActionResult ValidationProblem(string key, string message) { ModelState.AddModelError(key, message); return ValidationProblem(ModelState); }
}
