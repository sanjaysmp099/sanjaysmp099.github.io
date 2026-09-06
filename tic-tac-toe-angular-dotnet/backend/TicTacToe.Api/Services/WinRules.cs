using TicTacToe.Api.Models;

namespace TicTacToe.Api.Services;

/// <summary>Pure, stateless helpers for evaluating board state. Kept separate
/// from GameService so it can be unit-tested and reused by the computer player.</summary>
public static class WinRules
{
    /// <summary>All 8 possible winning lines (3 rows, 3 columns, 2 diagonals).</summary>
    public static readonly int[][] Lines =
    {
        new[] {0, 1, 2}, new[] {3, 4, 5}, new[] {6, 7, 8}, // rows
        new[] {0, 3, 6}, new[] {1, 4, 7}, new[] {2, 5, 8}, // columns
        new[] {0, 4, 8}, new[] {2, 4, 6}                    // diagonals
    };

    /// <summary>Returns the winning line for <paramref name="mark"/>, or null if none.</summary>
    public static int[]? FindWinningLine(Mark[] board, Mark mark)
    {
        if (mark == Mark.Empty) return null;

        foreach (var line in Lines)
        {
            if (board[line[0]] == mark && board[line[1]] == mark && board[line[2]] == mark)
            {
                return line;
            }
        }
        return null;
    }

    public static bool IsBoardFull(Mark[] board) => board.All(c => c != Mark.Empty);
}
