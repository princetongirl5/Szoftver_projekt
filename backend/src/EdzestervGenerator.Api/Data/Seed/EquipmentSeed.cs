using EdzestervGenerator.Api.Models;

namespace EdzestervGenerator.Api.Data.Seed;

// Fixed equipment list. The IDs never change, exercises refer to them.
// 1-7 can be owned at home, 8-12 are gym equipment.
// Everything is written by hand here, there is no wger import.
public static class EquipmentSeed
{
    public static readonly Equipment[] Items =
    [
        Eq(1, "Kézisúlyzó", "Dumbbell", EquipmentLocation.Home),
        Eq(2, "Kettlebell", "Kettlebell", EquipmentLocation.Home),
        Eq(3, "Húzódzkodó rúd", "Pull-up bar", EquipmentLocation.Home),
        Eq(4, "Gumiszalag", "Resistance band", EquipmentLocation.Home),
        Eq(5, "Szőnyeg", "Mat", EquipmentLocation.Home),
        Eq(6, "Pad", "Bench", EquipmentLocation.Home),
        Eq(7, "Ugrálókötél", "Jump rope", EquipmentLocation.Home),
        Eq(8, "Rúd", "Barbell", EquipmentLocation.Gym),
        Eq(9, "SZ-rúd", "EZ bar", EquipmentLocation.Gym),
        Eq(10, "Ferde pad", "Incline bench", EquipmentLocation.Gym),
        Eq(11, "Kábelgép", "Cable machine", EquipmentLocation.Gym),
        Eq(12, "Fitneszlabda", "Exercise ball", EquipmentLocation.Gym),
    ];

    private static Equipment Eq(int id, string name, string nameEn, EquipmentLocation location) => new()
    {
        Id = id,
        Name = name,
        NameEn = nameEn,
        Location = location
    };
}
