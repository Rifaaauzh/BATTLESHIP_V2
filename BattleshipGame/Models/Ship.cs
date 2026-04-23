using System.Collections;
using Battleship.Enum;
using Battleship.Interfaces;

namespace Battleship.Models;

public class Ship : IShip
{
    public ShipType ShipType { get;}
    public int Size { get;}
    public int Hits { get; set;}
    public Orientation Orientation { get;}
    public Position Position {get;}

    public Ship(ShipType shipType, Position position, Orientation orientation)
    {
        ShipType = shipType;
        Hits = 0;
        Orientation = orientation;
        Position = position;

        Size = shipType switch
        {
            ShipType.Carrier => 5,
            ShipType.Battleship => 4,
            ShipType.Cruiser => 3,
            ShipType.Destroyer => 3,
            ShipType.PatrolBoat => 2,
        };
    }
    
}