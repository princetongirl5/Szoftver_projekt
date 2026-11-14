using Microsoft.AspNetCore.Identity;

namespace EdzestervGenerator.Api.Models;

public class User : IdentityUser<int>
{
    public string Name { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // UTC
    public List<UserEquipment> Equipment { get; set; } = [];
    public List<Workout> Workouts { get; set; } = [];
    public PlanPreference? PlanPreference { get; set; }
}
