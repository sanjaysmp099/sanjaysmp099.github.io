namespace TicTacToe.Api.Services;

/// <summary>Thrown when a requested game operation violates the game rules
/// (e.g. occupied cell, wrong turn, move after completion). Caught by
/// controllers and translated into a 400 Bad Request.</summary>
public class GameRuleException : Exception
{
    public GameRuleException(string message) : base(message) { }
}

/// <summary>Thrown when a referenced game id does not exist. Translated into a 404.</summary>
public class GameNotFoundException : Exception
{
    public GameNotFoundException(string id) : base($"Game '{id}' was not found.") { }
}
