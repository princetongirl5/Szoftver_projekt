# Okos Otthoni Edzésterv Generáló Alkalmazás (C#ardio)

Created by: ILMS

## Getting started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download) (on macOS: `brew install --cask dotnet-sdk`)
- [Node.js](https://nodejs.org) 22 or newer (npm is installed with it)

### Backend (ASP.NET Core)

```bash
cd backend
dotnet run --project src/EdzestervGenerator.Api
```

The API runs at http://localhost:5013.
In Rider: open `backend/EdzestervGenerator.sln` and start the `http` run configuration.

### Frontend (Angular)

```bash
cd frontend
npm install
npm start
```

The app is available at http://localhost:4200.
`npm install` is only needed after cloning and whenever the dependencies change.

Requests to `/api` are forwarded to the backend, so start the backend as well.

### Tests

```bash
cd backend && dotnet test
cd frontend && npm test
```

## Workflow

- Do not commit directly to `main`. Create a branch, push it, and open a pull request.
- One task = one issue = one branch = one pull request. Keep them small.
- Every pull request needs one approval before merging. Use "Squash and merge".
- Commit messages are in English and follow [Conventional Commits](https://www.conventionalcommits.org), for example `feat: add calendar view`.
