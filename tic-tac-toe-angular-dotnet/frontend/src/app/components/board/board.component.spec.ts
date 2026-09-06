import { ComponentFixture, TestBed } from '@angular/core/testing';
import { BoardComponent } from './board.component';

describe('BoardComponent', () => {
  let fixture: ComponentFixture<BoardComponent>;
  let component: BoardComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BoardComponent]
    }).compileComponents();

    fixture = TestBed.createComponent(BoardComponent);
    component = fixture.componentInstance;
  });

  it('renders 9 cells', () => {
    fixture.detectChanges();
    const cells = fixture.nativeElement.querySelectorAll('.cell');
    expect(cells.length).toBe(9);
  });

  it('emits cellClicked with the index when an empty cell is clicked', () => {
    fixture.detectChanges();
    spyOn(component.cellClicked, 'emit');

    const cells = fixture.nativeElement.querySelectorAll('.cell');
    cells[3].click();

    expect(component.cellClicked.emit).toHaveBeenCalledWith(3);
  });

  it('does not emit when the board is disabled', () => {
    component.disabled = true;
    fixture.detectChanges();
    spyOn(component.cellClicked, 'emit');

    const cells = fixture.nativeElement.querySelectorAll('.cell');
    cells[0].click();

    expect(component.cellClicked.emit).not.toHaveBeenCalled();
  });

  it('does not emit when clicking an already-filled cell', () => {
    component.board = ['X', null, null, null, null, null, null, null, null];
    fixture.detectChanges();
    spyOn(component.cellClicked, 'emit');

    const cells = fixture.nativeElement.querySelectorAll('.cell');
    cells[0].click();

    expect(component.cellClicked.emit).not.toHaveBeenCalled();
  });

  it('applies the winning class to winning cells', () => {
    component.winningCells = [0, 1, 2];
    fixture.detectChanges();

    const cells = fixture.nativeElement.querySelectorAll('.cell');
    expect(cells[0].classList).toContain('winning');
    expect(cells[3].classList).not.toContain('winning');
  });
});
