# Stats API: proposal from the backend

Status: **proposal**, open for changes. It covers the "Statisztika" screen
(`docs/ui/screens/08_Stats.png`) except the strength chart (see open
question 1). Tell us before we start if you want a different shape.

Conventions, same as the rest of the API: JSON in camelCase, enums as strings,
validation errors as `ValidationProblem()` (400 with field errors). Like the
plan API, it uses the default user (id 1) and weeks run Monday to Sunday in
Europe/Budapest time.

No new tables: everything is calculated from the generated plans that are
already stored (`Workout` with `Activity = Generated` and its
`WorkoutExercise.IsDone` ticks).

## Endpoint

| Method | Path | What | Who uses it |
|---|---|---|---|
| `GET` | `/api/stats?weeks=12` | Workout log, streaks and completion | stats screen |

`weeks` is optional: `4` and `12` match the "4 hét" and "12 hét" buttons, no
value means "Összes" (from the first week that has a plan). The range always
ends with the current week. **400** if `weeks` is smaller than 1 or larger
than 104.

## Response

**200**, also when there are no plans yet (then `log` is empty and every
number is 0).

```json
{
  "from": "2026-08-03",
  "to": "2026-10-25",
  "weeks": 12,
  "currentStreakWeeks": 5,
  "longestStreakWeeks": 5,
  "completedWorkouts": 46,
  "plannedWorkouts": 48,
  "completionPercent": 96,
  "log": [
    {
      "weekStart": "2026-08-03",
      "completed": 4,
      "planned": 4,
      "days": ["Done", "Done", "Rest", "Done", "Done", "Rest", "Rest"]
    }
  ]
}
```

| Field | Notes |
|---|---|
| `from`, `to` | First Monday and last Sunday of the range, `yyyy-MM-dd`, Budapest dates. For the header ("AUG. 3 - OKT. 25 · 12 HÉT") |
| `weeks` | Number of rows in `log` |
| `log` | One row per week, **oldest first** (the screen shows aug. 3 at the top). Weeks without a plan are included with `planned: 0` and seven `Rest` days, so the grid has no gaps |
| `log[].completed` / `planned` | The "4/4" at the end of the row. Days in the future are not counted |
| `log[].days` | Always 7 values, Monday first |
| `completedWorkouts` / `plannedWorkouts` | Totals for the range, "46 / 48 edzés" |
| `completionPercent` | `completed / planned`, rounded to a whole number, 0 when nothing was planned |
| `currentStreakWeeks` | "Jelenlegi sorozat": full weeks in a row, counted back from the last finished week, where every planned workout is `Done`. The current week counts too once all its workouts so far are done. A week with `planned: 0` ends the streak |
| `longestStreakWeeks` | "Leghosszabb": the longest such run in the range |

### Day status

| Value | When | Colour on the screen |
|---|---|---|
| `Done` | Every exercise of that day's workout is ticked | Edzés kész (green) |
| `Partial` | Some, but not all exercises are ticked | Hiányos edzés (orange) |
| `Missed` | A past day with a workout and no ticks | Kimaradt (red) |
| `Rest` | No workout planned that day | Pihenőnap (grey) |
| `Planned` | Today or later, not fully ticked yet | New: not on the mockup, suggest the rest-day colour with a border |

`Planned` is needed for the current week: without it, the days still ahead
would show as missed.

## Open questions

1. **"Erőfejlődés" (estimated 1RM) is left out for now.** The data would be
   there (`WorkoutExercise.WeightKg`, `Reps`, `IsDone`), but the weight is the
   generator's suggestion: there is no way yet for the user to enter what they
   really lifted, so the chart would only show the generator, not progress.
   It needs a way to change the weight of an exercise first (for example
   `weightKg` in the existing `PATCH /api/plans/exercises/{id}`). Fine to do it
   later?
2. **Regenerating a plan** deletes the current week's generated workouts and
   their ticks (plan API). So the stats only remember the last plan of each
   week. That seems fine for the demo; say so if not.
3. **Weeks without a plan** are shown as empty rows and end the streak. The
   other option is to skip them in the log. Which one?
