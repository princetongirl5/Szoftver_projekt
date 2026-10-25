using EdzestervGenerator.Api.DTOs;
using EdzestervGenerator.Api.Models;

namespace EdzestervGenerator.Api.Services;

/// <summary>Pure workout selection logic. It does not read or write the database.</summary>
public sealed class WorkoutGenerator : IWorkoutGenerator
{
    public IReadOnlyList<GeneratedWorkoutDay> GenerateWeek(
        GeneratePlanRequest request,
        IReadOnlyList<Exercise> exercises,
        IReadOnlyList<int> equipmentIds,
        Random random)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(exercises);
        ArgumentNullException.ThrowIfNull(equipmentIds);
        ArgumentNullException.ThrowIfNull(random);
        Validate(request);

        var count = request.SessionMinutes switch { 45 => 4, 60 => 5, 75 => 6, 90 => 7, _ => throw new ArgumentOutOfRangeException(nameof(request.SessionMinutes)) };
        var allowedEquipment = equipmentIds.ToHashSet();
        var available = exercises
            .Where(e => request.Location == TrainingLocation.Gym || e.EquipmentId is null || allowedEquipment.Contains(e.EquipmentId.Value))
            .Where(e => (int)e.Difficulty <= (int)request.Difficulty)
            .ToList();

        var days = request.Days.Distinct().OrderBy(WeekOrder).ToArray();
        var fullBody = days.Length <= 3;
        var plans = new List<GeneratedWorkoutDay>(days.Length);
        var usedInWeek = new HashSet<int>();

        for (var i = 0; i < days.Length; i++)
        {
            var day = days[i];
            var groups = fullBody
                ? new[] { MuscleGroup.FullBody, MuscleGroup.UpperBody, MuscleGroup.LowerBody, MuscleGroup.Core }
                : i % 2 == 0
                    ? new[] { MuscleGroup.UpperBody, MuscleGroup.Core, MuscleGroup.FullBody }
                    : new[] { MuscleGroup.LowerBody, MuscleGroup.Core, MuscleGroup.FullBody };

            var candidates = available.Where(e => groups.Contains(e.MuscleGroup)).ToList();
            // Staple push-ups are selected first on at least one upper-body day when available.
            var staple = candidates.FirstOrDefault(e => e.Name.Contains("fekvőtámasz", StringComparison.OrdinalIgnoreCase)
                || e.Name.Contains("push-up", StringComparison.OrdinalIgnoreCase)
                || e.Name.Contains("push up", StringComparison.OrdinalIgnoreCase));
            var selected = new List<Exercise>();
            if (staple is not null && !usedInWeek.Contains(staple.Id)) selected.Add(staple);

            var fresh = candidates.Where(e => selected.All(s => s.Id != e.Id) && !usedInWeek.Contains(e.Id)).ToList();
            Shuffle(fresh, random);
            selected.AddRange(fresh.Take(Math.Max(0, count - selected.Count)));

            // A small seed may not contain enough distinct exercises for every day. Reuse across days,
            // but never duplicate an exercise within a single workout.
            if (selected.Count < count)
            {
                var reuse = candidates.Where(e => selected.All(s => s.Id != e.Id)).ToList();
                Shuffle(reuse, random);
                selected.AddRange(reuse.Take(count - selected.Count));
            }

            usedInWeek.UnionWith(selected.Select(e => e.Id));
            var (sets, reps) = Prescription(request.Goal, request.Difficulty);
            var planned = selected.Select(e => new PlannedExercise(e, sets,
                e.MuscleGroup == MuscleGroup.Mobility ? Math.Max(reps, 12) : reps,
                e.EquipmentId is null ? null : e.MinWeightKg)).ToArray();
            plans.Add(new GeneratedWorkoutDay(day, WorkoutName(groups, i, fullBody), planned));
        }

        return plans;
    }

    private static void Validate(GeneratePlanRequest request)
    {
        if (request.Days is null || request.Days.Count is < 2 or > 6 || request.Days.Distinct().Count() != request.Days.Count)
            throw new ArgumentException("2 és 6 közötti, különböző edzésnapot válassz.", nameof(request));
        if (request.SessionMinutes is not (45 or 60 or 75 or 90))
            throw new ArgumentException("Az edzés hossza 45, 60, 75 vagy 90 perc lehet.", nameof(request));
        if (!Enum.IsDefined(request.Goal) || !Enum.IsDefined(request.Location) || !Enum.IsDefined(request.Difficulty)
            || request.Days.Any(d => !Enum.IsDefined(d)))
            throw new ArgumentException("A kérés ismeretlen enum értéket tartalmaz.", nameof(request));
    }

    private static (int Sets, int Reps) Prescription(TrainingGoal goal, Difficulty difficulty)
    {
        var reps = goal switch
        {
            TrainingGoal.Strength => 5,
            TrainingGoal.Hypertrophy => 10,
            TrainingGoal.Endurance => 18,
            _ => 12
        };
        var sets = difficulty switch { Difficulty.Easy => 3, Difficulty.Medium => 4, _ => 5 };
        return (sets, reps);
    }

    private static string WorkoutName(MuscleGroup[] groups, int index, bool fullBody) => fullBody
        ? "Teljes test"
        : groups[0] == MuscleGroup.UpperBody
            ? $"Felsőtest {(index / 2) + 1}"
            : $"Alsótest {(index / 2) + 1}";

    private static int WeekOrder(DayOfWeek day) => ((int)day + 6) % 7;

    private static void Shuffle<T>(IList<T> list, Random random)
    {
        for (var i = list.Count - 1; i > 0; i--)
        {
            var j = random.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
