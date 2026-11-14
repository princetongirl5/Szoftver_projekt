using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using EdzestervGenerator.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace EdzestervGenerator.Tests.Api;

public sealed class ApiFactory : WebApplicationFactory<Program>
{
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"edzesterv-test-{Guid.NewGuid()}.db");
    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };
    protected override void ConfigureWebHost(IWebHostBuilder builder) => builder.UseSetting("ConnectionStrings:Default", $"Data Source={_dbPath}");
    public async Task<(HttpClient Client, int UserId)> CreateAuthenticatedClient(string email, string name = "Teszt")
    {
        var client = CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });
        var register = await client.PostAsJsonAsync("/api/auth/register", new { email, password = "TesztJelszo123" });
        if (!register.IsSuccessStatusCode) throw new InvalidOperationException($"Register failed: {await register.Content.ReadAsStringAsync()}");
        var login = await client.PostAsJsonAsync("/api/auth/login?useCookies=true", new { email, password = "TesztJelszo123" });
        if (!login.IsSuccessStatusCode) throw new InvalidOperationException($"Login failed: {await login.Content.ReadAsStringAsync()}");
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userId = await db.Users.Where(x => x.Email == email).Select(x => x.Id).SingleAsync();
        return (client, userId);
    }
    protected override void Dispose(bool disposing) { base.Dispose(disposing); SqliteConnection.ClearAllPools(); File.Delete(_dbPath); }
}
[CollectionDefinition("Api")]
public sealed class ApiCollection : ICollectionFixture<ApiFactory>;
