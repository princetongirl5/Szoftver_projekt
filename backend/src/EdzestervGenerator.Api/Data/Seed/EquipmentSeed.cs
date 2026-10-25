using EdzestervGenerator.Api.Models;

namespace EdzestervGenerator.Api.Data.Seed;

// Fixed equipment list. The IDs never change, exercises refer to them.
// The comments show the matching wger equipment ID for the exercise import.
public static class EquipmentSeed
{
    public static readonly Equipment[] Items =
    [
        new() { Id = 1, Name = "Kézisúlyzó" },     // wger 3: Dumbbell
        new() { Id = 2, Name = "Kettlebell" },     // wger 10: Kettlebell
        new() { Id = 3, Name = "Húzódzkodó rúd" }, // wger 6: Pull-up bar
        new() { Id = 4, Name = "Gumiszalag" },     // wger 11: Resistance band
        new() { Id = 5, Name = "Szőnyeg" },        // wger 4: Gym mat
        new() { Id = 6, Name = "Pad" },            // wger 8: Bench
        new() { Id = 7, Name = "Ugrálókötél" },    // not in wger
        new() { Id = 8, Name = "Rúd" },            // wger 1: Barbell
        new() { Id = 9, Name = "SZ-rúd" },         // wger 2: SZ-Bar
        new() { Id = 10, Name = "Ferde pad" },     // wger 9: Incline bench
        new() { Id = 11, Name = "Kábelgép" },      // wger 12: Cable machine
        new() { Id = 12, Name = "Fitneszlabda" },  // wger 5: Swiss Ball
    ];
}
