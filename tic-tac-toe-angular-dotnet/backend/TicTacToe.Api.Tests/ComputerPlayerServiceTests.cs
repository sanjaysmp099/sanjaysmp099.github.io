using TicTacToe.Api.Models;
using TicTacToe.Api.Services;
using Xunit;

namespace TicTacToe.Api.Tests;

public class ComputerPlayerServiceTests
{
    private readonly ComputerPlayerService _computer = new();

    [Fact]
    public void ChooseMove_PlaysWinningMoveWhenAvailable()
    {
        var board = new Mark[9];
        board[0] = Mark.O;
        board[1] = Mark.O;
        // board[2] empty -> completes the top row for O.
        board[3] = Mark.X;
        board[4] = Mark.X;

        var move = _computer.ChooseMove(board);

        Assert.Equal(2, move);
    }

    [Fact]
    public void ChooseMove_BlocksOpponentWinningMoveWhenNoOwnWinExists()
    {
        var board = new Mark[9];
        board[0] = Mark.X;
        board[1] = Mark.X;
        // board[2] empty -> X would win here; O has no winning move of its own.
        board[3] = Mark.O;

        var move = _computer.ChooseMove(board);

        Assert.Equal(2, move);
    }

    [Fact]
    public void ChooseMove_TakesCenterWhenBoardIsEmpty()
    {
        var board = new Mark[9];

        var move = _computer.ChooseMove(board);

        Assert.Equal(4, move);
    }

    [Fact]
    public void ChooseMove_TakesACornerWhenCenterIsTaken()
    {
        var board = new Mark[9];
        board[4] = Mark.X; // center already taken, no win/block available

        var move = _computer.ChooseMove(board);

        Assert.Contains(move, new[] { 0, 2, 6, 8 });
    }

    [Fact]
    public void ChooseMove_TakesAnyAvailableCellWhenNoCenterOrCornerLeft()
    {
        var board = new Mark[9];
        board[4] = Mark.X; // center
        board[0] = Mark.O; board[2] = Mark.O; board[6] = Mark.O; board[8] = Mark.X; // corners all taken

        var move = _computer.ChooseMove(board);

        Assert.Contains(move, new[] { 1, 3, 5, 7 });
    }

    [Fact]
    public void ChooseMove_ReturnsMinusOneWhenBoardIsFull()
    {
        var board = new[]
        {
            Mark.X, Mark.O, Mark.X,
            Mark.X, Mark.O, Mark.O,
            Mark.O, Mark.X, Mark.X
        };

        var move = _computer.ChooseMove(board);

        Assert.Equal(-1, move);
    }
}
