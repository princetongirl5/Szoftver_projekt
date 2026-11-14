using EdzestervGenerator.Api.Data;
using EdzestervGenerator.Api.DTOs;
using EdzestervGenerator.Api.Models;
using EdzestervGenerator.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EdzestervGenerator.Api.Controllers;

[ApiController, Authorize, Route("api")]
public sealed class WorkoutsController(AppDbContext db) : ControllerBase
{
    [HttpGet("users/{userId:int}/workouts")]
    public async Task<ActionResult<List<WorkoutDto>>> GetAll(int userId, DateTime? from, DateTime? to, CancellationToken cancellationToken)
    {
        var current = RequestUser.Id(User);
        if (current is null) return Unauthorized();
        if (current != userId) return Forbid();
        var query = db.Workouts.Where(w => w.UserId == current);
        if (from.HasValue) query = query.Where(w => w.Date >= ToUtc(from.Value));
        if (to.HasValue) query = query.Where(w => w.Date < ToUtc(to.Value));
        var workouts = await query.OrderBy(w => w.Date).ToListAsync(cancellationToken);
        return workouts.Select(ToDto).ToList();
    }

    [HttpPost("users/{userId:int}/workouts")]
    public async Task<ActionResult<WorkoutDto>> Create(int userId, CreateWorkoutRequest request, CancellationToken cancellationToken)
    {
        var current = RequestUser.Id(User);
        if (current is null) return Unauthorized();
        if (current != userId) return Forbid();
        if (request.Activity == Activity.Generated) { ModelState.AddModelError("activity", "Generated workouts cannot be added from the calendar."); return ValidationProblem(); }
        var workout = new Workout { UserId = current.Value, Date = ToUtc(request.Date), Activity = request.Activity,
            DurationMinutes = request.DurationMinutes, Intensity = request.Intensity, Note = request.Note };
        db.Workouts.Add(workout);
        await db.SaveChangesAsync(cancellationToken);
        return StatusCode(StatusCodes.Status201Created, ToDto(workout));
    }

    [HttpDelete("workouts/{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var current = RequestUser.Id(User);
        if (current is null) return Unauthorized();
        var workout = await db.Workouts.FirstOrDefaultAsync(w => w.Id == id && w.UserId == current && w.Activity != Activity.Generated, cancellationToken);
        if (workout is null) return NotFound();
        db.Workouts.Remove(workout);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private static DateTime ToUtc(DateTime date) => date.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(date, DateTimeKind.Utc) : date.ToUniversalTime();
    private static WorkoutDto ToDto(Workout workout) => new(workout.Id, DateTime.SpecifyKind(workout.Date, DateTimeKind.Utc), workout.Activity, workout.DurationMinutes, workout.Intensity, workout.Note);
}
