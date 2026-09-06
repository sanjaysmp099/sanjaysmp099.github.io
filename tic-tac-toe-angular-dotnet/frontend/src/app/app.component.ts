import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GameService } from './services/game.service';
import { GameMode, GameState } from './models/game.models';
import { BoardComponent } from './components/board/board.component';
import { MoveHistoryComponent } from './components/move-history/move-history.component';
import { ScoreboardComponent } from './components/scoreboard/scoreboard.component';
import { ModeSelectorComponent } from './components/mode-selector/mode-selector.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    CommonModule,
    BoardComponent,
    MoveHistoryComponent,
    ScoreboardComponent,
    ModeSelectorComponent
  ],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent implements OnInit {
  game: GameState | null = null;
  errorMessage: string | null = null;
  loading = false;

  constructor(private gameService: GameService) {}

  ngOnInit(): void {
    this.startNewGame('TwoPlayer');
  }

  get statusMessage(): string {
    if (!this.game) return '';
    if (this.game.status === 'Won') return `${this.game.winner} wins!`;
    if (this.game.status === 'Draw') return "It's a draw!";
    return `${this.game.currentPlayer}'s turn`;
  }

  get boardDisabled(): boolean {
    return !this.game || this.loading || this.game.status !== 'InProgress';
  }

  startNewGame(mode: GameMode): void {
    this.loading = true;
    this.gameService.createGame(mode).subscribe({
      next: (state) => {
        this.game = state;
        this.loading = false;
        this.errorMessage = null;
      },
      error: () => this.handleError('Could not start a new game. Is the backend running?')
    });
  }

  onCellClicked(cellIndex: number): void {
    if (!this.game) return;
    this.loading = true;
    this.gameService.makeMove(this.game.id, this.game.currentPlayer, cellIndex).subscribe({
      next: (state) => this.applyState(state),
      error: (err) => this.handleError(this.extractMessage(err, 'That move was not allowed.'))
    });
  }

  onUndo(): void {
    if (!this.game) return;
    this.loading = true;
    this.gameService.undo(this.game.id).subscribe({
      next: (state) => this.applyState(state),
      error: (err) => this.handleError(this.extractMessage(err, 'Could not undo.'))
    });
  }

  onResetGame(): void {
    if (!this.game) return;
    this.loading = true;
    this.gameService.resetGame(this.game.id).subscribe({
      next: (state) => this.applyState(state),
      error: () => this.handleError('Could not reset the game.')
    });
  }

  onResetScoreboard(): void {
    this.gameService.resetScoreboard().subscribe({
      next: () => {
        if (this.game) {
          this.gameService.getGame(this.game.id).subscribe((state) => (this.game = state));
        }
      },
      error: () => this.handleError('Could not reset the scoreboard.')
    });
  }

  private applyState(state: GameState): void {
    this.game = state;
    this.loading = false;
    this.errorMessage = null;
  }

  private handleError(message: string): void {
    this.loading = false;
    this.errorMessage = message;
  }

  private extractMessage(err: unknown, fallback: string): string {
    const httpErr = err as { error?: { message?: string } };
    return httpErr?.error?.message ?? fallback;
  }
}
