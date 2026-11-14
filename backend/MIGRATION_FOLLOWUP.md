# EF Core migration follow-up

The checked-in `AddIdentityPreferencesAndWorkoutLogging` migration is a hand-authored
migration and currently has no generated `.Designer.cs`; the model snapshot must be
regenerated from the actual EF model. Do not delete the migration or hand-edit the
snapshot as a substitute for scaffolding.

With the .NET 10 SDK installed, install the matching EF command-line tool and run these commands from `backend/`:

```bash
dotnet tool install --global dotnet-ef --version 10.0.13
dotnet ef migrations remove --project src/EdzestervGenerator.Api --startup-project src/EdzestervGenerator.Api
dotnet ef migrations add AddIdentityPreferencesAndWorkoutLogging --project src/EdzestervGenerator.Api --startup-project src/EdzestervGenerator.Api
dotnet test EdzestervGenerator.sln --configuration Release
```

Only use `migrations remove` if this migration has not been applied to a database
that must be preserved. For a shared or already-deployed database, create a new
corrective migration instead of rewriting migration history. Review the generated
`.cs`, `.Designer.cs`, and `AppDbContextModelSnapshot.cs` before committing.
