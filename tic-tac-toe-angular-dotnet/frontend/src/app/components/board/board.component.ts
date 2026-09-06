import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Mark } from '../../models/game.models';

@Component({
  selector: 'app-board',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './board.component.html',
  styleUrl: './board.component.css'
})
export class BoardComponent {
  /** 9 entries: 'X', 'O', or null. */
  @Input() board: (Mark | null)[] = new Array(9).fill(null);
  @Input() winningCells: number[] = [];
  /** True once the game is Won/Draw, or while a request is in flight. */
  @Input() disabled = false;

  @Output() cellClicked = new EventEmitter<number>();

  onCellClick(index: number): void {
    if (this.disabled || this.board[index] !== null) {
      return;
    }
    this.cellClicked.emit(index);
  }

  isWinningCell(index: number): boolean {
    return this.winningCells.includes(index);
  }

  trackByIndex(index: number): number {
    return index;
  }
}
