using Battleship.Enum;
using Battleship.Models;

namespace Battleship.Interfaces;

public interface ICell
{
    Position Position { get; }
    CellState State { get; }
    IShip? Ship { get; }
}