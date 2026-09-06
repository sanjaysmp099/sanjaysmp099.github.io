using Microsoft.AspNetCore.Mvc;
using TicTacToe.Api.Dtos;
using TicTacToe.Api.Models;
using TicTacToe.Api.Services;

namespace TicTacToe.Api.Controllers;

[ApiController]
[Route("api/games")]
public class GamesController : ControllerBase
{
    private readonly IGameService _gameService;
    private readonly IScoreboardService _scoreboardService;

    public GamesController(IGameService gameService, IScoreboardService scoreboardService)
    {
        _gameService = gameService;
        _scoreboardService = scoreboardService;
    }

    /// <summary>POST /api/games — create a new game session.</summary>
    [HttpPost]
    public ActionResult<GameStateResponse> CreateGame([FromBody] CreateGameRequest request)
    {
        var session = _gameService.CreateGame(request.Mode);
        return Ok(GameStateMapper.ToResponse(session, _scoreboardService));
    }

    /// <summary>GET /api/games/{id} — get the current state of a game session.</summary>
    [HttpGet("{id}")]
    public ActionResult<GameStateResponse> GetGame(string id)
    {
        try
        {
            var session = _gameService.GetGame(id);
            return Ok(GameStateMapper.ToResponse(session, _scoreboardService));
        }
        catch (GameNotFoundException ex)
        {
            return NotFound(new ApiErrorResponse { Message = ex.Message });
        }
    }

    /// <summary>POST /api/games/{id}/moves — submit a player move.</summary>
    [HttpPost("{id}/moves")]
    public ActionResult<GameStateResponse> MakeMove(string id, [FromBody] MoveRequest request)
    {
        try
        {
            var cellIndex = ResolveCellIndex(request);
            var session = _gameService.MakeMove(id, request.Player, cellIndex);
            return Ok(GameStateMapper.ToResponse(session, _scoreboardService));
        }
        catch (GameNotFoundException ex)
        {
            return NotFound(new ApiErrorResponse { Message = ex.Message });
        }
        catch (GameRuleException ex)
        {
            return BadRequest(new ApiErrorResponse { Message = ex.Message });
        }
    }

    /// <summary>POST /api/games/{id}/undo — undo the last move (or move pair in computer mode).</summary>
    [HttpPost("{id}/undo")]
    public ActionResult<GameStateResponse> Undo(string id)
    {
        try
        {
            var session = _gameService.Undo(id);
            return Ok(GameStateMapper.ToResponse(session, _scoreboardService));
        }
        catch (GameNotFoundException ex)
        {
            return NotFound(new ApiErrorResponse { Message = ex.Message });
        }
        catch (GameRuleException ex)
        {
            return BadRequest(new ApiErrorResponse { Message = ex.Message });
        }
    }

    /// <summary>POST /api/games/{id}/reset — reset the current game (scoreboard is preserved).</summary>
    [HttpPost("{id}/reset")]
    public ActionResult<GameStateResponse> Reset(string id)
    {
        try
        {
            var session = _gameService.Reset(id);
            return Ok(GameStateMapper.ToResponse(session, _scoreboardService));
        }
        catch (GameNotFoundException ex)
        {
            return NotFound(new ApiErrorResponse { Message = ex.Message });
        }
    }

    /// <summary>Resolves either CellIndex, or Row+Column, into a single 0-8 index.</summary>
    private static int ResolveCellIndex(MoveRequest request)
    {
        if (request.CellIndex.HasValue)
        {
            return request.CellIndex.Value;
        }

        if (request.Row.HasValue && request.Column.HasValue)
        {
            return (request.Row.Value * 3) + request.Column.Value;
        }

        // Out-of-range sentinel; GameService validates and rejects it as
        // "outside the board" so both callers share one error path.
        return -1;
    }
}
