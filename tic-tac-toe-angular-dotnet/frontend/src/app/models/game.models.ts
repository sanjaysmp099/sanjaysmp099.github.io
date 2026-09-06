export type Mark = 'X' | 'O';
export type GameMode = 'TwoPlayer' | 'VsComputer';
export type GameStatus = 'InProgress' | 'Won' | 'Draw';

export interface MoveRecord {
  moveNumber: number;
  player: Mark;
  cellIndex: number;
  row: number;
  column: number;
}

export interface Scoreboard {
  xWins: number;
  oWins: number;
  draws: number;
}

export interface GameState {
  id: string;
  /** 9 entries: 'X', 'O', or null for an empty cell. */
  board: (Mark | null)[];
  currentPlayer: Mark;
  mode: GameMode;
  status: GameStatus;
  winner: Mark | null;
  winningCells: number[];
  moveHistory: MoveRecord[];
  canUndo: boolean;
  scoreboard: Scoreboard;
}

export interface ApiError {
  message: string;
}
