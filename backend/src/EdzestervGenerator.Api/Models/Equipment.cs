namespace EdzestervGenerator.Api.Models;

public class Equipment
{
    public int Id { get; set; }
    public string Name { get; set; } = "";   // Hungarian (default language)
    public string NameEn { get; set; } = ""; // English
    public EquipmentLocation Location { get; set; }
}
