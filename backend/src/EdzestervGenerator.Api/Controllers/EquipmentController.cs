using EdzestervGenerator.Api.Data;
using EdzestervGenerator.Api.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EdzestervGenerator.Api.Controllers;

[ApiController]
[Route("api/equipment")]
public class EquipmentController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<List<EquipmentDto>> GetAll()
    {
        return await db.Equipment
            .OrderBy(e => e.Id)
            .Select(e => new EquipmentDto(e.Id, e.Name))
            .ToListAsync();
    }
}
