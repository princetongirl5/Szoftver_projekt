using EdzestervGenerator.Api.Models;

namespace EdzestervGenerator.Api.Data.Seed;

/// <summary>
/// Hand-written exercise catalogue (no wger import). IDs are stable local IDs, equipment IDs
/// match EquipmentSeed. 1-26 are bodyweight and home exercises, 27-48 are the gym exercises
/// for the gym equipment (8-12). Name is Hungarian, NameEn is English.
/// The generator picks the first exercise whose Hungarian Name contains "fekvőtámasz" as the staple
/// push-up, so "Fekvőtámasz" (id 1) must stay before every other name containing that word.
/// </summary>
public static class ExerciseSeed
{
    public static readonly Exercise[] Items =
    [
        // Home and bodyweight
        Ex(1, "Fekvőtámasz", "Push-up", MuscleGroup.UpperBody, Difficulty.Easy),
        Ex(2, "Térdelő fekvőtámasz", "Knee push-up", MuscleGroup.UpperBody, Difficulty.Easy),
        Ex(3, "Fekvenyomás", "Bench press", MuscleGroup.UpperBody, Difficulty.Medium, 6, 20),
        Ex(4, "Kézisúlyzós vállból nyomás", "Dumbbell shoulder press", MuscleGroup.UpperBody, Difficulty.Medium, 1, 4),
        Ex(5, "Oldalemelés kézisúlyzóval", "Dumbbell lateral raise", MuscleGroup.UpperBody, Difficulty.Easy, 1, 2),
        Ex(6, "Bicepsz hajlítás", "Biceps curl", MuscleGroup.UpperBody, Difficulty.Easy, 1, 2),
        Ex(7, "Tricepsz tolódzkodás", "Triceps dip", MuscleGroup.UpperBody, Difficulty.Medium),
        Ex(8, "Evezés kézisúlyzóval", "Dumbbell row", MuscleGroup.UpperBody, Difficulty.Medium, 1, 4),
        Ex(9, "Húzódzkodás", "Pull-up", MuscleGroup.UpperBody, Difficulty.Hard, 3),
        Ex(10, "Gumiszalagos evezés", "Resistance band row", MuscleGroup.UpperBody, Difficulty.Easy, 4),
        Ex(11, "Guggolás", "Squat", MuscleGroup.LowerBody, Difficulty.Easy),
        Ex(12, "Serlegguggolás", "Goblet squat", MuscleGroup.LowerBody, Difficulty.Medium, 2, 8),
        Ex(13, "Kitörés", "Lunge", MuscleGroup.LowerBody, Difficulty.Medium),
        Ex(14, "Román felhúzás", "Romanian deadlift", MuscleGroup.LowerBody, Difficulty.Medium, 1, 8),
        Ex(15, "Csípőemelés", "Glute bridge", MuscleGroup.LowerBody, Difficulty.Easy),
        Ex(16, "Vádliemelés", "Calf raise", MuscleGroup.LowerBody, Difficulty.Easy),
        Ex(17, "Fellépés padra", "Step-up", MuscleGroup.LowerBody, Difficulty.Medium, 6),
        Ex(18, "Plank", "Plank", MuscleGroup.Core, Difficulty.Easy),
        Ex(19, "Hasprés", "Crunch", MuscleGroup.Core, Difficulty.Easy),
        Ex(20, "Oldalsó plank", "Side plank", MuscleGroup.Core, Difficulty.Medium),
        Ex(21, "Dead bug", "Dead bug", MuscleGroup.Core, Difficulty.Easy),
        Ex(22, "Hegymászó", "Mountain climber", MuscleGroup.FullBody, Difficulty.Medium),
        Ex(23, "Jumping jack", "Jumping jack", MuscleGroup.FullBody, Difficulty.Easy),
        Ex(24, "Burpee", "Burpee", MuscleGroup.FullBody, Difficulty.Hard),
        Ex(25, "Teljes testes átmozgatás", "Full-body mobility flow", MuscleGroup.Mobility, Difficulty.Easy),
        Ex(26, "Gumiszalagos vállmobilizálás", "Resistance band shoulder mobility", MuscleGroup.Mobility, Difficulty.Easy, 4),

        // Gym: barbell (8), the empty bar weighs 20 kg
        Ex(27, "Rudas guggolás", "Barbell back squat", MuscleGroup.LowerBody, Difficulty.Medium, 8, 20),
        Ex(28, "Felhúzás", "Deadlift", MuscleGroup.LowerBody, Difficulty.Hard, 8, 20),
        Ex(29, "Rudas evezés", "Barbell row", MuscleGroup.UpperBody, Difficulty.Medium, 8, 20),
        Ex(30, "Rudas vállból nyomás", "Barbell overhead press", MuscleGroup.UpperBody, Difficulty.Medium, 8, 20),
        Ex(31, "Rudas csípőemelés", "Barbell hip thrust", MuscleGroup.LowerBody, Difficulty.Medium, 8, 20),

        // Gym: EZ bar (9), the bar weighs about 10 kg
        Ex(32, "SZ-rudas bicepszhajlítás", "EZ-bar biceps curl", MuscleGroup.UpperBody, Difficulty.Easy, 9, 10),
        Ex(33, "Fekvő tricepsznyomás SZ-rúddal", "EZ-bar skull crusher", MuscleGroup.UpperBody, Difficulty.Medium, 9, 10),
        Ex(34, "SZ-rudas felhúzás állhoz", "EZ-bar upright row", MuscleGroup.UpperBody, Difficulty.Medium, 9, 10),

        // Gym: incline bench (10)
        Ex(35, "Ferde pados súlyzós nyomás", "Incline dumbbell press", MuscleGroup.UpperBody, Difficulty.Medium, 10, 6),
        Ex(36, "Ferde pados súlyzós keresztemelés", "Incline dumbbell fly", MuscleGroup.UpperBody, Difficulty.Medium, 10, 4),
        Ex(37, "Mellkastámaszos evezés ferde padon", "Incline bench chest-supported row", MuscleGroup.UpperBody, Difficulty.Medium, 10, 6),

        // Gym: cable machine (11)
        Ex(38, "Csigás lehúzás", "Lat pulldown", MuscleGroup.UpperBody, Difficulty.Medium, 11, 20),
        Ex(39, "Ülő csigás evezés", "Seated cable row", MuscleGroup.UpperBody, Difficulty.Medium, 11, 20),
        Ex(40, "Tricepsz letolás csigán", "Cable triceps pushdown", MuscleGroup.UpperBody, Difficulty.Easy, 11, 10),
        Ex(41, "Arcra húzás csigán", "Cable face pull", MuscleGroup.UpperBody, Difficulty.Easy, 11, 5),
        Ex(42, "Kábeles favágás", "Cable wood chop", MuscleGroup.Core, Difficulty.Medium, 11, 5),
        Ex(43, "Kábeles hasprés", "Cable crunch", MuscleGroup.Core, Difficulty.Medium, 11, 10),

        // Gym: exercise ball (12), no weight
        Ex(44, "Labdás hasprés", "Stability ball crunch", MuscleGroup.Core, Difficulty.Easy, 12),
        Ex(45, "Labdás hátsó combhajlítás", "Stability ball hamstring curl", MuscleGroup.LowerBody, Difficulty.Medium, 12),
        Ex(46, "Fali guggolás labdával", "Stability ball wall squat", MuscleGroup.LowerBody, Difficulty.Easy, 12),
        Ex(47, "Labdás plank", "Stability ball plank", MuscleGroup.Core, Difficulty.Medium, 12),
        Ex(48, "Labdás hátemelés", "Stability ball back extension", MuscleGroup.LowerBody, Difficulty.Easy, 12),
    ];

    private static Exercise Ex(int id, string name, string nameEn, MuscleGroup group, Difficulty difficulty,
        int? equipmentId = null, double? minWeightKg = null) => new()
    {
        Id = id,
        Name = name,
        NameEn = nameEn,
        MuscleGroup = group,
        Difficulty = difficulty,
        EquipmentId = equipmentId,
        MinWeightKg = minWeightKg
    };
}
