using Battleship.Enum;

namespace Battleship.Models;

public class Ship
{
    public ShipType ShipType { get; set;}
    public int Size { get; set; }
    public int Hits { get; set; }
    public Orientation Orientation { get; }

    public Ship(ShipType shipType, int size, int hits, Orientation orientation)
    {
        ShipType = shipType;
        Hits = 0;
        Orientation = orientation;
    }
    
    
    
    
}