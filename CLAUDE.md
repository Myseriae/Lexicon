# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Lexicon is a collaborative wiki/lexicon app. Users can create articles (with Markdown content), tag them, and invite collaborators. Articles maintain a full revision history on each update. Article summaries are auto-fetched from the Wikipedia REST API when not provided.

**Stack:** ASP.NET Core (.NET 10) backend · React 19 + Vite frontend · SQL Server (via Docker)

## Development Commands

### Running the full stack (recommended)
```bash
# Requires a JWT_SIGNING_KEY in .env (already present)
docker compose up
# Backend → http://localhost:5149
# Frontend → http://localhost:5173
```

### Running locally (without Docker)

**Start SQL Server first** (Docker must be running):
```bash
docker compose up db
```

**Backend** — requires the JWT signing key in user-secrets:
```bash
dotnet user-secrets set "Jwt:IssuerSigningKey" "<your-secret>" --project backend
cd backend && dotnet run
# http://localhost:5149
```

**Frontend:**
```bash
cd frontend && npm install && npm run dev
# http://localhost:5173
```

### Tests
```bash
dotnet test LexiconTest/          # run all tests
dotnet test LexiconTest/ --filter "FullyQualifiedName~ArticleService"  # single test class
```

Tests use NUnit + Moq. Test project is `LexiconTest/` and references the backend project directly.

### Frontend linting
```bash
cd frontend && npm run lint
```

### EF Core migrations (run from repo root)
```bash
dotnet ef migrations add <MigrationName> --project backend --startup-project backend
dotnet ef database update --project backend --startup-project backend
```

## Architecture

### Backend (`backend/`)

Layered architecture: **Controllers → Services → Repositories → EF Core → SQL Server**

```
Controllers/          HTTP layer — maps routes, extracts claims, calls services
Services/             Business logic (IArticleService, IWikipediaService)
Services/Authentication/  Auth-specific logic (IAuthService, ITokenService, AuthSeeder)
Data/                 Repository interfaces + EF Core implementations
Model/                Domain entities (Article, Revision, Tag, ArticleCollaborator, RefreshToken)
DTOs/                 Request/response shapes
Migrations/           EF Core migration history
```

**Key design decisions:**
- `ArticleService` delegates authorization to `CanEditArticle()`: Admins can edit everything; Editors can only edit their own articles or articles they are a collaborator on.
- On every `UpdateArticle`, the old content is saved to `Revisions` before being overwritten.
- `WikipediaService` calls the Wikipedia REST API (`/api/rest_v1/page/summary/{title}`) and falls back silently on failure; disambiguation pages return `null`.
- `LexiconDbContext` extends `IdentityDbContext` — ASP.NET Identity tables and app tables live in the same DB.

**Auth flow:**
- Access token: short-lived JWT (15 min), returned in the response body.
- Refresh token: cryptographically random, stored in the `RefreshTokens` table, sent as an `httpOnly` cookie scoped to `/api/auth`. Rotated (old one revoked, new one issued) on every `/api/auth/refresh` call.
- `AuthSeeder` creates the `Admin` and `Editor` roles on startup. New registrations are assigned the `Editor` role.
- `Jwt:IssuerSigningKey` must be provided at runtime (user-secrets locally, env var in Docker via `.env`).

**Roles:** `Admin` · `Editor` (default) · `Guest` (defined but not currently assigned)

**API base routes:**
- `GET/POST/PUT/DELETE /api/articles` — article CRUD + search + collaborator management
- `GET /api/articles/search?query=` — title/content search
- `GET /api/articles/{id}/revisions` — revision history
- `POST/DELETE /api/articles/{id}/collaborators/{userId}` — collaborator management
- `POST /api/auth/register|login|refresh|logout` · `GET /api/auth/verify`
- `GET/POST/DELETE /api/tags`

### Frontend (`frontend/src/`)

React SPA with React Router. No global state library — auth state lives in `AuthContext`.

```
api/httpClient.js     Base fetch wrapper; handles auth headers and auto-retry on 401
api/authApi.js        Auth-specific API calls
api/api.js            Article/tag API calls
auth/tokenStore.js    In-memory access token store with pub/sub (NOT localStorage)
context/AuthContext.jsx  Auth state, session restore on load, refresh handler wiring
utils/jwtUtils.js     JWT decode (used to extract currentUser from the access token)
pages/                Route-level components (HomePage, ArticlePage, CreatePage, LoginPage, RegisterPage)
components/           Shared UI (Navbar, Modal, SpotlightCard, CardNav, Collaborators, LightRays)
```

**Auth pattern:**
- Access token is kept in `tokenStore.js` (module-level variable — survives re-renders, cleared on page reload).
- On app load, `AuthContext` calls `POST /api/auth/refresh` with the cookie to restore the session silently.
- `httpClient.js` automatically retries any 401 response once after calling the registered refresh handler — this enables transparent token rotation without per-call handling.
- The refresh token cookie is httpOnly and scoped to `/api/auth`, so JS cannot read it.

**Routing:** `/` · `/create` · `/article/:id` · `/login` · `/register`

**Notable dependencies:** `@uiw/react-md-editor` (Markdown editor), `react-markdown` (rendering), `gsap` + `ogl` (LightRays WebGL background animation).