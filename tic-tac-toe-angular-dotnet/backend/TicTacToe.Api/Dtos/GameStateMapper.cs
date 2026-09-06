using TicTacToe.Api.Models;
using TicTacToe.Api.Services;

namespace TicTacToe.Api.Dtos;

public static class GameStateMapper
{
    public static GameStateResponse ToResponse(GameSession session, IScoreboardService scoreboardService)
    {
        var scoreboard = scoreboardService.Get();

        return new GameStateResponse
        {
            Id = session.Id,
            Board = session.Board.Select(MarkToString).ToArray(),
            CurrentPlayer = session.CurrentPlayer.ToString(),
            Mode = session.Mode.ToString(),
            Status = session.Status.ToString(),
            Winner = session.Winner.HasValue ? session.Winner.Value.ToString() : null,
            WinningCells = session.WinningCells.ToList(),
            MoveHistory = session.Moves.Select(m => new MoveRecordDto
            {
                MoveNumber = m.MoveNumber,
                Player = m.Player.ToString(),
                CellIndex = m.CellIndex,
                Row = m.Row,
                Column = m.Column
            }).ToList(),
            CanUndo = session.Moves.Count > 0 && session.Status == GameStatus.InProgress,
            Scoreboard = new ScoreboardResponse
            {
                XWins = scoreboard.XWins,
                OWins = scoreboard.OWins,
                Draws = scoreboard.Draws
            }
        };
    }

    private static string? MarkToString(Mark mark) => mark switch
    {
        Mark.X => "X",
        Mark.O => "O",
        _ => null
    };
}
