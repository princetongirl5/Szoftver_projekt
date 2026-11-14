using System.Net;
using System.Net.Http.Json;
using EdzestervGenerator.Api.DTOs;
using EdzestervGenerator.Api.Models;

namespace EdzestervGenerator.Tests.Api;

[Collection("Api")]
public sealed class WorkoutsApiTests(ApiFactory factory)
{
    [Fact]
    public async Task Create_returns_201_and_date_in_utc()
    {
        var (client, userId) = await factory.CreateAuthenticatedClient($"workout-{Guid.NewGuid():N}@example.com");
        var response = await client.PostAsJsonAsync($"/api/users/{userId}/workouts", new { date = "2026-10-22T16:00:00Z", activity = "Football", durationMinutes = 90, intensity = "High", note = "Focimeccs" });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var workout = await response.Content.ReadFromJsonAsync<WorkoutDto>(ApiFactory.Json);
        Assert.Equal(new DateTime(2026, 10, 22, 16, 0, 0, DateTimeKind.Utc), workout!.Date);
        Assert.Equal(Activity.Football, workout.Activity);
    }

    [Fact]
    public async Task User_cannot_read_or_write_another_users_workouts()
    {
        var (a, idA) = await factory.CreateAuthenticatedClient($"a-{Guid.NewGuid():N}@example.com");
        var (_, idB) = await factory.CreateAuthenticatedClient($"b-{Guid.NewGuid():N}@example.com");
        Assert.Equal(HttpStatusCode.Forbidden, (await a.GetAsync($"/api/users/{idB}/workouts")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await a.PostAsJsonAsync($"/api/users/{idB}/workouts", new { date = "2026-10-22T16:00:00Z", activity = "Running", durationMinutes = 30, intensity = "Medium" })).StatusCode);
    }

    [Fact]
    public async Task Generated_activity_cannot_be_created_from_calendar()
    {
        var (client, userId) = await factory.CreateAuthenticatedClient($"generated-{Guid.NewGuid():N}@example.com");
        var response = await client.PostAsJsonAsync($"/api/users/{userId}/workouts", new { date = "2026-10-22T16:00:00Z", activity = "Generated", durationMinutes = 30, intensity = "Low" });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
