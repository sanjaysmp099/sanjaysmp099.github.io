# Tic Tac Toe — Angular + .NET

A browser-based Tic Tac Toe game with an Angular frontend and a .NET Web API
backend that owns all game rules, session state, and the scoreboard.

## Project Overview

- The **backend** (`backend/TicTacToe.Api`) is the single source of truth: it
  validates every move, detects wins/draws, runs the computer opponent, and
  tracks the scoreboard, all in memory for the lifetime of the process.
- The **frontend** (`frontend/`) is a thin Angular client: it renders
  whatever `GameState` the backend returns and never applies game rules
  itself.

## Tech Stack

| Layer      | Technology                                   |
|------------|-----------------------------------------------|
| Frontend   | Angular 17 (standalone components), TypeScript |
| Backend    | .NET 8 Web API, C#                            |
| API style  | REST (JSON)                                   |
| Storage    | In-memory (`ConcurrentDictionary` + singleton scoreboard) |
| Testing    | xUnit (backend), Jasmine/Karma (frontend)      |

## Features Implemented

- 3×3 board, click-to-play, locked cells once filled
- Two Player mode and Play vs Computer mode (computer plays O)
- Turn indicator, alternating turns, invalid moves rejected without changing turn
- Win detection (rows, columns, diagonals) with winning-cell highlighting
- Draw detection
- Move history (move #, player, row/column)
- Undo Last Move, mode-aware:
  - Two Player: removes only the last move
  - Vs Computer: removes the computer's move and the preceding human move together
- Reset Game (board/history/status only — scoreboard untouched)
- Session scoreboard (X wins / O wins / Draws) with a separate Reset Scoreboard action
- Computer opponent priority: win → block → center → corner → any cell

## How to Run the Backend Locally

Requires the [.NET 8 SDK](https://dotnet.microsoft.com/download).

```bash
cd backend/TicTacToe.Api
dotnet run
```

The API listens on **http://localhost:5000** (see `Properties/launchSettings.json`).
Swagger UI is available at `http://localhost:5000/swagger` in Development.

## How to Run the Frontend Locally

Requires Node.js 18+ and npm.

```bash
cd frontend
npm install
npm start
```

This runs `ng serve` with `proxy.conf.json`, which forwards `/api/*` requests
to `http://localhost:5000`, so the Angular dev server (http://localhost:4200)
and the backend can talk to each other with no CORS setup needed on your part
(CORS for `http://localhost:4200` is also configured on the backend directly,
as a second line of defense).

Open **http://localhost:4200** in a browser once both are running.

## API Endpoint Summary

All responses are JSON; enums (`Player`, `Mode`, `Status`) are serialized as
strings (e.g. `"X"`, `"VsComputer"`, `"InProgress"`).

| Method | Endpoint                     | Purpose                                   |
|--------|-------------------------------|--------------------------------------------|
| POST   | `/api/games`                  | Create a new game. Body: `{ "mode": "TwoPlayer" \| "VsComputer" }` |
| GET    | `/api/games/{id}`             | Get current game state                    |
| POST   | `/api/games/{id}/moves`       | Submit a move. Body: `{ "player": "X", "cellIndex": 0 }` (or `row`/`column` instead of `cellIndex`) |
| POST   | `/api/games/{id}/undo`        | Undo last move (or move pair, in Vs Computer mode) |
| POST   | `/api/games/{id}/reset`       | Reset the current game (scoreboard unchanged) |
| GET    | `/api/scoreboard`             | Get the session scoreboard                |
| POST   | `/api/scoreboard/reset`       | Reset the scoreboard to zero              |

**Game state response shape** (`GameStateResponse`):

```json
{
  "id": "a1b2c3",
  "board": ["X", null, "O", null, "X", null, null, null, null],
  "currentPlayer": "O",
  "mode": "TwoPlayer",
  "status": "InProgress",
  "winner": null,
  "winningCells": [],
  "moveHistory": [
    { "moveNumber": 1, "player": "X", "cellIndex": 0, "row": 0, "column": 0 }
  ],
  "canUndo": true,
  "scoreboard": { "xWins": 0, "oWins": 0, "draws": 0 }
}
```

Invalid moves (occupied cell, out-of-range cell, wrong player's turn, move
after completion) return **400 Bad Request** with `{ "message": "..." }`.
An unknown game id returns **404 Not Found**.

## How to Run Tests

**Backend (xUnit):**

```bash
cd backend
dotnet test
```

Covers: valid/invalid moves, turn switching, row/column/diagonal wins, draw,
reset, undo in both modes, undo edge cases (no moves / after completion),
scoreboard update-once semantics, and computer move-selection priority.

**Frontend (Karma/Jasmine):**

```bash
cd frontend
npm test
```

Covers board rendering/interaction (click emits index, disabled cells don't
emit, winning cells are highlighted) and `AppComponent`'s API integration
(game creation, move submission, and error-message handling) via
`HttpClientTestingModule`.

## AI Tools and Prompt Summary

This project was scaffolded with AI assistance (Claude). At a high level:

- **Prompt approach:** the assignment's problem statement was used almost
  directly as the spec, broken down into backend domain model → services →
  controllers → tests, then frontend models → service → components → root
  component, then README.
- **What the AI generated:** the full initial scaffold — C# models, DTOs,
  `GameService`/`ComputerPlayerService`/`ScoreboardService`, controllers,
  xUnit tests, and the Angular standalone components/service/tests.
- **What to review/change manually before submitting as your own work:**
  - Run `dotnet test` and `npm test` locally and fix any environment-specific
    issues (this was scaffolded without a live .NET/Angular toolchain
    available in the authoring sandbox — see *Known Limitations*).
  - Walk through `GameService.Undo` and `ComputerPlayerService.ChooseMove`
    line by line so you can defend the logic unprompted in the panel review.
  - Consider renaming/restructuring anything that doesn't match your usual
    style, and add any extra tests you think are missing.

## Design Decisions

- **Clarification 2 — Option A (Disable Undo After Completion) was chosen.**
  Once `Status` is `Won` or `Draw`, `/undo` returns 400. This keeps the
  scoreboard-update-once invariant trivial to reason about and test, at the
  cost of not letting a player "take back" a finished game.
- **Reset Game reuses the same game id** and only clears board/history/status
  (mode is preserved); it does not mint a brand-new id. `POST /api/games` is
  the way to start a genuinely new session (e.g. when switching modes).
- **Computer move selection** is implemented as a pure function
  (`Mark[] → int`) so it's trivially unit-testable in isolation from session
  state.
- **Enums serialize as strings** (`JsonStringEnumConverter`) so the wire
  format and Swagger docs are human-readable instead of numeric codes.
- **In-memory storage** via a singleton `ConcurrentDictionary<string, GameSession>`
  and a singleton `ScoreboardService`, per the assignment's "in-memory is
  acceptable" storage option. State does not survive a backend restart.

## Clarifications and Assumptions

- A move request may supply either `cellIndex` (0–8) or `row`+`column`
  (0–2 each); the backend normalizes both to a single cell index.
- "Wrong player" validation compares the request's `player` against the
  server's `currentPlayer` — the frontend never decides whose turn it is.
- In Vs Computer mode, the computer's reply to a valid human move is applied
  synchronously within the same `POST /moves` call, so the response already
  reflects both moves (and, if the game ended, the updated scoreboard).

## Known Limitations

- No persistence: restarting the backend clears all games and the scoreboard
  (acceptable per the assignment's in-memory storage option).
- No authentication/multi-user isolation — the scoreboard is a single
  shared, process-wide counter, matching a "local, single-session" scope.
- This scaffold was authored without a live .NET SDK or Angular CLI in the
  generating environment. One issue was found and fixed this way (a missing
  `Swashbuckle.AspNetCore` package reference, needed for `AddSwaggerGen`/
  `UseSwagger`/`UseSwaggerUI` in `Program.cs`, now added to
  `TicTacToe.Api.csproj`). Run `dotnet build`/`dotnet test` and
  `npm install`/`ng serve`/`ng test` locally before submission in case any
  other environment-specific issue surfaces.

## Future Improvements

- SQLite-backed persistence for games/scoreboard across restarts
- WebSocket/SignalR push for a two-browser-tab multiplayer experience
- Configurable computer difficulty (e.g. an unbeatable minimax mode)
- E2E tests (Cypress/Playwright) covering full user flows through both apps
