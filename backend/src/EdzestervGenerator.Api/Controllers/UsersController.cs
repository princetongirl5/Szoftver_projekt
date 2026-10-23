using EdzestervGenerator.Api.Data;
using EdzestervGenerator.Api.DTOs;
using EdzestervGenerator.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EdzestervGenerator.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<List<UserDto>> GetAll()
    {
        var users = await db.Users.OrderBy(u => u.Name).ToListAsync();
        return users.Select(ToDto).ToList();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> Get(int id)
    {
        var user = await db.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        return ToDto(user);
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> Create(SaveUserRequest request)
    {
        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            CreatedAt = DateTime.UtcNow
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = user.Id }, ToDto(user));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<UserDto>> Update(int id, SaveUserRequest request)
    {
        var user = await db.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        user.Name = request.Name;
        user.Email = request.Email;
        await db.SaveChangesAsync();

        return ToDto(user);
    }

    // SQLite does not store the DateTime kind, so mark it as UTC before sending it out.
    private static UserDto ToDto(User user) =>
        new(user.Id, user.Name, user.Email, DateTime.SpecifyKind(user.CreatedAt, DateTimeKind.Utc));
}
