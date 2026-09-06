using System.Collections.Concurrent;
using TicTacToe.Api.Models;

namespace TicTacToe.Api.Services;

public interface IGameService
{
    GameSession CreateGame(GameMode mode);
    GameSession GetGame(string id);
    GameSession MakeMove(string id, Mark player, int cellIndex);
    GameSession Undo(string id);
    GameSession Reset(string id);
}

/// <summary>
/// Owns all game session state for the process (in-memory storage — acceptable
/// per the assignment's storage requirements). Thread-safe via ConcurrentDictionary
/// and per-session locking, since ASP.NET Core may process requests concurrently.
/// </summary>
public class GameService : IGameService
{
    private readonly ConcurrentDictionary<string, GameSession> _games = new();
    private readonly IScoreboardService _scoreboardService;
    private readonly IComputerPlayerService _computerPlayerService;

    public GameService(IScoreboardService scoreboardService, IComputerPlayerService computerPlayerService)
    {
        _scoreboardService = scoreboardService;
        _computerPlayerService = computerPlayerService;
    }

    public GameSession CreateGame(GameMode mode)
    {
        var session = new GameSession
        {
            Mode = mode,
            Board = new Mark[9],
            CurrentPlayer = Mark.X,
            Status = GameStatus.InProgress
        };
        _games[session.Id] = session;
        return session;
    }

    public GameSession GetGame(string id)
    {
        if (!_games.TryGetValue(id, out var session))
        {
            throw new GameNotFoundException(id);
        }
        return session;
    }

    public GameSession MakeMove(string id, Mark player, int cellIndex)
    {
        var session = GetGame(id);

        lock (session)
        {
            if (session.Status != GameStatus.InProgress)
            {
                throw new GameRuleException("Move rejected: the game has already been completed.");
            }

            if (cellIndex < 0 || cellIndex > 8)
            {
                throw new GameRuleException("Move rejected: cell index is outside the board (must be 0-8).");
            }

            if (session.Board[cellIndex] != Mark.Empty)
            {
                throw new GameRuleException("Move rejected: that cell is already occupied.");
            }

            if (player != session.CurrentPlayer)
            {
                throw new GameRuleException($"Move rejected: it is currently {session.CurrentPlayer}'s turn.");
            }

            ApplyMove(session, player, cellIndex);

            // Auto-play the computer's turn if applicable.
            if (session.Mode == GameMode.VsComputer &&
                session.Status == GameStatus.InProgress &&
                session.CurrentPlayer == Mark.O)
            {
                var computerCell = _computerPlayerService.ChooseMove(session.Board);
                if (computerCell != -1)
                {
                    ApplyMove(session, Mark.O, computerCell);
                }
            }

            return session;
        }
    }

    public GameSession Undo(string id)
    {
        var session = GetGame(id);

        lock (session)
        {
            if (session.Moves.Count == 0)
            {
                throw new GameRuleException("Undo rejected: there are no moves to undo.");
            }

            // Clarification 2 / Option A: once a game is completed, Undo is disabled
            // and the scoreboard result for that game is final. See README.
            if (session.Status != GameStatus.InProgress)
            {
                throw new GameRuleException("Undo rejected: the game has already been completed.");
            }

            // Two Player Mode: remove only the most recent move.
            // Computer Mode: remove the computer's move together with the human's
            // preceding move, so control returns to the human player.
            int removeCount = session.Mode == GameMode.VsComputer
                ? Math.Min(2, session.Moves.Count)
                : 1;

            session.Moves.RemoveRange(session.Moves.Count - removeCount, removeCount);

            ReplayFromMoveHistory(session);

            return session;
        }
    }

    public GameSession Reset(string id)
    {
        var session = GetGame(id);

        lock (session)
        {
            session.Board = new Mark[9];
            session.Moves.Clear();
            session.Status = GameStatus.InProgress;
            session.Winner = null;
            session.WinningCells.Clear();
            session.CurrentPlayer = Mark.X;
            session.ScoreboardUpdated = false;
            // Mode is intentionally preserved: Reset Game starts a fresh game
            // in the same mode. Scoreboard is intentionally left untouched.
            return session;
        }
    }

    /// <summary>
    /// Applies a single move to the board, records it, and evaluates win/draw.
    /// Updates the scoreboard exactly once per completed game. Advances
    /// CurrentPlayer only if the game continues.
    /// </summary>
    private void ApplyMove(GameSession session, Mark mark, int cellIndex)
    {
        session.Board[cellIndex] = mark;
        session.Moves.Add(new MoveRecord
        {
            MoveNumber = session.Moves.Count + 1,
            Player = mark,
            CellIndex = cellIndex
        });

        var winningLine = WinRules.FindWinningLine(session.Board, mark);
        if (winningLine != null)
        {
            session.Status = GameStatus.Won;
            session.Winner = mark;
            session.WinningCells = winningLine.ToList();
            RecordScoreOnce(session, mark);
            return;
        }

        if (WinRules.IsBoardFull(session.Board))
        {
            session.Status = GameStatus.Draw;
            RecordScoreOnce(session, null);
            return;
        }

        session.CurrentPlayer = mark == Mark.X ? Mark.O : Mark.X;
    }

    private void RecordScoreOnce(GameSession session, Mark? winner)
    {
        if (session.ScoreboardUpdated) return;

        if (winner.HasValue) _scoreboardService.RecordWin(winner.Value);
        else _scoreboardService.RecordDraw();

        session.ScoreboardUpdated = true;
    }

    /// <summary>
    /// Rebuilds board/status/current-player purely from the (already trimmed)
    /// move history. Used after Undo so state can never drift from the log.
    /// </summary>
    private static void ReplayFromMoveHistory(GameSession session)
    {
        session.Board = new Mark[9];
        session.Status = GameStatus.InProgress;
        session.Winner = null;
        session.WinningCells.Clear();
        session.ScoreboardUpdated = false;

        foreach (var move in session.Moves)
        {
            session.Board[move.CellIndex] = move.Player;
        }

        session.CurrentPlayer = session.Moves.Count == 0
            ? Mark.X
            : (session.Moves[^1].Player == Mark.X ? Mark.O : Mark.X);
    }
}
