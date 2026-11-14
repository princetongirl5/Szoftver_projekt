using System.Globalization;

namespace EdzestervGenerator.Api.Services;

public static class LocalizationText
{
    public static bool IsEnglish => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.Equals("en", StringComparison.OrdinalIgnoreCase);
    public static string ExerciseName(string hu, string en) => IsEnglish && !string.IsNullOrWhiteSpace(en) ? en : hu;
    public static string WorkoutName(string hu) => hu switch
    {
        "Teljes test" => IsEnglish ? "Full body" : hu,
        "Felsőtest" => IsEnglish ? "Upper body" : hu,
        "Alsótest" => IsEnglish ? "Lower body" : hu,
        "Edzés" => IsEnglish ? "Workout" : hu,
        _ => hu
    };
    public static string Validation(string hu, string en) => IsEnglish ? en : hu;
}
