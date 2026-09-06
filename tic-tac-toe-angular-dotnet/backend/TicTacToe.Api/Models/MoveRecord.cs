namespace TicTacToe.Api.Models;

/// <summary>A single recorded move in a game's history.</summary>
public class MoveRecord
{
    public int MoveNumber { get; set; }
    public Mark Player { get; set; }
    public int CellIndex { get; set; }
    public int Row => CellIndex / 3;
    public int Column => CellIndex % 3;
}
