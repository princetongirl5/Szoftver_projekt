namespace EdzestervGenerator.Api.Models;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public DateTime CreatedAt { get; set; } // UTC

    public List<UserEquipment> Equipment { get; set; } = [];
    public List<Workout> Workouts { get; set; } = [];
}
