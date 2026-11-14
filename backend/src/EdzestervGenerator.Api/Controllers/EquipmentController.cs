using EdzestervGenerator.Api.Data;
using EdzestervGenerator.Api.DTOs;
using EdzestervGenerator.Api.Services;
using EdzestervGenerator.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EdzestervGenerator.Api.Controllers;

[ApiController, Route("api/equipment")]
public sealed class EquipmentController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<List<EquipmentDto>> GetAll([FromQuery] string? location, CancellationToken cancellationToken)
    {
        var query = db.Equipment.AsNoTracking();
        if (Enum.TryParse<EquipmentLocation>(location, true, out var parsed)) query = query.Where(e => e.Location == parsed);
        return await query.OrderBy(e => e.Id).Select(e => new EquipmentDto(e.Id,
            LocalizationText.IsEnglish ? e.NameEn : e.Name, e.Location)).ToListAsync(cancellationToken);
    }
}
