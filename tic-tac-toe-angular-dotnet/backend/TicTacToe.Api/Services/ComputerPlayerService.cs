using TicTacToe.Api.Models;

namespace TicTacToe.Api.Services;

public interface IComputerPlayerService
{
    /// <summary>
    /// Chooses the computer's next move (always plays as O) following this priority:
    /// 1. Win now if possible.
    /// 2. Block the opponent's (X's) winning move if they have one.
    /// 3. Take the center.
    /// 4. Take a corner.
    /// 5. Take any remaining cell.
    /// Returns -1 if the board is full (no move available).
    /// </summary>
    int ChooseMove(Mark[] board);
}

public class ComputerPlayerService : IComputerPlayerService
{
    private static readonly int[] Corners = { 0, 2, 6, 8 };
    private const int Center = 4;

    public int ChooseMove(Mark[] board)
    {
        // 1. Win if possible.
        var winMove = FindCompletingMove(board, Mark.O);
        if (winMove != -1) return winMove;

        // 2. Block opponent's winning move.
        var blockMove = FindCompletingMove(board, Mark.X);
        if (blockMove != -1) return blockMove;

        // 3. Center.
        if (board[Center] == Mark.Empty) return Center;

        // 4. Corner.
        foreach (var corner in Corners)
        {
            if (board[corner] == Mark.Empty) return corner;
        }

        // 5. Any available cell.
        for (int i = 0; i < board.Length; i++)
        {
            if (board[i] == Mark.Empty) return i;
        }

        return -1;
    }

    /// <summary>
    /// Finds a cell that, if <paramref name="mark"/> were placed there, would
    /// immediately complete a line for that mark.
    /// </summary>
    private static int FindCompletingMove(Mark[] board, Mark mark)
    {
        for (int i = 0; i < board.Length; i++)
        {
            if (board[i] != Mark.Empty) continue;

            var trial = (Mark[])board.Clone();
            trial[i] = mark;
            if (WinRules.FindWinningLine(trial, mark) != null)
            {
                return i;
            }
        }
        return -1;
    }
}
