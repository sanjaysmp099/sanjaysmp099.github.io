import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { AppComponent } from './app.component';
import { GameState } from './models/game.models';

const emptyGame: GameState = {
  id: 'game-1',
  board: new Array(9).fill(null),
  currentPlayer: 'X',
  mode: 'TwoPlayer',
  status: 'InProgress',
  winner: null,
  winningCells: [],
  moveHistory: [],
  canUndo: false,
  scoreboard: { xWins: 0, oWins: 0, draws: 0 }
};

describe('AppComponent', () => {
  let fixture: ComponentFixture<AppComponent>;
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AppComponent],
      providers: [provideHttpClient(), provideHttpClientTesting()]
    }).compileComponents();

    fixture = TestBed.createComponent(AppComponent);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('creates a new Two Player game on init', () => {
    fixture.detectChanges();

    const req = httpMock.expectOne('/api/games');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ mode: 'TwoPlayer' });
    req.flush(emptyGame);

    expect(fixture.componentInstance.game?.id).toBe('game-1');
  });

  it('submits a move with the current player and clicked cell', () => {
    fixture.detectChanges();
    httpMock.expectOne('/api/games').flush(emptyGame);

    fixture.componentInstance.onCellClicked(4);

    const req = httpMock.expectOne('/api/games/game-1/moves');
    expect(req.request.body).toEqual({ player: 'X', cellIndex: 4 });
    req.flush({ ...emptyGame, board: [null, null, null, null, 'X', null, null, null, null], currentPlayer: 'O' });

    expect(fixture.componentInstance.game?.currentPlayer).toBe('O');
  });

  it('shows the backend error message when a move is rejected', () => {
    fixture.detectChanges();
    httpMock.expectOne('/api/games').flush(emptyGame);

    fixture.componentInstance.onCellClicked(0);
    const req = httpMock.expectOne('/api/games/game-1/moves');
    req.flush({ message: 'Move rejected: that cell is already occupied.' }, { status: 400, statusText: 'Bad Request' });

    expect(fixture.componentInstance.errorMessage).toContain('occupied');
  });
});
