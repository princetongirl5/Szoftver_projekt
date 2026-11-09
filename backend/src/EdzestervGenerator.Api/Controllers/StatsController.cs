using EdzestervGenerator.Api.Data;
using EdzestervGenerator.Api.DTOs;
using EdzestervGenerator.Api.Models;
using EdzestervGenerator.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EdzestervGenerator.Api.Controllers;

[ApiController]
[Route("api/stats")]
public class StatsController(AppDbContext db) : ControllerBase
{
    // Same default user as the plan API.
    private const int DefaultUserId = 1;

    // weeks: 4 or 12 on the screen, empty = all weeks
    [HttpGet]
    public async Task<ActionResult<StatsDto>> Get(int? weeks)
    {
        if (weeks is < 1 or > 104)
        {
            ModelState.AddModelError("weeks", "weeks must be between 1 and 104.");
            return ValidationProblem();
        }

        var workouts = await db.Workouts
            .Where(w => w.UserId == DefaultUserId && w.Activity == Activity.Generated)
            .Select(w => new
            {
                w.Date,
                Count = w.Exercises.Count,
                Done = w.Exercises.Count(e => e.IsDone)
            })
            .ToListAsync();

        var zone = BudapestTimeZone();
        var days = workouts
            .Select(w => new WorkoutDay(ToLocalDate(w.Date, zone), w.Count, w.Done))
            .ToList();
        var today = ToLocalDate(DateTime.UtcNow, zone);

        return StatsCalculator.Calculate(days, today, weeks);
    }

    private static DateOnly ToLocalDate(DateTime utc, TimeZoneInfo zone) =>
        DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), zone));

    private static TimeZoneInfo BudapestTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Europe/Budapest");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time");
        }
    }
}
