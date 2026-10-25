using EdzestervGenerator.Api.Models;

namespace EdzestervGenerator.Api.Data.Seed;

/// <summary>
/// Small curated catalogue. Replace/extend names and equipment IDs when the wger import
/// is integrated; IDs below are stable local IDs, equipment IDs match EquipmentSeed.
/// </summary>
public static class ExerciseSeed
{
    public static readonly Exercise[] Items =
    [
        Ex(1, "Fekvőtámasz", MuscleGroup.UpperBody, Difficulty.Easy),
        Ex(2, "Térdelő fekvőtámasz", MuscleGroup.UpperBody, Difficulty.Easy),
        Ex(3, "Fekvenyomás", MuscleGroup.UpperBody, Difficulty.Medium, 6, 20),
        Ex(4, "Kézisúlyzós vállból nyomás", MuscleGroup.UpperBody, Difficulty.Medium, 1, 4),
        Ex(5, "Oldalemelés kézisúlyzóval", MuscleGroup.UpperBody, Difficulty.Easy, 1, 2),
        Ex(6, "Bicepsz hajlítás", MuscleGroup.UpperBody, Difficulty.Easy, 1, 2),
        Ex(7, "Tricepsz tolódzkodás", MuscleGroup.UpperBody, Difficulty.Medium),
        Ex(8, "Evezés kézisúlyzóval", MuscleGroup.UpperBody, Difficulty.Medium, 1, 4),
        Ex(9, "Húzódzkodás", MuscleGroup.UpperBody, Difficulty.Hard, 3),
        Ex(10, "Gumiszalagos evezés", MuscleGroup.UpperBody, Difficulty.Easy, 4),
        Ex(11, "Guggolás", MuscleGroup.LowerBody, Difficulty.Easy),
        Ex(12, "Serlegguggolás", MuscleGroup.LowerBody, Difficulty.Medium, 2, 8),
        Ex(13, "Kitörés", MuscleGroup.LowerBody, Difficulty.Medium),
        Ex(14, "Román felhúzás", MuscleGroup.LowerBody, Difficulty.Medium, 1, 8),
        Ex(15, "Csípőemelés", MuscleGroup.LowerBody, Difficulty.Easy),
        Ex(16, "Vádliemelés", MuscleGroup.LowerBody, Difficulty.Easy),
        Ex(17, "Fellépés padra", MuscleGroup.LowerBody, Difficulty.Medium, 6),
        Ex(18, "Plank", MuscleGroup.Core, Difficulty.Easy),
        Ex(19, "Hasprés", MuscleGroup.Core, Difficulty.Easy),
        Ex(20, "Oldalsó plank", MuscleGroup.Core, Difficulty.Medium),
        Ex(21, "Dead bug", MuscleGroup.Core, Difficulty.Easy),
        Ex(22, "Hegymászó", MuscleGroup.FullBody, Difficulty.Medium),
        Ex(23, "Jumping jack", MuscleGroup.FullBody, Difficulty.Easy),
        Ex(24, "Burpee", MuscleGroup.FullBody, Difficulty.Hard),
        Ex(25, "Teljes testes átmozgatás", MuscleGroup.Mobility, Difficulty.Easy),
        Ex(26, "Gumiszalagos vállmobilizálás", MuscleGroup.Mobility, Difficulty.Easy, 4),
    ];

    private static Exercise Ex(int id, string name, MuscleGroup group, Difficulty difficulty,
        int? equipmentId = null, double? minWeightKg = null) => new()
    {
        Id = id,
        Name = name,
        MuscleGroup = group,
        Difficulty = difficulty,
        EquipmentId = equipmentId,
        MinWeightKg = minWeightKg
    };
}
