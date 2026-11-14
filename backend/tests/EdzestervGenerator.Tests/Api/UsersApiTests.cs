using System.Net;
using System.Net.Http.Json;

namespace EdzestervGenerator.Tests.Api;

[Collection("Api")]
public sealed class UsersApiTests(ApiFactory factory)
{
    [Fact]
    public async Task Register_login_and_manage_info_work()
    {
        var (client, id) = await factory.CreateAuthenticatedClient($"user-{Guid.NewGuid():N}@example.com", "Anna");
        Assert.True(id > 0);
        Assert.Equal(HttpStatusCode.NoContent, (await client.PostAsync("/api/auth/logout", null)).StatusCode);
    }

    [Fact]
    public async Task Protected_api_returns_401_without_login()
    {
        using var client = factory.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/plans/current")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/stats")).StatusCode);
    }

    [Fact]
    public async Task Legacy_users_endpoints_require_authentication()
    {
        using var client = factory.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/users")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/users/1")).StatusCode);
    }

    [Fact]
    public async Task Duplicate_email_is_rejected()
    {
        var email = $"duplicate-{Guid.NewGuid():N}@example.com";
        var (first, _) = await factory.CreateAuthenticatedClient(email);
        using var second = factory.CreateClient();
        var response = await second.PostAsJsonAsync("/api/auth/register", new { email, password = "TesztJelszo123" });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
