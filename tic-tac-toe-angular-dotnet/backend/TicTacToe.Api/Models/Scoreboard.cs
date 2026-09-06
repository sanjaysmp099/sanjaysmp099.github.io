namespace TicTacToe.Api.Models;

/// <summary>Session-level (in-memory, process-lifetime) scoreboard.</summary>
public class Scoreboard
{
    public int XWins { get; set; }
    public int OWins { get; set; }
    public int Draws { get; set; }
}
