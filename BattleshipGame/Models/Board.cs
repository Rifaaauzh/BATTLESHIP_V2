namespace Battleship.Models;

public class Board
{
    public int Size { get; set; }
    public int[,] Cell { get; }

    public Board(int size)
    {
        Size = size;
    }
}