namespace TicTacToe.Api.Models;

/// <summary>
/// Server-owned state for a single Tic Tac Toe game session.
/// The backend is the single source of truth for this state.
/// </summary>
public class GameSession
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>9 cells, index = row * 3 + col.</summary>
    public Mark[] Board { get; set; } = new Mark[9];

    public Mark CurrentPlayer { get; set; } = Mark.X;

    public GameMode Mode { get; set; } = GameMode.TwoPlayer;

    public GameStatus Status { get; set; } = GameStatus.InProgress;

    public Mark? Winner { get; set; }

    public List<int> WinningCells { get; set; } = new();

    public List<MoveRecord> Moves { get; set; } = new();

    /// <summary>
    /// Guards against double-counting: the scoreboard is updated exactly once
    /// per completed game (see Clarification 2, Option A in the README).
    /// </summary>
    public bool ScoreboardUpdated { get; set; }
}
