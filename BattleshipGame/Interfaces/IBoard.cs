using Battleship.Models;

namespace Battleship.Interfaces;

public interface IBoard
{
    public int Size {get; }
    ICell GetCell (Position position);
}