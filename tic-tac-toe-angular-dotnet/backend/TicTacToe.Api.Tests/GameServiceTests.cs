using TicTacToe.Api.Models;
using TicTacToe.Api.Services;
using Xunit;

namespace TicTacToe.Api.Tests;

public class GameServiceTests
{
    private static GameService CreateService(out IScoreboardService scoreboard)
    {
        scoreboard = new ScoreboardService();
        return new GameService(scoreboard, new ComputerPlayerService());
    }

    [Fact]
    public void CreateGame_ReturnsNewSessionWithEmptyBoardAndXToStart()
    {
        var service = CreateService(out _);

        var session = service.CreateGame(GameMode.TwoPlayer);

        Assert.All(session.Board, cell => Assert.Equal(Mark.Empty, cell));
        Assert.Equal(Mark.X, session.CurrentPlayer);
        Assert.Equal(GameStatus.InProgress, session.Status);
        Assert.Empty(session.Moves);
    }

    [Fact]
    public void MakeMove_ValidMove_UpdatesBoardAndSwitchesTurn()
    {
        var service = CreateService(out _);
        var session = service.CreateGame(GameMode.TwoPlayer);

        var updated = service.MakeMove(session.Id, Mark.X, 0);

        Assert.Equal(Mark.X, updated.Board[0]);
        Assert.Equal(Mark.O, updated.CurrentPlayer); // turn switches after a valid move
        Assert.Single(updated.Moves);
    }

    [Fact]
    public void MakeMove_InvalidMoveOnOccupiedCell_ThrowsAndDoesNotChangeTurn()
    {
        var service = CreateService(out _);
        var session = service.CreateGame(GameMode.TwoPlayer);
        service.MakeMove(session.Id, Mark.X, 0);

        var ex = Assert.Throws<GameRuleException>(() => service.MakeMove(session.Id, Mark.O, 0));

        Assert.Contains("occupied", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(Mark.O, service.GetGame(session.Id).CurrentPlayer); // turn unchanged
    }

    [Fact]
    public void MakeMove_WrongPlayerTurn_Throws()
    {
        var service = CreateService(out _);
        var session = service.CreateGame(GameMode.TwoPlayer);

        // It is X's turn; O tries to move.
        var ex = Assert.Throws<GameRuleException>(() => service.MakeMove(session.Id, Mark.O, 0));

        Assert.Contains("turn", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(9)]
    public void MakeMove_CellIndexOutsideBoard_Throws(int cellIndex)
    {
        var service = CreateService(out _);
        var session = service.CreateGame(GameMode.TwoPlayer);

        var ex = Assert.Throws<GameRuleException>(() => service.MakeMove(session.Id, Mark.X, cellIndex));

        Assert.Contains("outside", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void MakeMove_AfterGameCompletion_Throws()
    {
        var service = CreateService(out _);
        var session = service.CreateGame(GameMode.TwoPlayer);
        WinRowForX(service, session.Id);

        var ex = Assert.Throws<GameRuleException>(() => service.MakeMove(session.Id, Mark.O, 8));

        Assert.Contains("completed", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void MakeMove_CompletingRow_DeclaresWinnerAndHighlightsCells()
    {
        var service = CreateService(out _);
        var session = service.CreateGame(GameMode.TwoPlayer);

        var final = WinRowForX(service, session.Id);

        Assert.Equal(GameStatus.Won, final.Status);
        Assert.Equal(Mark.X, final.Winner);
        Assert.Equal(new List<int> { 0, 1, 2 }, final.WinningCells);
    }

    [Fact]
    public void MakeMove_CompletingColumn_DeclaresWinner()
    {
        var service = CreateService(out _);
        var session = service.CreateGame(GameMode.TwoPlayer);

        service.MakeMove(session.Id, Mark.X, 0);
        service.MakeMove(session.Id, Mark.O, 1);
        service.MakeMove(session.Id, Mark.X, 3);
        service.MakeMove(session.Id, Mark.O, 2);
        var final = service.MakeMove(session.Id, Mark.X, 6);

        Assert.Equal(GameStatus.Won, final.Status);
        Assert.Equal(Mark.X, final.Winner);
        Assert.Equal(new List<int> { 0, 3, 6 }, final.WinningCells);
    }

    [Fact]
    public void MakeMove_CompletingDiagonal_DeclaresWinner()
    {
        var service = CreateService(out _);
        var session = service.CreateGame(GameMode.TwoPlayer);

        service.MakeMove(session.Id, Mark.X, 0);
        service.MakeMove(session.Id, Mark.O, 1);
        service.MakeMove(session.Id, Mark.X, 4);
        service.MakeMove(session.Id, Mark.O, 2);
        var final = service.MakeMove(session.Id, Mark.X, 8);

        Assert.Equal(GameStatus.Won, final.Status);
        Assert.Equal(Mark.X, final.Winner);
        Assert.Equal(new List<int> { 0, 4, 8 }, final.WinningCells);
    }

    [Fact]
    public void MakeMove_BoardFullWithNoWinner_IsDraw()
    {
        var service = CreateService(out _);
        var session = service.CreateGame(GameMode.TwoPlayer);

        // Board (no line is fully X or fully O):
        // X O X
        // X O O
        // O X X
        service.MakeMove(session.Id, Mark.X, 0);
        service.MakeMove(session.Id, Mark.O, 1);
        service.MakeMove(session.Id, Mark.X, 2);
        service.MakeMove(session.Id, Mark.O, 4);
        service.MakeMove(session.Id, Mark.X, 3);
        service.MakeMove(session.Id, Mark.O, 5);
        service.MakeMove(session.Id, Mark.X, 7);
        service.MakeMove(session.Id, Mark.O, 6);
        var final = service.MakeMove(session.Id, Mark.X, 8);

        Assert.Equal(GameStatus.Draw, final.Status);
        Assert.Null(final.Winner);
    }

    [Fact]
    public void Reset_ClearsGameButKeepsScoreboard()
    {
        var service = CreateService(out var scoreboard);
        var session = service.CreateGame(GameMode.TwoPlayer);
        WinRowForX(service, session.Id);
        Assert.Equal(1, scoreboard.Get().XWins);

        var reset = service.Reset(session.Id);

        Assert.All(reset.Board, cell => Assert.Equal(Mark.Empty, cell));
        Assert.Empty(reset.Moves);
        Assert.Equal(GameStatus.InProgress, reset.Status);
        Assert.Equal(Mark.X, reset.CurrentPlayer);
        Assert.Equal(1, scoreboard.Get().XWins); // untouched by Reset Game
    }

    [Fact]
    public void Undo_TwoPlayerMode_RemovesOnlyMostRecentMove()
    {
        var service = CreateService(out _);
        var session = service.CreateGame(GameMode.TwoPlayer);
        service.MakeMove(session.Id, Mark.X, 0);
        service.MakeMove(session.Id, Mark.O, 4);

        var afterUndo = service.Undo(session.Id);

        Assert.Single(afterUndo.Moves);
        Assert.Equal(Mark.X, afterUndo.Board[0]);
        Assert.Equal(Mark.Empty, afterUndo.Board[4]);
        Assert.Equal(Mark.O, afterUndo.CurrentPlayer); // O's turn again
    }

    [Fact]
    public void Undo_ComputerMode_RemovesComputerMoveAndHumanMoveTogether()
    {
        var service = CreateService(out _);
        var session = service.CreateGame(GameMode.VsComputer);

        // Human (X) plays a corner; computer (O) auto-responds (center, by priority).
        var afterHumanMove = service.MakeMove(session.Id, Mark.X, 0);
        Assert.Equal(2, afterHumanMove.Moves.Count); // human + computer moves both applied

        var afterUndo = service.Undo(session.Id);

        Assert.Empty(afterUndo.Moves);
        Assert.All(afterUndo.Board, cell => Assert.Equal(Mark.Empty, cell));
        Assert.Equal(Mark.X, afterUndo.CurrentPlayer); // back to the human
    }

    [Fact]
    public void Undo_WithNoMoves_Throws()
    {
        var service = CreateService(out _);
        var session = service.CreateGame(GameMode.TwoPlayer);

        var ex = Assert.Throws<GameRuleException>(() => service.Undo(session.Id));
        Assert.Contains("no moves", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Undo_AfterGameCompleted_IsDisabled()
    {
        var service = CreateService(out _);
        var session = service.CreateGame(GameMode.TwoPlayer);
        WinRowForX(service, session.Id);

        var ex = Assert.Throws<GameRuleException>(() => service.Undo(session.Id));
        Assert.Contains("completed", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Scoreboard_UpdatesExactlyOncePerCompletedGame()
    {
        var service = CreateService(out var scoreboard);
        var session = service.CreateGame(GameMode.TwoPlayer);

        WinRowForX(service, session.Id);
        Assert.Equal(1, scoreboard.Get().XWins);

        // Further calls against the completed game must not double-count.
        Assert.Throws<GameRuleException>(() => service.MakeMove(session.Id, Mark.O, 8));
        Assert.Equal(1, scoreboard.Get().XWins);
    }

    [Fact]
    public void Scoreboard_RecordsDraw()
    {
        var service = CreateService(out var scoreboard);
        var session = service.CreateGame(GameMode.TwoPlayer);

        service.MakeMove(session.Id, Mark.X, 0);
        service.MakeMove(session.Id, Mark.O, 1);
        service.MakeMove(session.Id, Mark.X, 2);
        service.MakeMove(session.Id, Mark.O, 4);
        service.MakeMove(session.Id, Mark.X, 3);
        service.MakeMove(session.Id, Mark.O, 5);
        service.MakeMove(session.Id, Mark.X, 7);
        service.MakeMove(session.Id, Mark.O, 6);
        service.MakeMove(session.Id, Mark.X, 8);

        Assert.Equal(1, scoreboard.Get().Draws);
    }

    [Fact]
    public void ComputerMode_ComputerMovesAutomaticallyAndOnlyOnValidCells()
    {
        var service = CreateService(out _);
        var session = service.CreateGame(GameMode.VsComputer);

        var afterHumanMove = service.MakeMove(session.Id, Mark.X, 0);

        // Computer should have played exactly one valid (previously empty) cell.
        Assert.Equal(2, afterHumanMove.Moves.Count);
        var computerMove = afterHumanMove.Moves[1];
        Assert.Equal(Mark.O, computerMove.Player);
        Assert.NotEqual(0, computerMove.CellIndex); // didn't overwrite the human's cell
    }

    /// <summary>Helper: plays out a top-row win for X in Two Player mode.</summary>
    private static GameSession WinRowForX(IGameService service, string gameId)
    {
        service.MakeMove(gameId, Mark.X, 0);
        service.MakeMove(gameId, Mark.O, 3);
        service.MakeMove(gameId, Mark.X, 1);
        service.MakeMove(gameId, Mark.O, 4);
        return service.MakeMove(gameId, Mark.X, 2);
    }
}
