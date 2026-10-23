namespace EdzestervGenerator.Api.Models;

// An item the user has at home. The same equipment can appear more than once
// with different weights (e.g. an 8 kg and a 12 kg kettlebell).
public class UserEquipment
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public int EquipmentId { get; set; }
    public Equipment Equipment { get; set; } = null!;
    public double? WeightKg { get; set; } // null for items without weight, e.g. a jump rope
}
