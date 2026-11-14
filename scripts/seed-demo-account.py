#!/usr/bin/env python3
"""Creates a demo account with a few weeks of history, so the stats page has data.

The backend must be running. The account is created through the API (real Identity
password hash, real generator, saved preferences); the past weeks are copies of this
week's plan written straight into the SQLite database, because the API only creates
the current week. This week's days before today are ticked through the API.

    python3 scripts/seed-demo-account.py
    python3 scripts/seed-demo-account.py --email demo2@cardio.hu --weeks 8

Run it again on a fresh database (the email must not exist yet). Standard library only.
"""

import argparse
import http.cookiejar
import json
import sqlite3
import sys
import urllib.error
import urllib.request
from datetime import date, datetime, timedelta, timezone
from pathlib import Path
from zoneinfo import ZoneInfo

ROOT = Path(__file__).resolve().parent.parent
DEFAULT_DB = ROOT / "backend/src/EdzestervGenerator.Api/edzesterv.db"
BUDAPEST = ZoneInfo("Europe/Budapest")

# What the demo account answered in the wizard: muscle building at home, three days.
REQUEST = {
    "goal": "Hypertrophy",
    "location": "Home",
    "equipmentIds": [1, 2, 4],  # Kézisúlyzó, Kettlebell, Gumiszalag
    "days": ["Monday", "Wednesday", "Friday"],
    "sessionMinutes": 60,
    "difficulty": "Medium",
}


def api(opener, base, method, path, body=None):
    data = None if body is None else json.dumps(body).encode()
    request = urllib.request.Request(
        base + path,
        data=data,
        method=method,
        headers={"Content-Type": "application/json", "Accept-Language": "hu"},
    )
    try:
        with opener.open(request) as response:
            text = response.read().decode()
            return json.loads(text) if text else None
    except urllib.error.HTTPError as error:
        sys.exit(f"{method} {path}: {error.code} {error.read().decode()[:300]}")
    except urllib.error.URLError as error:
        sys.exit(f"The backend is not reachable at {base} ({error.reason}). Start it first.")


def local_midnight_utc(day: date) -> str:
    """How the backend stores a workout date: local midnight in Budapest, as UTC."""
    local = datetime(day.year, day.month, day.day, tzinfo=BUDAPEST)
    return local.astimezone(timezone.utc).strftime("%Y-%m-%d %H:%M:%S")


def utc_to_local_day(value: str) -> date:
    utc = datetime.strptime(value[:19], "%Y-%m-%d %H:%M:%S").replace(tzinfo=timezone.utc)
    return utc.astimezone(BUDAPEST).date()


def actual_for(weeks_back: int, reps: int, weight):
    """A steady progression towards today: a little lighter (or fewer reps) each week back."""
    if weight is None:
        return max(1, reps + 2 - weeks_back // 2), None
    return reps, max(1.0, round((weight + 2 - 1.0 * weeks_back) * 2) / 2)


def main():
    parser = argparse.ArgumentParser(description=__doc__.splitlines()[0])
    parser.add_argument("--api", default="http://localhost:5013")
    parser.add_argument("--db", default=str(DEFAULT_DB))
    parser.add_argument("--email", default="demo@cardio.hu")
    parser.add_argument("--password", default="cardio2026")
    parser.add_argument("--weeks", type=int, default=6, help="past weeks to fill")
    args = parser.parse_args()

    if not Path(args.db).exists():
        sys.exit(f"No database at {args.db}. Start the backend once, it creates it.")

    jar = http.cookiejar.CookieJar()
    opener = urllib.request.build_opener(urllib.request.HTTPCookieProcessor(jar))
    credentials = {"email": args.email, "password": args.password}

    api(opener, args.api, "POST", "/api/auth/register", credentials)
    api(opener, args.api, "POST", "/api/auth/login?useCookies=true", credentials)
    week = api(opener, args.api, "POST", "/api/plans/generate", REQUEST)
    print(f"Registered {args.email} and generated this week's plan.")

    today = datetime.now(BUDAPEST).date()
    monday = today - timedelta(days=today.weekday())

    # This week: tick the workout days before today, with the actual values.
    ticked = 0
    for index, day in enumerate(week["days"]):
        if monday + timedelta(days=index) >= today:
            continue
        for exercise in day["exercises"]:
            reps, weight = actual_for(0, exercise["reps"], exercise["weightKg"])
            api(opener, args.api, "PATCH", f"/api/plans/exercises/{exercise['id']}",
                {"isDone": True, "actualReps": reps, "actualWeightKg": weight})
            ticked += 1

    # Past weeks: copies of this week's workouts, mostly done, one partial and one missed day.
    db = sqlite3.connect(args.db)
    user_id = db.execute(
        "SELECT Id FROM Users WHERE NormalizedEmail = ?", (args.email.upper(),)
    ).fetchone()[0]
    workouts = db.execute(
        "SELECT Id, Date, DurationMinutes, Intensity FROM Workouts "
        "WHERE UserId = ? AND Activity = 0 ORDER BY Date",
        (user_id,),
    ).fetchall()
    sessions = 0
    for weeks_back in range(1, args.weeks + 1):
        for position, (workout_id, stored, minutes, intensity) in enumerate(workouts):
            day = utc_to_local_day(stored) - timedelta(weeks=weeks_back)
            missed = weeks_back == 2 and position == len(workouts) - 1
            partial = weeks_back == 4 and position == 1
            cursor = db.execute(
                "INSERT INTO Workouts (UserId, Date, Activity, DurationMinutes, Intensity, Note) "
                "VALUES (?, ?, 0, ?, ?, NULL)",
                (user_id, local_midnight_utc(day), minutes, intensity),
            )
            rows = db.execute(
                'SELECT ExerciseId, "Order", Sets, Reps, WeightKg FROM WorkoutExercises '
                "WHERE WorkoutId = ? ORDER BY \"Order\"",
                (workout_id,),
            ).fetchall()
            for exercise_id, order, sets, reps, weight in rows:
                done = not missed and not (partial and order >= len(rows) // 2)
                actual_reps, actual_weight = actual_for(weeks_back, reps, weight)
                db.execute(
                    'INSERT INTO WorkoutExercises (WorkoutId, ExerciseId, "Order", Sets, Reps, '
                    "WeightKg, IsDone, ActualReps, ActualWeightKg) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)",
                    (cursor.lastrowid, exercise_id, order, sets, reps, weight, int(done),
                     actual_reps if done else None, actual_weight if done else None),
                )
            sessions += 1
    db.commit()

    print(f"Ticked {ticked} exercises this week, added {sessions} workouts in {args.weeks} past weeks.")
    print(f"Log in with {args.email} / {args.password}")


if __name__ == "__main__":
    main()
