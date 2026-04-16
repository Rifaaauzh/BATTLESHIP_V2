using Battleship.Enum;

namespace Battleship.Interfaces;

public interface IShip
{
    public ShipType ShipType { get; }
    public int Hits { get; }
    public int Size { get; }
}