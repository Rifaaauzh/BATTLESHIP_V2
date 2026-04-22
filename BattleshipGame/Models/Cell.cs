using Battleship.Enum;
using Battleship.Interfaces;

namespace Battleship.Models;

public class Cell : ICell
{
    public Position Position { get; private set; }
    public CellState State { get; set; }
    public IShip? Ship { get; set; }

    public Cell(Position position)
    {
        Position = position;
        State = CellState.Empty;
        Ship = null;
    }
}