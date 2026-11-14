using EdzestervGenerator.Api.Data;
using EdzestervGenerator.Api.DTOs;
using EdzestervGenerator.Api.Models;
using EdzestervGenerator.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EdzestervGenerator.Api.Controllers;

[ApiController, Authorize, Route("api/users/{userId:int}/equipment")]
public sealed class UserEquipmentController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<UserEquipmentDto>>> Get(int userId, CancellationToken cancellationToken)
    {
        var current = RequestUser.Id(User);
        if (current is null) return Unauthorized();
        if (current != userId) return Forbid();
        return await GetItems(current.Value, cancellationToken);
    }

    [HttpPut]
    public async Task<ActionResult<List<UserEquipmentDto>>> Save(int userId, List<SaveUserEquipmentItem> items, CancellationToken cancellationToken)
    {
        var current = RequestUser.Id(User);
        if (current is null) return Unauthorized();
        if (current != userId) return Forbid();
        var equipmentIds = await db.Equipment.Select(e => e.Id).ToListAsync(cancellationToken);
        foreach (var item in items)
            if (!equipmentIds.Contains(item.EquipmentId)) { ModelState.AddModelError("equipmentId", "Unknown equipment id."); return ValidationProblem(); }
        var oldItems = await db.UserEquipment.Where(ue => ue.UserId == current).ToListAsync(cancellationToken);
        db.UserEquipment.RemoveRange(oldItems);
        db.UserEquipment.AddRange(items.Select(item => new UserEquipment { UserId = current.Value, EquipmentId = item.EquipmentId, WeightKg = item.WeightKg }));
        await db.SaveChangesAsync(cancellationToken);
        return await GetItems(current.Value, cancellationToken);
    }

    private async Task<List<UserEquipmentDto>> GetItems(int userId, CancellationToken cancellationToken) =>
        await db.UserEquipment.Where(ue => ue.UserId == userId).OrderBy(ue => ue.EquipmentId).ThenBy(ue => ue.WeightKg)
            .Select(ue => new UserEquipmentDto(ue.EquipmentId, LocalizationText.IsEnglish ? ue.Equipment.NameEn : ue.Equipment.Name, ue.WeightKg)).ToListAsync(cancellationToken);
}
