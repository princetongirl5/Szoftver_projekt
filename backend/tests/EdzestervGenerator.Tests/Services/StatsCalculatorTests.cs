using EdzestervGenerator.Api.DTOs;
using EdzestervGenerator.Api.Services;

namespace EdzestervGenerator.Tests.Services;

public sealed class StatsCalculatorTests
{
    // Wednesday, so the current week (from 2026-11-09) has past days, today and days ahead.
    private static readonly DateOnly Today = new(2026, 11, 11);

    [Fact]
    public void No_plans_and_all_weeks_returns_empty_log()
    {
        var stats = StatsCalculator.Calculate([], Today, weeks: null);

        Assert.Empty(stats.Log);
        Assert.Equal(0, stats.Weeks);
        Assert.Equal(0, stats.PlannedWorkouts);
        Assert.Equal(0, stats.CompletionPercent);
        Assert.Equal(new DateOnly(2026, 11, 9), stats.From);
        Assert.Equal(new DateOnly(2026, 11, 15), stats.To);
    }

    [Fact]
    public void No_plans_with_weeks_returns_empty_rest_weeks()
    {
        var stats = StatsCalculator.Calculate([], Today, weeks: 4);

        Assert.Equal(4, stats.Log.Count);
        Assert.Equal(new DateOnly(2026, 10, 19), stats.From);
        Assert.All(stats.Log, w => Assert.Equal(0, w.Planned));
        Assert.All(stats.Log.SelectMany(w => w.Days), d => Assert.Equal(DayStatus.Rest, d));
    }

    [Fact]
    public void Past_days_get_done_partial_missed_and_rest()
    {
        var stats = StatsCalculator.Calculate(
        [
            W("2026-11-02", 4, 4), // Monday: all ticked
            W("2026-11-03", 4, 1), // Tuesday: some ticked
            W("2026-11-05", 4, 0), // Thursday: nothing ticked
        ], Today, weeks: 2);

        var week = stats.Log[0];
        Assert.Equal(new DateOnly(2026, 11, 2), week.WeekStart);
        Assert.Equal(
            [DayStatus.Done, DayStatus.Partial, DayStatus.Rest, DayStatus.Missed, DayStatus.Rest, DayStatus.Rest, DayStatus.Rest],
            week.Days);
        Assert.Equal(1, week.Completed);
        Assert.Equal(3, week.Planned);
    }

    [Fact]
    public void Today_and_later_are_planned_and_not_counted()
    {
        var stats = StatsCalculator.Calculate(
        [
            W("2026-11-11", 4, 2), // today, half done
            W("2026-11-13", 4, 0), // Friday
        ], Today, weeks: 1);

        var week = stats.Log[0];
        Assert.Equal(DayStatus.Planned, week.Days[2]);
        Assert.Equal(DayStatus.Planned, week.Days[4]);
        Assert.Equal(0, week.Planned);
        Assert.Equal(0, stats.PlannedWorkouts);
    }

    [Fact]
    public void Today_fully_ticked_counts_as_done()
    {
        var stats = StatsCalculator.Calculate([W("2026-11-11", 4, 4)], Today, weeks: 1);

        Assert.Equal(DayStatus.Done, stats.Log[0].Days[2]);
        Assert.Equal(1, stats.Log[0].Completed);
        Assert.Equal(1, stats.Log[0].Planned);
    }

    [Fact]
    public void Day_without_exercises_is_a_rest_day()
    {
        var stats = StatsCalculator.Calculate([W("2026-11-02", 0, 0)], Today, weeks: 2);

        Assert.Equal(DayStatus.Rest, stats.Log[0].Days[0]);
        Assert.Equal(0, stats.PlannedWorkouts);
    }

    [Fact]
    public void Completion_percent_is_rounded_and_only_counts_the_range()
    {
        var stats = StatsCalculator.Calculate(
        [
            W("2026-10-12", 4, 0), // outside a 2-week range
            W("2026-11-02", 4, 4),
            W("2026-11-03", 4, 4),
            W("2026-11-05", 4, 0),
        ], Today, weeks: 2);

        Assert.Equal(2, stats.CompletedWorkouts);
        Assert.Equal(3, stats.PlannedWorkouts);
        Assert.Equal(67, stats.CompletionPercent);
    }

    [Fact]
    public void Log_is_oldest_first_and_all_weeks_start_at_the_first_plan()
    {
        var stats = StatsCalculator.Calculate([W("2026-10-21", 4, 4)], Today, weeks: null);

        Assert.Equal(4, stats.Weeks);
        Assert.Equal(new DateOnly(2026, 10, 19), stats.From);
        Assert.Equal(new DateOnly(2026, 10, 19), stats.Log[0].WeekStart);
        Assert.Equal(new DateOnly(2026, 11, 9), stats.Log[^1].WeekStart);
    }

    [Fact]
    public void Current_streak_skips_an_unfinished_current_week()
    {
        var stats = StatsCalculator.Calculate(
        [
            W("2026-10-26", 4, 4),
            W("2026-11-02", 4, 4),
            W("2026-11-09", 4, 0), // this Monday: missed
        ], Today, weeks: 4);

        Assert.Equal(2, stats.CurrentStreakWeeks);
    }

    [Fact]
    public void Current_streak_includes_the_current_week_when_everything_so_far_is_done()
    {
        var stats = StatsCalculator.Calculate(
        [
            W("2026-11-02", 4, 4),
            W("2026-11-09", 4, 4),
            W("2026-11-13", 4, 0), // Friday is still ahead
        ], Today, weeks: 4);

        Assert.Equal(2, stats.CurrentStreakWeeks);
    }

    [Fact]
    public void Week_without_plan_breaks_the_streak()
    {
        var stats = StatsCalculator.Calculate(
        [
            W("2026-10-12", 4, 4),
            W("2026-10-19", 4, 4),
            W("2026-10-26", 4, 4),
            // no plan in the week of 2026-11-02
        ], Today, weeks: null);

        Assert.Equal(0, stats.CurrentStreakWeeks);
        Assert.Equal(3, stats.LongestStreakWeeks);
    }

    [Fact]
    public void Streaks_count_weeks_outside_the_selected_range()
    {
        var stats = StatsCalculator.Calculate(
        [
            W("2026-10-12", 4, 4),
            W("2026-10-19", 4, 4),
            W("2026-10-26", 4, 4),
            W("2026-11-02", 4, 4),
        ], Today, weeks: 2);

        Assert.Equal(2, stats.Log.Count);
        Assert.Equal(4, stats.CurrentStreakWeeks);
        Assert.Equal(4, stats.LongestStreakWeeks);
    }

    private static WorkoutDay W(string date, int exercises, int done) =>
        new(DateOnly.Parse(date), exercises, done);
}
