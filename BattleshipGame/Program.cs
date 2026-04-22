using System;
using Battleship.Models;
using Battleship.Enum;
using Battleship.Interfaces;
using Battleship.Controller;

namespace Battleship;

class Program
{
    static void Main()
    {
        Console.WriteLine("=== BATTLESHIP GAME ===\n");

        Console.Write("Enter Player 1 name: ");
        var p1 = new Player(Console.ReadLine()!);

        Console.Write("Enter Player 2 name: ");
        var p2 = new Player(Console.ReadLine()!);

        var game = new GameController(p1, p2);

// 🎮 EVENT: turn change
        game.OnTurnChanged += player =>
        {
            Console.WriteLine($"\n Turn: {player.Name}");
        };

//  EVENT: move result
        game.OnMoveProcessed += cell =>
        {
            if (cell.State == CellState.Hit)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(" HIT!");
            }
            else if (cell.State == CellState.Miss)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(" MISS!");
            }

            Console.ResetColor();
        };

//  EVENT: ship sunk
        game.OnShipSunk += ship =>
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($" {ship.ShipType} SUNK!");
            Console.ResetColor();
        };

//  EVENT: game over
        game.OnGameOver += player =>
        {
            Console.Clear();
            Console.WriteLine("================================");
            Console.WriteLine("🏆 GAME OVER");
            Console.WriteLine($"Winner: {player.Name}");
            Console.WriteLine("================================");
        };

        // Setup phase
        SetupPhase(game, p1);
        SetupPhase(game, p2);

        game.StartGame();

        // Game loop
        while (game.GetStatus() != GameStatus.End)
        {
            Console.Clear();
            var current = game.GetCurrentPlayer();

            Console.WriteLine($"=== {current.Name}'s Turn ===\n");

            // Enemy board
            Console.WriteLine("Enemy Board:");
            PrintBoard(game.GetBoard(game.GetOpponent()), true);

            Console.WriteLine();

            // Own board
            Console.WriteLine("Your Board:");
            PrintBoard(game.GetBoard(current), false);

            Console.WriteLine();

            // Input attack
            Console.Write("Enter attack (x y): ");
            var input = Console.ReadLine()?.Split(' ');

            if (input == null || input.Length != 2 ||
                !int.TryParse(input[0], out int x) ||
                !int.TryParse(input[1], out int y))
            {
                Console.WriteLine("Invalid input! Press any key...");
                Console.ReadKey();
                continue;
            }

            bool success = game.MakeMove(new Position(x, y));

            if (!success)
            {
                Console.WriteLine(" Invalid move!");
            }

            Console.ReadKey();
        }

        Console.Clear();
        Console.WriteLine($" Winner: {game.GetWinner()!.Name}");
    }

    // ================= SETUP =================
    static void SetupPhase(GameController game, Player player)
    {
        Console.Clear();
        Console.WriteLine($"=== {player.Name} - Place Your Ships ===\n");

        foreach (ShipType type in System.Enum.GetValues(typeof(ShipType)))
        {
            bool placed = false;

            while (!placed)
            {
                Console.Clear();
                Console.WriteLine($"=== {player.Name} - Place Your Ships ===\n");
                Console.WriteLine($"Placing: {type}\n\n");
                
                Console.WriteLine("How to place ships:");
                Console.WriteLine("- Enter coordinates: x y (example: 3 4)");
                Console.WriteLine("- H = Horizontal (→), V = Vertical (↓)\n");

                PrintBoard(game.GetBoard(player), false);

                Console.Write("\nEnter position (x y): ");
                var posInput = Console.ReadLine()?.Split(' ');

                if (posInput == null || posInput.Length != 2 ||
                    !int.TryParse(posInput[0], out int x) ||
                    !int.TryParse(posInput[1], out int y))
                {
                    Console.WriteLine("Invalid input!");
                    Console.ReadKey();
                    continue;
                }

                Console.Write("Orientation [H = Horizontal →, V = Vertical ↓]: ");
                var ori = Console.ReadLine()?.ToUpper();

                if (ori != "H" && ori != "V")
                {
                    Console.WriteLine("Invalid orientation!");
                    Console.ReadKey();
                    continue;
                }

                var orientation = ori == "H"
                    ? Orientation.Horizontal
                    : Orientation.Vertical;

                placed = game.PlaceShip(player, type, new Position(x, y), orientation);

                if (placed)
                {
                    Console.Clear();
                    Console.WriteLine("Ship placed successfully!\n");

                    PrintBoard(game.GetBoard(player), false);

                    Console.WriteLine("\nPress any key...");
                    Console.ReadKey();
                }
                else
                {
                    Console.WriteLine("Invalid placement! Try again.");
                    Console.ReadKey();
                }
            }
        }

        Console.WriteLine("\nAll ships placed!");
        Console.WriteLine("Press any key to continue...");
        Console.ReadKey();
    }

    // ================= BOARD PRINT =================
    static void PrintBoard(IBoard board, bool hideShips)
    {
        // X axis
        Console.Write("  ");
        for (int x = 0; x < board.Size; x++)
            Console.Write(x + " ");
        Console.WriteLine();

        for (int y = 0; y < board.Size; y++)
        {
            Console.Write(y + " ");

            for (int x = 0; x < board.Size; x++)
            {
                var cell = board.GetCell(new Position(x, y));

                char c = '.';

                if (cell.State == CellState.Hit) c = 'X';
                else if (cell.State == CellState.Miss) c = 'O';
                else if (!hideShips && cell.Ship != null) c = 'S';

                Console.Write(c + " ");
            }

            Console.WriteLine();
        }
    }
}