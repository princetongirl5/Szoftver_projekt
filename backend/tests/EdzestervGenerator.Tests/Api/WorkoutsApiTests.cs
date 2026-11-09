using System.Net;
using System.Net.Http.Json;
using EdzestervGenerator.Api.DTOs;
using EdzestervGenerator.Api.Models;

namespace EdzestervGenerator.Tests.Api;

[Collection("Api")]
public sealed class WorkoutsApiTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Create_returns_201_and_the_date_in_utc()
    {
        var userId = await CreateUser();

        var response = await _client.PostAsJsonAsync($"/api/users/{userId}/workouts", new
        {
            date = "2026-10-22T16:00:00Z",
            activity = "Football",
            durationMinutes = 90,
            intensity = "High",
            note = "Focimeccs"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var workout = await response.Content.ReadFromJsonAsync<WorkoutDto>(ApiFactory.Json);
        Assert.Equal(new DateTime(2026, 10, 22, 16, 0, 0, DateTimeKind.Utc), workout!.Date);
        Assert.Equal(DateTimeKind.Utc, workout.Date.Kind);
        Assert.Equal(Activity.Football, workout.Activity);
        Assert.Equal("Focimeccs", workout.Note);
    }

    [Fact]
    public async Task Date_with_time_zone_is_converted_to_utc()
    {
        var userId = await CreateUser();

        var workout = await Create(userId, "2026-10-31T23:30:00+02:00");

        Assert.Equal(new DateTime(2026, 10, 31, 21, 30, 0, DateTimeKind.Utc), workout.Date);
    }

    [Fact]
    public async Task List_is_ordered_by_date_and_to_is_exclusive()
    {
        var userId = await CreateUser();
        await Create(userId, "2026-11-01T00:00:00Z");
        await Create(userId, "2026-10-05T10:00:00Z");
        await Create(userId, "2026-10-20T10:00:00Z");

        var october = await _client.GetFromJsonAsync<List<WorkoutDto>>(
            $"/api/users/{userId}/workouts?from=2026-10-01&to=2026-11-01", ApiFactory.Json);
        var all = await _client.GetFromJsonAsync<List<WorkoutDto>>($"/api/users/{userId}/workouts", ApiFactory.Json);

        Assert.Equal([5, 20], october!.Select(w => w.Date.Day));
        Assert.Equal(3, all!.Count);
        Assert.True(all[0].Date < all[1].Date && all[1].Date < all[2].Date);
    }

    [Fact]
    public async Task Generated_or_invalid_workouts_return_400()
    {
        var userId = await CreateUser();

        var generated = await _client.PostAsJsonAsync($"/api/users/{userId}/workouts",
            new { date = "2026-10-22T16:00:00Z", activity = "Generated", durationMinutes = 30, intensity = "Low" });
        var zeroMinutes = await _client.PostAsJsonAsync($"/api/users/{userId}/workouts",
            new { date = "2026-10-22T16:00:00Z", activity = "Football", durationMinutes = 0, intensity = "High" });
        var unknownActivity = await _client.PostAsJsonAsync($"/api/users/{userId}/workouts",
            new { date = "2026-10-22T16:00:00Z", activity = "Chess", durationMinutes = 30, intensity = "High" });

        Assert.Equal(HttpStatusCode.BadRequest, generated.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, zeroMinutes.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, unknownActivity.StatusCode);
    }

    [Fact]
    public async Task Delete_removes_the_entry_then_returns_404()
    {
        var userId = await CreateUser();
        var workout = await Create(userId, "2026-10-22T16:00:00Z");

        var first = await _client.DeleteAsync($"/api/workouts/{workout.Id}");
        var second = await _client.DeleteAsync($"/api/workouts/{workout.Id}");

        Assert.Equal(HttpStatusCode.NoContent, first.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, second.StatusCode);
        Assert.Empty((await _client.GetFromJsonAsync<List<WorkoutDto>>($"/api/users/{userId}/workouts", ApiFactory.Json))!);
    }

    [Fact]
    public async Task Unknown_user_returns_404()
    {
        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync("/api/users/99999/workouts")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _client.PostAsJsonAsync("/api/users/99999/workouts",
            new { date = "2026-10-22T16:00:00Z", activity = "Football", durationMinutes = 90, intensity = "High" })).StatusCode);
    }

    private async Task<int> CreateUser()
    {
        var response = await _client.PostAsJsonAsync("/api/users", new { name = "Naptár Teszt", email = "naptar@example.com" });
        return (await response.Content.ReadFromJsonAsync<UserDto>(ApiFactory.Json))!.Id;
    }

    private async Task<WorkoutDto> Create(int userId, string date)
    {
        var response = await _client.PostAsJsonAsync($"/api/users/{userId}/workouts",
            new { date, activity = "Running", durationMinutes = 30, intensity = "Medium" });
        return (await response.Content.ReadFromJsonAsync<WorkoutDto>(ApiFactory.Json))!;
    }
}
