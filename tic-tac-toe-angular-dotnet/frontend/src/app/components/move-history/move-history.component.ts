import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MoveRecord } from '../../models/game.models';

@Component({
  selector: 'app-move-history',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './move-history.component.html',
  styleUrl: './move-history.component.css'
})
export class MoveHistoryComponent {
  @Input() moves: MoveRecord[] = [];

  trackByMoveNumber(_index: number, move: MoveRecord): number {
    return move.moveNumber;
  }
}
