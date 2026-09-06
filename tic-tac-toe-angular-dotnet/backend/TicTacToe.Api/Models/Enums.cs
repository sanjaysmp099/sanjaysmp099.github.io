namespace TicTacToe.Api.Models;

/// <summary>A cell/player mark on the board.</summary>
public enum Mark
{
    Empty = 0,
    X = 1,
    O = 2
}

/// <summary>Game mode selected when a game session is created.</summary>
public enum GameMode
{
    TwoPlayer = 0,
    VsComputer = 1
}

/// <summary>Overall status of a game session.</summary>
public enum GameStatus
{
    InProgress = 0,
    Won = 1,
    Draw = 2
}
