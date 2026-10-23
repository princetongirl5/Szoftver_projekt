using EdzestervGenerator.Api.Data;
using EdzestervGenerator.Api.DTOs;
using EdzestervGenerator.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EdzestervGenerator.Api.Controllers;

// The equipment a user has at home.
[ApiController]
[Route("api/users/{userId}/equipment")]
public class UserEquipmentController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<UserEquipmentDto>>> Get(int userId)
    {
        if (!await db.Users.AnyAsync(u => u.Id == userId))
        {
            return NotFound();
        }

        return await GetItems(userId);
    }

    // Replaces the whole list with the one sent by the frontend.
    [HttpPut]
    public async Task<ActionResult<List<UserEquipmentDto>>> Save(int userId, List<SaveUserEquipmentItem> items)
    {
        if (!await db.Users.AnyAsync(u => u.Id == userId))
        {
            return NotFound();
        }

        var equipmentIds = await db.Equipment.Select(e => e.Id).ToListAsync();
        foreach (var item in items)
        {
            if (!equipmentIds.Contains(item.EquipmentId))
            {
                ModelState.AddModelError("equipmentId", $"Unknown equipment id: {item.EquipmentId}");
                return ValidationProblem();
            }
        }

        var oldItems = await db.UserEquipment.Where(ue => ue.UserId == userId).ToListAsync();
        db.UserEquipment.RemoveRange(oldItems);

        foreach (var item in items)
        {
            db.UserEquipment.Add(new UserEquipment
            {
                UserId = userId,
                EquipmentId = item.EquipmentId,
                WeightKg = item.WeightKg
            });
        }

        await db.SaveChangesAsync();

        return await GetItems(userId);
    }

    private async Task<List<UserEquipmentDto>> GetItems(int userId)
    {
        return await db.UserEquipment
            .Where(ue => ue.UserId == userId)
            .OrderBy(ue => ue.EquipmentId)
            .ThenBy(ue => ue.WeightKg)
            .Select(ue => new UserEquipmentDto(ue.EquipmentId, ue.Equipment.Name, ue.WeightKg))
            .ToListAsync();
    }
}
