using EdzestervGenerator.Api.Data;
using EdzestervGenerator.Api.DTOs;
using EdzestervGenerator.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EdzestervGenerator.Api.Controllers;

// Calendar entries of a user.
[ApiController]
[Route("api")]
public class WorkoutsController(AppDbContext db) : ControllerBase
{
    // from and to are optional, to is exclusive: October = ?from=2026-10-01&to=2026-11-01
    [HttpGet("users/{userId}/workouts")]
    public async Task<ActionResult<List<WorkoutDto>>> GetAll(int userId, DateTime? from, DateTime? to)
    {
        if (!await db.Users.AnyAsync(u => u.Id == userId))
        {
            return NotFound();
        }

        var query = db.Workouts.Where(w => w.UserId == userId);
        if (from != null)
        {
            var fromUtc = ToUtc(from.Value);
            query = query.Where(w => w.Date >= fromUtc);
        }
        if (to != null)
        {
            var toUtc = ToUtc(to.Value);
            query = query.Where(w => w.Date < toUtc);
        }

        var workouts = await query.OrderBy(w => w.Date).ToListAsync();
        return workouts.Select(ToDto).ToList();
    }

    [HttpPost("users/{userId}/workouts")]
    public async Task<ActionResult<WorkoutDto>> Create(int userId, CreateWorkoutRequest request)
    {
        if (!await db.Users.AnyAsync(u => u.Id == userId))
        {
            return NotFound();
        }

        // Generated workouts are created by the generator, not from the calendar.
        if (request.Activity == Activity.Generated)
        {
            ModelState.AddModelError("activity", "Generated workouts cannot be added from the calendar.");
            return ValidationProblem();
        }

        var workout = new Workout
        {
            UserId = userId,
            Date = ToUtc(request.Date),
            Activity = request.Activity,
            DurationMinutes = request.DurationMinutes,
            Intensity = request.Intensity,
            Note = request.Note
        };

        db.Workouts.Add(workout);
        await db.SaveChangesAsync();

        return StatusCode(StatusCodes.Status201Created, ToDto(workout));
    }

    [HttpDelete("workouts/{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var workout = await db.Workouts.FindAsync(id);
        if (workout == null)
        {
            return NotFound();
        }

        db.Workouts.Remove(workout);
        await db.SaveChangesAsync();

        return NoContent();
    }

    // Dates without a time zone are treated as UTC, others are converted to UTC.
    private static DateTime ToUtc(DateTime date) =>
        date.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(date, DateTimeKind.Utc)
            : date.ToUniversalTime();

    private static WorkoutDto ToDto(Workout workout) =>
        new(workout.Id,
            DateTime.SpecifyKind(workout.Date, DateTimeKind.Utc),
            workout.Activity,
            workout.DurationMinutes,
            workout.Intensity,
            workout.Note);
}
