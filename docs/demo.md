# Demo guide

How to set up a machine for a C#ardio demo, and a short script to follow.
UI labels are quoted in Hungarian, as they appear in the app.

## Preparation (about 30 minutes before)

Needs: .NET 10 SDK, Node.js with npm, Python 3, Chrome.

1. Get the latest code:
   ```bash
   git switch main && git pull
   cd frontend && npm install && cd ..
   ```
2. Start from an empty database:
   ```bash
   rm -f backend/src/EdzestervGenerator.Api/edzesterv.db
   ```
3. Start the backend (terminal 1) and wait until it listens on `:5013`:
   ```bash
   cd backend && dotnet run --project src/EdzestervGenerator.Api
   ```
4. Create the demo account (terminal 2, backend must run):
   ```bash
   python3 scripts/seed-demo-account.py
   ```
   This registers `demo@cardio.hu` / `cardio2026` with a plan for this week
   and 6 past weeks of history. If it says the email exists, step 2 did not
   delete the database.
5. Start the frontend (terminal 3):
   ```bash
   cd frontend && npm start
   ```
6. Browser: light theme, zoom 110-125%, open `http://localhost:4200/login`.
   Keep DevTools ready with the iPhone device preview.
7. Quick check: log in as the demo account, open "Heti terv" and
   "Statisztika", then log out.

Things to know:

- Every page needs the backend. If it stops, the app sends you to `/login`.
- The week runs Monday to Sunday (Budapest time): generate the live plan on
  the demo day, not the day before.
- Fallback: the slides in `docs/presentation/cardio_bemutato.pptx` and the
  backup recording.

## Script (about 10 minutes)

1. **Intro (1').** The problem: a weekly workout plan that fits your goal,
   equipment and free days.
2. **Register (1').** Create a new account on `/register`. A password without
   a digit shows the Hungarian error message.
3. **Wizard (2').** Pick a goal, "Otthon" with a few items, three days, a
   length and a level. Show the "Miért kérdezzük?" panel, the disabled
   "Tovább" button, and a row on "Összegzés" jumping back. Generate the plan.
4. **Dashboard (2').** Tick a few exercises, change a weight, reload: it is
   kept. Show the phone view with the bottom tab bar.
5. **Stats (2').** Log out, log in as `demo@cardio.hu`: it does not see the
   other account's plan. Show the week log, the streak and an exercise with
   rising weights (e.g. "Evezés kézisúlyzóval").
6. **Backend and teamwork (2').** Accounts with ASP.NET Core Identity, the
   generator rules, the tests; Figma, Trello, pull requests with review,
   Conventional Commits.
