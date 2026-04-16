namespace Battleship.Models;

public struct Position
{
    int X { get; }
    int Y { get; }

    public Position(int x, int y)
    {
        X = x;
        Y = y;
    }
}