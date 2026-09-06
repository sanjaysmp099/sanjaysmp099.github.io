import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GameMode } from '../../models/game.models';

@Component({
  selector: 'app-mode-selector',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './mode-selector.component.html',
  styleUrl: './mode-selector.component.css'
})
export class ModeSelectorComponent {
  @Input() selectedMode: GameMode = 'TwoPlayer';
  /** Emits the chosen mode; the parent starts a brand-new game in that mode. */
  @Output() modeSelected = new EventEmitter<GameMode>();
}
