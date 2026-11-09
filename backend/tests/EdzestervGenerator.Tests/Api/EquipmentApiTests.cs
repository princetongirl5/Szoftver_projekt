using System.Net;
using System.Net.Http.Json;
using EdzestervGenerator.Api.DTOs;

namespace EdzestervGenerator.Tests.Api;

[Collection("Api")]
public sealed class EquipmentApiTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Lists_the_12_seeded_items_in_id_order()
    {
        var items = await _client.GetFromJsonAsync<List<EquipmentDto>>("/api/equipment", ApiFactory.Json);

        Assert.Equal(Enumerable.Range(1, 12), items!.Select(e => e.Id));
        Assert.Equal("Kézisúlyzó", items[0].Name);
        Assert.Equal("Rúd", items[7].Name);
    }

    [Fact]
    public async Task New_user_has_no_equipment()
    {
        var userId = await CreateUser();

        var items = await _client.GetFromJsonAsync<List<UserEquipmentDto>>($"/api/users/{userId}/equipment", ApiFactory.Json);

        Assert.Empty(items!);
    }

    [Fact]
    public async Task Save_replaces_the_list_and_allows_the_same_item_with_different_weights()
    {
        var userId = await CreateUser();
        await _client.PutAsJsonAsync($"/api/users/{userId}/equipment", new[] { new { equipmentId = 3, weightKg = (double?)null } });

        var response = await _client.PutAsJsonAsync($"/api/users/{userId}/equipment", new[]
        {
            new { equipmentId = 2, weightKg = (double?)12 },
            new { equipmentId = 1, weightKg = (double?)3 },
            new { equipmentId = 2, weightKg = (double?)8 },
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var items = await _client.GetFromJsonAsync<List<UserEquipmentDto>>($"/api/users/{userId}/equipment", ApiFactory.Json);
        Assert.Equal(
            [new UserEquipmentDto(1, "Kézisúlyzó", 3), new UserEquipmentDto(2, "Kettlebell", 8), new UserEquipmentDto(2, "Kettlebell", 12)],
            items);
    }

    [Fact]
    public async Task Unknown_equipment_or_bad_weight_returns_400()
    {
        var userId = await CreateUser();

        var unknown = await _client.PutAsJsonAsync($"/api/users/{userId}/equipment", new[] { new { equipmentId = 999, weightKg = (double?)null } });
        var badWeight = await _client.PutAsJsonAsync($"/api/users/{userId}/equipment", new[] { new { equipmentId = 1, weightKg = (double?)-5 } });

        Assert.Equal(HttpStatusCode.BadRequest, unknown.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, badWeight.StatusCode);
    }

    [Fact]
    public async Task Unknown_user_returns_404()
    {
        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync("/api/users/99999/equipment")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,
            (await _client.PutAsJsonAsync("/api/users/99999/equipment", Array.Empty<object>())).StatusCode);
    }

    private async Task<int> CreateUser()
    {
        var response = await _client.PostAsJsonAsync("/api/users", new { name = "Eszköz Teszt", email = "eszkoz@example.com" });
        return (await response.Content.ReadFromJsonAsync<UserDto>(ApiFactory.Json))!.Id;
    }
}
