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
    ];
}
