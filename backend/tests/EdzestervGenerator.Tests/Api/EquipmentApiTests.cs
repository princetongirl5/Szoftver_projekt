using System.Net;
using System.Net.Http.Json;
using EdzestervGenerator.Api.DTOs;
using EdzestervGenerator.Api.Models;

namespace EdzestervGenerator.Tests.Api;

[Collection("Api")]
public sealed class EquipmentApiTests(ApiFactory factory)
{
    [Fact]
    public async Task Lists_the_12_seeded_items_in_id_order()
    {
        using var client = factory.CreateClient();
        var items = await client.GetFromJsonAsync<List<EquipmentDto>>("/api/equipment", ApiFactory.Json);
        Assert.Equal(Enumerable.Range(1, 12), items!.Select(e => e.Id));
        Assert.Equal("Kézisúlyzó", items[0].Name);
        Assert.Equal("Rúd", items[7].Name);
    }

    [Fact]
    public async Task Home_filter_returns_only_home_equipment()
    {
        using var client = factory.CreateClient();
        var items = await client.GetFromJsonAsync<List<EquipmentDto>>("/api/equipment?location=Home", ApiFactory.Json);
        Assert.Equal(Enumerable.Range(1, 7), items!.Select(e => e.Id));
    }

    [Fact]
    public async Task English_header_returns_english_names()
    {
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("Accept-Language", "en");
        var items = await client.GetFromJsonAsync<List<EquipmentDto>>("/api/equipment", ApiFactory.Json);
        Assert.Equal("Dumbbell", items![0].Name);
    }

    [Fact]
    public async Task User_equipment_is_private_and_can_contain_multiple_weights_for_same_item()
    {
        var (client, userId) = await factory.CreateAuthenticatedClient($"equipment-{Guid.NewGuid():N}@example.com");
        var empty = await client.GetFromJsonAsync<List<UserEquipmentDto>>($"/api/users/{userId}/equipment", ApiFactory.Json);
        Assert.Empty(empty!);
        var response = await client.PutAsJsonAsync($"/api/users/{userId}/equipment", new[]
        {
            new { equipmentId = 2, weightKg = (double?)12 },
            new { equipmentId = 1, weightKg = (double?)3 },
            new { equipmentId = 2, weightKg = (double?)8 },
        });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var items = await client.GetFromJsonAsync<List<UserEquipmentDto>>($"/api/users/{userId}/equipment", ApiFactory.Json);
        Assert.Equal(3, items!.Count);
    }

    [Fact]
    public async Task User_cannot_access_another_users_equipment()
    {
        var (first, _) = await factory.CreateAuthenticatedClient($"equipment-a-{Guid.NewGuid():N}@example.com");
        var (_, secondId) = await factory.CreateAuthenticatedClient($"equipment-b-{Guid.NewGuid():N}@example.com");
        Assert.Equal(HttpStatusCode.Forbidden, (await first.GetAsync($"/api/users/{secondId}/equipment")).StatusCode);
    }
}
