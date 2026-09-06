using Microsoft.AspNetCore.Mvc;
using TicTacToe.Api.Dtos;
using TicTacToe.Api.Services;

namespace TicTacToe.Api.Controllers;

[ApiController]
[Route("api/scoreboard")]
public class ScoreboardController : ControllerBase
{
    private readonly IScoreboardService _scoreboardService;

    public ScoreboardController(IScoreboardService scoreboardService)
    {
        _scoreboardService = scoreboardService;
    }

    /// <summary>GET /api/scoreboard — current session scoreboard.</summary>
    [HttpGet]
    public ActionResult<ScoreboardResponse> Get()
    {
        var scoreboard = _scoreboardService.Get();
        return Ok(new ScoreboardResponse
        {
            XWins = scoreboard.XWins,
            OWins = scoreboard.OWins,
            Draws = scoreboard.Draws
        });
    }

    /// <summary>POST /api/scoreboard/reset — reset X wins / O wins / draws to zero.</summary>
    [HttpPost("reset")]
    public ActionResult<ScoreboardResponse> Reset()
    {
        _scoreboardService.Reset();
        var scoreboard = _scoreboardService.Get();
        return Ok(new ScoreboardResponse
        {
            XWins = scoreboard.XWins,
            OWins = scoreboard.OWins,
            Draws = scoreboard.Draws
        });
    }
}
