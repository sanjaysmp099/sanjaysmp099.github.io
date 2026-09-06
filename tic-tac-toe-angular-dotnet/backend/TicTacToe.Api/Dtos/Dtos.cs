using TicTacToe.Api.Models;

namespace TicTacToe.Api.Dtos;

public class CreateGameRequest
{
    /// <summary>"TwoPlayer" or "VsComputer".</summary>
    public GameMode Mode { get; set; } = GameMode.TwoPlayer;
}

public class MoveRequest
{
    /// <summary>The player making the move ("X" or "O"). Must match the server's CurrentPlayer.</summary>
    public Mark Player { get; set; }

    /// <summary>Preferred: 0-8 index into the board (row * 3 + col).</summary>
    public int? CellIndex { get; set; }

    /// <summary>Alternative to CellIndex: 0-2 row/column pair.</summary>
    public int? Row { get; set; }
    public int? Column { get; set; }
}

public class MoveRecordDto
{
    public int MoveNumber { get; set; }
    public string Player { get; set; } = "";
    public int CellIndex { get; set; }
    public int Row { get; set; }
    public int Column { get; set; }
}

public class ScoreboardResponse
{
    public int XWins { get; set; }
    public int OWins { get; set; }
    public int Draws { get; set; }
}

public class GameStateResponse
{
    public string Id { get; set; } = "";

    /// <summary>9 entries, each "X", "O", or null for an empty cell.</summary>
    public string?[] Board { get; set; } = new string?[9];

    public string CurrentPlayer { get; set; } = "X";
    public string Mode { get; set; } = "TwoPlayer";
    public string Status { get; set; } = "InProgress";
    public string? Winner { get; set; }
    public List<int> WinningCells { get; set; } = new();
    public List<MoveRecordDto> MoveHistory { get; set; } = new();
    public bool CanUndo { get; set; }
    public ScoreboardResponse Scoreboard { get; set; } = new();
}

/// <summary>Generic error payload returned for invalid requests.</summary>
public class ApiErrorResponse
{
    public string Message { get; set; } = "";
}
