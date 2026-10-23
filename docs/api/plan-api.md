# Plan API: proposal from the frontend

Status: **proposal**, open for changes. The frontend works against this shape
with a mock (`frontend/src/app/core/models/plan.ts`). Two small additions on
our side land before Sunday (the exercise `id` and the PATCH call); after
that, connecting the two is a one-line switch in `app.config.ts`. If you want
something different, tell us before Sunday and we change the TypeScript types
to match.

Conventions, same as the existing API: JSON in camelCase, enums as strings
(`JsonStringEnumConverter` is already on), validation errors as
`ValidationProblem()` (400 with field errors).

## Endpoints

| Method | Path | What | Who uses it |
|---|---|---|---|
| `POST` | `/api/plans/generate` | Generates this week's plan from the wizard's answers | wizard |
| `GET` | `/api/plans/current` | This week's plan | dashboard |
| `PATCH` | `/api/plans/exercises/{id}` | Marks one exercise done or not done | dashboard |
| `GET` | `/api/equipment` | Already exists (home equipment chips) | wizard |

No `userId` in these routes: there is no user management, so the backend uses
one default user internally (id 1). **This user is not seeded yet**: only
`Equipment` has seed data today. The existing `/api/users/...` routes can stay
as they are.

## 1. `POST /api/plans/generate`

Request:

```json
{
  "goal": "Hypertrophy",
  "location": "Home",
  "equipmentIds": [1, 3],
  "days": ["Monday", "Tuesday", "Thursday", "Friday"],
  "sessionMinutes": 60,
  "difficulty": "Medium"
}
```

| Field | Values | Notes |
|---|---|---|
| `goal` | `Strength`, `Hypertrophy`, `Endurance`, `General` | Decides reps and rest (see "Generation" below) |
| `location` | `Gym`, `Home` | `Gym`: every exercise is allowed; `equipmentIds` is sent empty and ignored |
| `equipmentIds` | ids from `GET /api/equipment` | Only for `Home`. May be empty: bodyweight only |
| `days` | `Monday` ... `Sunday` | 2 to 6 distinct days, in week order |
| `sessionMinutes` | `45`, `60`, `75`, `90` | Decides how many exercises fit; also stored as `Workout.DurationMinutes` |
| `difficulty` | `Easy`, `Medium`, `Hard` | The existing `Difficulty` enum. UI: Kezdő, Haladó, Profi |

Response: **201** with the week plan (same shape as `GET /api/plans/current`).

Which week: the current week, Monday to Sunday, in Europe/Budapest time. Each
training day is one `Workout` with `Activity = Generated` and `Date` = that
day (stored as UTC, like today). Generating again deletes this week's
`Generated` workouts (and their exercises) and creates new ones; other
activities are not touched.

If there are not enough matching exercises for a day (for example `Home`
with no equipment), return fewer exercises instead of an error.

Errors: **400** if `days` has fewer than 2 or more than 6 items or duplicates,
if an enum value is unknown, if `sessionMinutes` is not one of the four
values, or if an `equipmentIds` item does not exist.

## 2. `GET /api/plans/current`

Response: **200** with the week plan, or **404** if nothing was generated for
this week yet (the frontend then sends the user to the wizard).

```json
{
  "days": [
    {
      "day": "Monday",
      "workoutName": "Felsőtest A",
      "exercises": [
        {
          "id": 101,
          "name": "Bench Press",
          "muscleGroup": "Chest",
          "sets": 4,
          "reps": 8,
          "weightKg": 60,
          "isDone": false
        }
      ]
    },
    { "day": "Wednesday", "workoutName": null, "exercises": [] }
  ]
}
```

| Field | Notes |
|---|---|
| `days` | Always all 7 days, Monday first (C# `DayOfWeek` starts at Sunday, so sort explicitly). A rest day has `workoutName: null` and no exercises |
| `workoutName` | Hungarian, shown as is: `Felsőtest A`, `Alsótest A`, `Teljes test`, ... **Needs a new `Workout.Name` column** (migration) |
| `exercises` | In `WorkoutExercise.Order` |
| `exercises[].id` | The `WorkoutExercise` id, needed for the PATCH below |
| `name` | The wger name, English is fine |
| `muscleGroup` | Text. See open question 3: today this would be the `MuscleGroup` enum (`UpperBody`, `LowerBody`, ...) |
| `weightKg` | `null` for bodyweight exercises |

The existing tables hold almost all of this: one `Workout` per training day
(`Activity = Generated`, `DurationMinutes`, `Intensity`), its exercises with
`Order`, `Sets`, `Reps`, `WeightKg`, `IsDone`. Missing: the workout's name.
`Intensity` is required: suggest Easy = Low, Medium = Medium, Hard = High.

## 3. `PATCH /api/plans/exercises/{id}`

Request: `{ "isDone": true }`. Response: **204**. **404** if the id does not
exist.

## Generation: a suggestion, not a requirement

Only so the demo looks sensible; change anything you like.

- **Split by number of days:** 2-3 days full body; 4 days upper / lower
  alternating (`Felsőtest A`, `Alsótest A`, `Felsőtest B`, `Alsótest B`);
  5-6 days by muscle group. Every muscle group gets at least 2 days of rest
  before it is trained again.
- **Goal:** Strength 3-6 reps, Hypertrophy 8-12, Endurance 15-20, General
  10-12.
- **Session length:** 45 min = 4 exercises, 60 = 5, 75 = 6, 90 = 7.
- **Difficulty:** sets per exercise, Easy 3, Medium 3-4, Hard 4-5. The
  existing `Exercise.Difficulty` can also filter out exercises that are too
  hard for `Easy`.
- **Equipment:** `Home` allows an exercise when its `EquipmentId` is in
  `equipmentIds`, or it is `null` (bodyweight). `Gym` allows everything.
- **Weights:** a rough starting value (`Exercise.MinWeightKg` is there), or
  `null`; the user adjusts it.

## Open questions

1. **Gym equipment** is not in the seed yet (wger: barbell, SZ-bar, incline
   bench, Swiss ball). Needed so `Gym` plans have barbell exercises. But
   `GET /api/equipment` feeds the wizard's **home** chips: if gym equipment
   goes into the same table, it needs a flag (for example `IsHome`) or a
   filter (`GET /api/equipment?location=Home`), otherwise "Rúd" shows up as a
   home option.
2. Is one default user (id 1) fine for you, instead of `userId` in the routes?
   If yes, it needs a seed row.
3. **Muscle groups:** `Exercise.MuscleGroup` is `UpperBody`, `LowerBody`,
   `Core`, `Mobility`, `FullBody`. That is enough for full body and
   upper / lower, but a 5-6 day split "by muscle group" needs finer groups
   (the wger categories: Chest, Back, Legs, Shoulders, Arms, Abs, Calves).
   Keep the enum and split 5-6 days differently, or import the wger category
   too? Either way, tell us which strings `muscleGroup` will contain.
4. The calendar endpoints are out of scope now (no calendar in the demo). They
   can stay, the frontend just won't call them.