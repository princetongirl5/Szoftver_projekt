using EdzestervGenerator.Api.DTOs;

namespace EdzestervGenerator.Api.Models;

public sealed class PlanPreference
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public TrainingGoal Goal { get; set; }
    public TrainingLocation Location { get; set; }
    public List<int> EquipmentIds { get; set; } = [];
    public List<DayOfWeek> Days { get; set; } = [];
    public int SessionMinutes { get; set; }
    public Difficulty Difficulty { get; set; }
    public DateTime UpdatedAt { get; set; }
}
