import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Scoreboard } from '../../models/game.models';

@Component({
  selector: 'app-scoreboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './scoreboard.component.html',
  styleUrl: './scoreboard.component.css'
})
export class ScoreboardComponent {
  @Input() scoreboard: Scoreboard = { xWins: 0, oWins: 0, draws: 0 };
  @Output() resetScoreboard = new EventEmitter<void>();
}
