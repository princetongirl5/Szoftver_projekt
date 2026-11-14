using System.Globalization;
using System.Text.Json.Serialization;
using EdzestervGenerator.Api.Data;
using EdzestervGenerator.Api.Data.Seed;
using EdzestervGenerator.Api.Models;
using EdzestervGenerator.Api.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(options =>
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.Configure<Microsoft.AspNetCore.Mvc.ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var english = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "en";
        var errors = context.ModelState.Where(x => x.Value?.Errors.Count > 0)
            .ToDictionary(x => x.Key, _ => new[] { english ? "The request contains invalid values." : "A kérés érvénytelen adatokat tartalmaz." });
        return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(new Microsoft.AspNetCore.Mvc.ValidationProblemDetails(errors)
        {
            Title = english ? "Validation failed." : "Sikertelen adatellenőrzés."
        });
    };
});
builder.Services.AddOpenApi();
builder.Services.AddLocalization();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddIdentityApiEndpoints<User>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddErrorDescriber<LocalizedIdentityErrorDescriber>();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    };
});
builder.Services.AddScoped<IWorkoutGenerator, WorkoutGenerator>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    var existing = await db.Exercises.ToDictionaryAsync(e => e.Id);
    foreach (var item in ExerciseSeed.Items)
    {
        if (existing.TryGetValue(item.Id, out var current))
        {
            current.Name = item.Name;
            current.NameEn = item.NameEn;
            current.MuscleGroup = item.MuscleGroup;
            current.Difficulty = item.Difficulty;
            current.EquipmentId = item.EquipmentId;
            current.MinWeightKg = item.MinWeightKg;
        }
        else db.Exercises.Add(new Exercise { Id = item.Id, Name = item.Name, NameEn = item.NameEn,
            MuscleGroup = item.MuscleGroup, Difficulty = item.Difficulty, EquipmentId = item.EquipmentId, MinWeightKg = item.MinWeightKg });
    }
    await db.SaveChangesAsync();
}

var supported = new[] { new CultureInfo("hu"), new CultureInfo("en") };
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("hu"),
    SupportedCultures = supported,
    SupportedUICultures = supported,
    RequestCultureProviders = [new Microsoft.AspNetCore.Localization.AcceptLanguageHeaderRequestCultureProvider()]
});
if (app.Environment.IsDevelopment()) app.MapOpenApi();
if (!app.Environment.IsDevelopment()) app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapGroup("/api/auth").MapIdentityApi<User>();
app.MapPost("/api/auth/logout", async (SignInManager<User> signInManager) =>
{
    await signInManager.SignOutAsync();
    return Results.NoContent();
}).RequireAuthorization();
app.MapControllers();
app.Run();

public partial class Program { }
