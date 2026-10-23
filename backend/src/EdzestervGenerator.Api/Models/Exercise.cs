namespace EdzestervGenerator.Api.Models;

public class Exercise
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public MuscleGroup MuscleGroup { get; set; }
    public Difficulty Difficulty { get; set; }
    public int? EquipmentId { get; set; } // null = bodyweight exercise
    public Equipment? Equipment { get; set; }
    public double? MinWeightKg { get; set; }
}
