using EdzestervGenerator.Api.DTOs;

namespace EdzestervGenerator.Api.Services;

// One generated workout: its local (Budapest) date and how many exercises are ticked.
public record WorkoutDay(DateOnly Date, int ExerciseCount, int DoneCount);

// Calculates the stats screen from the stored weekly plans (see docs/api/stats-api.md).
// It does not use the database, so it can be tested with plain lists.
public static class StatsCalculator
{
    public static StatsDto Calculate(IReadOnlyList<WorkoutDay> workouts, DateOnly today, int? weeks)
    {
        var currentMonday = MondayOf(today);

        // Days without exercises count as rest days.
        var byDate = workouts
            .Where(w => w.ExerciseCount > 0)
            .GroupBy(w => w.Date)
            .ToDictionary(g => g.Key, g => (Count: g.Sum(w => w.ExerciseCount), Done: g.Sum(w => w.DoneCount)));

        // "Összes" without any plan: nothing to show.
        if (weeks == null && byDate.Count == 0)
        {
            return new StatsDto(currentMonday, currentMonday.AddDays(6), 0, 0, 0, 0, 0, 0, []);
        }

        var firstPlanMonday = byDate.Count > 0 ? MondayOf(byDate.Keys.Min()) : currentMonday;
        var rangeStart = weeks == null ? firstPlanMonday : currentMonday.AddDays(-7 * (weeks.Value - 1));

        // Every week from the first plan (or the range start, if earlier) to the current week.
        // Streaks are counted on all of them, the log only shows the selected range.
        var start = rangeStart < firstPlanMonday ? rangeStart : firstPlanMonday;
        var allWeeks = new List<StatsWeekDto>();
        for (var monday = start; monday <= currentMonday; monday = monday.AddDays(7))
        {
            allWeeks.Add(BuildWeek(monday, byDate, today));
        }

        var log = allWeeks.Where(w => w.WeekStart >= rangeStart).ToList();
        var completed = log.Sum(w => w.Completed);
        var planned = log.Sum(w => w.Planned);
        var percent = planned == 0 ? 0 : (int)Math.Round(completed * 100.0 / planned, MidpointRounding.AwayFromZero);

        return new StatsDto(
            rangeStart,
            currentMonday.AddDays(6),
            log.Count,
            CurrentStreak(allWeeks),
            LongestStreak(allWeeks),
            completed,
            planned,
            percent,
            log);
    }

    private static StatsWeekDto BuildWeek(DateOnly monday, Dictionary<DateOnly, (int Count, int Done)> byDate, DateOnly today)
    {
        var days = new List<DayStatus>();
        for (var i = 0; i < 7; i++)
        {
            var date = monday.AddDays(i);
            if (!byDate.TryGetValue(date, out var day))
            {
                days.Add(DayStatus.Rest);
            }
            else if (day.Done >= day.Count)
            {
                days.Add(DayStatus.Done);
            }
            else if (date >= today)
            {
                days.Add(DayStatus.Planned);
            }
            else if (day.Done > 0)
            {
                days.Add(DayStatus.Partial);
            }
            else
            {
                days.Add(DayStatus.Missed);
            }
        }

        // Days still ahead (Planned) are not counted.
        var completed = days.Count(d => d == DayStatus.Done);
        var planned = days.Count(d => d is DayStatus.Done or DayStatus.Partial or DayStatus.Missed);
        return new StatsWeekDto(monday, completed, planned, days);
    }

    private static bool IsComplete(StatsWeekDto week) => week.Planned > 0 && week.Completed == week.Planned;

    // Counted back from the last finished week. The current week only adds to it
    // when everything so far is done; otherwise it is skipped, not a break.
    private static int CurrentStreak(List<StatsWeekDto> weeks)
    {
        var streak = 0;
        var i = weeks.Count - 1;
        if (i >= 0 && IsComplete(weeks[i]))
        {
            streak++;
        }

        for (i--; i >= 0 && IsComplete(weeks[i]); i--)
        {
            streak++;
        }

        return streak;
    }

    private static int LongestStreak(List<StatsWeekDto> weeks)
    {
        var longest = 0;
        var run = 0;
        foreach (var week in weeks)
        {
            run = IsComplete(week) ? run + 1 : 0;
            longest = Math.Max(longest, run);
        }

        return longest;
    }

    private static DateOnly MondayOf(DateOnly date) => date.AddDays(-(((int)date.DayOfWeek + 6) % 7));
}
