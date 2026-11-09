using System.Net;
using System.Net.Http.Json;
using EdzestervGenerator.Api.DTOs;

namespace EdzestervGenerator.Tests.Api;

[Collection("Api")]
public sealed class UsersApiTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Create_returns_201_with_the_new_user()
    {
        var response = await _client.PostAsJsonAsync("/api/users", new { name = "Anna", email = "anna@example.com" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var user = await response.Content.ReadFromJsonAsync<UserDto>(ApiFactory.Json);
        Assert.NotNull(user);
        Assert.True(user.Id > 0);
        Assert.Equal("Anna", user.Name);
        Assert.Equal(DateTimeKind.Utc, user.CreatedAt.Kind);
    }

    [Fact]
    public async Task Created_user_can_be_read_and_is_in_the_list()
    {
        var created = await CreateUser("Béla");

        var user = await _client.GetFromJsonAsync<UserDto>($"/api/users/{created.Id}", ApiFactory.Json);
        var all = await _client.GetFromJsonAsync<List<UserDto>>("/api/users", ApiFactory.Json);

        Assert.Equal("Béla", user!.Name);
        Assert.Contains(all!, u => u.Id == created.Id);
    }

    [Fact]
    public async Task Update_changes_name_and_email()
    {
        var created = await CreateUser("Cecil");

        var response = await _client.PutAsJsonAsync($"/api/users/{created.Id}", new { name = "Cecília", email = "cecilia@example.com" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var user = await _client.GetFromJsonAsync<UserDto>($"/api/users/{created.Id}", ApiFactory.Json);
        Assert.Equal("Cecília", user!.Name);
        Assert.Equal("cecilia@example.com", user.Email);
    }

    [Fact]
    public async Task Invalid_data_returns_400()
    {
        var response = await _client.PostAsJsonAsync("/api/users", new { name = "", email = "not-an-email" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Unknown_user_returns_404()
    {
        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync("/api/users/99999")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,
            (await _client.PutAsJsonAsync("/api/users/99999", new { name = "X", email = "x@example.com" })).StatusCode);
    }

    private async Task<UserDto> CreateUser(string name)
    {
        var response = await _client.PostAsJsonAsync("/api/users", new { name, email = "test@example.com" });
        return (await response.Content.ReadFromJsonAsync<UserDto>(ApiFactory.Json))!;
    }
}
