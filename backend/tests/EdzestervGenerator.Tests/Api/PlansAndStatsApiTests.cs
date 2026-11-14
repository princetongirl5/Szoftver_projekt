using System.Net;
using System.Net.Http.Json;
using EdzestervGenerator.Api.DTOs;

namespace EdzestervGenerator.Tests.Api;

[Collection("Api")]
public sealed class PlansAndStatsApiTests(ApiFactory factory)
{
    [Fact]
    public async Task Generate_saves_preferences_and_returns_current_plan()
    {
        var (client, _) = await factory.CreateAuthenticatedClient($"plans-{Guid.NewGuid():N}@example.com");
        var request = new { goal = "Hypertrophy", location = "Home", equipmentIds = new[] { 1, 3, 4, 5 }, days = new[] { "Monday", "Wednesday" }, sessionMinutes = 60, difficulty = "Medium" };
        var generated = await client.PostAsJsonAsync("/api/plans/generate", request);
        Assert.Equal(HttpStatusCode.Created, generated.StatusCode);
        var preferences = await client.GetAsync("/api/plans/preferences");
        Assert.Equal(HttpStatusCode.OK, preferences.StatusCode);
        var current = await client.GetAsync("/api/plans/current");
        Assert.Equal(HttpStatusCode.OK, current.StatusCode);
    }

    [Fact]
    public async Task Actual_values_are_saved_and_appear_in_exercise_statistics()
    {
        var (client, _) = await factory.CreateAuthenticatedClient($"stats-{Guid.NewGuid():N}@example.com");
        var generated = await client.PostAsJsonAsync("/api/plans/generate", new { goal = "Hypertrophy", location = "Home", equipmentIds = new[] { 1, 3, 4, 5 }, days = new[] { "Monday", "Wednesday" }, sessionMinutes = 60, difficulty = "Medium" });
        Assert.Equal(HttpStatusCode.Created, generated.StatusCode);
        var plan = await generated.Content.ReadFromJsonAsync<WeekPlanDto>(ApiFactory.Json);
        var exercise = plan!.Days.SelectMany(d => d.Exercises).First();
        var patch = await client.PatchAsJsonAsync($"/api/plans/exercises/{exercise.Id}", new { isDone = true, actualReps = 11, actualWeightKg = 17.5 });
        Assert.Equal(HttpStatusCode.NoContent, patch.StatusCode);
        var list = await client.GetFromJsonAsync<List<ExerciseStatsDto>>("/api/stats/exercises", ApiFactory.Json);
        Assert.Contains(list!, x => x.TimesDone == 1);
        var historyItem = list!.First(x => x.TimesDone == 1);
        var history = await client.GetFromJsonAsync<List<ExerciseHistoryDto>>($"/api/stats/exercises/{historyItem.ExerciseId}", ApiFactory.Json);
        Assert.Contains(history!, x => x.ActualReps == 11 && x.ActualWeightKg == 17.5 && x.IsDone);
    }

    [Fact]
    public async Task Plan_preferences_are_private_to_each_account()
    {
        var (first, _) = await factory.CreateAuthenticatedClient($"private-a-{Guid.NewGuid():N}@example.com");
        var (second, _) = await factory.CreateAuthenticatedClient($"private-b-{Guid.NewGuid():N}@example.com");
        var generated = await first.PostAsJsonAsync("/api/plans/generate", new { goal = "Strength", location = "Home", equipmentIds = new[] { 1, 3 }, days = new[] { "Monday", "Wednesday" }, sessionMinutes = 45, difficulty = "Easy" });
        Assert.Equal(HttpStatusCode.Created, generated.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await second.GetAsync("/api/plans/preferences")).StatusCode);
    }
}
