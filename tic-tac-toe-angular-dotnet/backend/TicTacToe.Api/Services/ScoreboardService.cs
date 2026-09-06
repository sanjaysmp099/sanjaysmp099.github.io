using TicTacToe.Api.Models;

namespace TicTacToe.Api.Services;

public interface IScoreboardService
{
    Scoreboard Get();
    void RecordWin(Mark winner);
    void RecordDraw();
    void Reset();
}

/// <summary>
/// Session-level scoreboard, held in memory for the lifetime of the backend
/// process. Registered as a singleton so all games share one scoreboard.
/// </summary>
public class ScoreboardService : IScoreboardService
{
    private readonly Scoreboard _scoreboard = new();
    private readonly object _lock = new();

    public Scoreboard Get()
    {
        lock (_lock)
        {
            // Return a copy so callers can't mutate internal state directly.
            return new Scoreboard
            {
                XWins = _scoreboard.XWins,
                OWins = _scoreboard.OWins,
                Draws = _scoreboard.Draws
            };
        }
    }

    public void RecordWin(Mark winner)
    {
        lock (_lock)
        {
            if (winner == Mark.X) _scoreboard.XWins++;
            else if (winner == Mark.O) _scoreboard.OWins++;
        }
    }

    public void RecordDraw()
    {
        lock (_lock)
        {
            _scoreboard.Draws++;
        }
    }

    public void Reset()
    {
        lock (_lock)
        {
            _scoreboard.XWins = 0;
            _scoreboard.OWins = 0;
            _scoreboard.Draws = 0;
        }
    }
}
