using Battleship.Models;
using Battleship.Enum;
using Battleship.Interfaces;
using Battleship.Controller;

namespace Battleship;

class Program
{
    static IPlayer? _lastAttacker;
    static IPlayer? _lastDefender;

    static void Main()
    {
        ShowTitle();

        Console.WriteLine("Select Game Mode:");
        Console.WriteLine("1. Two Player");
        Console.WriteLine("2. Play vs Bot (Jimmie)");
        Console.Write("Choose (1/2): ");

        string modeInput = Console.ReadLine()!;
        bool isBotMode = modeInput == "2";

        IPlayer p1 = ReadPlayer("Enter Player 1 name: ");
        IPlayer p2;

        if (isBotMode)
        {
            p2 = new Player("Jimmie");
            ShowMessage("Bot selected: Jimmie", ConsoleColor.Cyan);
        }
        else
        {
            p2 = ReadPlayer("Enter Player 2 name: ");
        }

        Board board1 = new Board(10);
        Board board2 = new Board(10);

        List<Ship> ships1 = new List<Ship>();
        List<Ship> ships2 = new List<Ship>();

        IGameController game = new GameController( (Player)p1, (Player)p2, board1, board2, ships1, ships2, isBotMode );

        RegisterEvents(game);

        RunSetupPhase(game, p1);

        if (isBotMode)
        {
            AutoPlaceBotShips(game, p2);
        }
        else
        {
            RunSetupPhase(game, p2);
        }

        if (!game.StartGame())
        {
            ShowMessage("Game could not be started.", ConsoleColor.Yellow);
            Wait();
            return;
        }

        RunMainGameLoop(game, isBotMode);

        Console.Clear();
        Console.WriteLine("====================================");

        IPlayer? winner = game.GetWinner();

        if (winner != null)
        {
            Console.WriteLine($"FINAL WINNER: {winner.Name}");
        }
        else
        {
            Console.WriteLine("FINAL WINNER: Unknown");
        }

        Console.WriteLine("====================================");
        Wait();
    }

    static IPlayer ReadPlayer(string prompt)
    {
        Console.Write(prompt);
        return new Player(Console.ReadLine()!);
    }

    static void RegisterEvents(IGameController game)
    {
        game.OnTurnChanged += player =>
        {
            ShowMessage($"Now playing: {player.Name}", ConsoleColor.Cyan);
        };

        game.OnMoveProcessed += cell =>
        {
            if (cell.State == CellState.Hit)
            {
                ShowMessage(
                    $"{_lastAttacker?.Name} hit {_lastDefender?.Name}'s ship!",
                    ConsoleColor.Red
                );
            }
            else if (cell.State == CellState.Miss)
            {
                ShowMessage(
                    $"{_lastAttacker?.Name} missed!",
                    ConsoleColor.Yellow
                );
            }
        };

        game.OnShipSunk += ship =>
        {
            ShowMessage(
                $"{_lastDefender?.Name}'s {ship.ShipType} was sunk!",
                ConsoleColor.Magenta
            );
        };

        game.OnGameOver += player =>
        {
            ShowMessage($"Game Over! Winner: {player.Name}", ConsoleColor.Green);
        };
    }

    static void RunSetupPhase(IGameController game, IPlayer player)
    {
        foreach (ShipType type in System.Enum.GetValues(typeof(ShipType)))
        {
            bool placed = false;

            while (!placed)
            {
                Console.Clear();
                DrawSetupScreen(game, player, type);

                Position position = ReadPosition("\nPosition (x y): ");
                Orientation orientation = ReadOrientation("Orientation (H/V): ");

                placed = game.PlaceShip(player, type, position, orientation);

                if (placed)
                {
                    ShowMessage("Ship placed successfully!", ConsoleColor.Green);
                }
                else
                {
                    ShowMessage(
                        "Invalid placement. Ship may be outside the board or overlapping another ship.",
                        ConsoleColor.Yellow
                    );
                }

                Wait();
            }
        }

        ShowMessage($"{player.Name} has placed all ships!", ConsoleColor.Green);
        Wait();
    }

    static void AutoPlaceBotShips(IGameController game, IPlayer botPlayer)
    {
        Random random = new Random();

        foreach (ShipType type in System.Enum.GetValues(typeof(ShipType)))
        {
            bool placed = false;

            while (!placed)
            {
                int x = random.Next(0, 10);
                int y = random.Next(0, 10);

                Position position = new Position(x, y);

                Orientation orientation;

                if (random.Next(0, 2) == 0)
                {
                    orientation = Orientation.Horizontal;
                }
                else
                {
                    orientation = Orientation.Vertical;
                }

                placed = game.PlaceShip(botPlayer, type, position, orientation);
            }
        }

        ShowMessage($"{botPlayer.Name} has placed all ships automatically!", ConsoleColor.Green);
        Wait();
    }

    static void RunMainGameLoop(IGameController game, bool isBotMode)
    {
        while (game.GetStatus() != GameStatus.End)
        {
            Console.Clear();

            IPlayer currentPlayer = game.GetCurrentPlayer();
            IPlayer opponentPlayer = game.GetOpponent();

            DrawBattleScreen(game, currentPlayer, opponentPlayer);

            Position attackPosition = ReadPosition("\nAttack (x y): ");

            _lastAttacker = currentPlayer;
            _lastDefender = opponentPlayer;

            bool success = game.MakeMove(attackPosition);

            if (!success)
            {
                ShowMessage(
                    "Invalid move. The position may be outside the board or already attacked.",
                    ConsoleColor.Yellow
                );

                Wait();
                continue;
            }

            Wait();

            while (
                isBotMode &&
                game.GetStatus() != GameStatus.End &&
                game.GetCurrentPlayer().Name == "Jimmie"
            )
            {
                Console.Clear();

                ShowMessage("Jimmie is thinking...", ConsoleColor.Cyan);

                _lastAttacker = game.GetCurrentPlayer();
                _lastDefender = game.GetOpponent();

                bool botSuccess = game.MakeBotMove();

                if (!botSuccess)
                {
                    ShowMessage("Jimmie could not make a move.", ConsoleColor.Yellow);
                    break;
                }

                Wait();
            }
        }
    }

    static void ShowTitle()
    {
        Console.WriteLine("============================================================================================\n");
        Console.WriteLine("                                   BATTLESHIP GAME                                          \n");
        Console.WriteLine("============================================================================================\n");
    }

    static void DrawSetupScreen(IGameController game, IPlayer player, ShipType currentShip)
    {
        Console.WriteLine("====================================");
        Console.WriteLine($"{player.Name} - Place Your Ships");
        Console.WriteLine("====================================\n");

        Console.WriteLine($"Current ship: {currentShip}");
        Console.WriteLine("Enter position in format: x y");
        Console.WriteLine("Example: 3 4");
        Console.WriteLine("Orientation: H = Horizontal, V = Vertical\n");

        PrintBoard(game.GetBoard(player), false);
    }

    static void DrawBattleScreen(IGameController game, IPlayer currentPlayer, IPlayer opponentPlayer)
    {
        Console.WriteLine("====================================");
        Console.WriteLine($"TURN: {currentPlayer.Name}");
        Console.WriteLine("====================================\n");

        Console.WriteLine("Enemy Board");
        PrintBoard(game.GetBoard(opponentPlayer), true);

        Console.WriteLine();

        Console.WriteLine("Your Board");
        PrintBoard(game.GetBoard(currentPlayer), false);

        Console.WriteLine();
        Console.WriteLine("Enter attack position in format: x y");
        Console.WriteLine("Example: 3 4");
    }

    static Position ReadPosition(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string[] input = Console.ReadLine()!.Split(' ');

            if (TryParsePosition(input, out Position position))
            {
                return position;
            }

            ShowMessage("Input is invalid. Use format: x y", ConsoleColor.Yellow);
        }
    }

    static Orientation ReadOrientation(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string input = Console.ReadLine()!.Trim().ToUpper();

            if (TryParseOrientation(input, out Orientation orientation))
            {
                return orientation;
            }

            ShowMessage("Orientation is invalid. Use H or V.", ConsoleColor.Yellow);
        }
    }

    static bool TryParsePosition(string[]? input, out Position position)
    {
        position = new Position(0, 0);

        if (input == null || input.Length != 2)
        {
            return false;
        }

        if (!int.TryParse(input[0], out int x) || !int.TryParse(input[1], out int y))
        {
            return false;
        }

        position = new Position(x, y);
        return true;
    }

    static bool TryParseOrientation(string? input, out Orientation orientation)
    {
        orientation = Orientation.Horizontal;

        if (input == "H")
        {
            orientation = Orientation.Horizontal;
            return true;
        }

        if (input == "V")
        {
            orientation = Orientation.Vertical;
            return true;
        }

        return false;
    }

    static void PrintBoard(IBoard board, bool hideShips)
    {
        Console.Write("  ");

        for (int x = 0; x < board.Size; x++)
        {
            Console.Write(x + " ");
        }

        Console.WriteLine();

        for (int y = 0; y < board.Size; y++)
        {
            Console.Write(y + " ");

            for (int x = 0; x < board.Size; x++)
            {
                ICell cell = board.GetCell(new Position(x, y));
                char symbol = '.';

                if (cell.State == CellState.Hit)
                {
                    symbol = 'X';
                }
                else if (cell.State == CellState.Miss)
                {
                    symbol = 'O';
                }
                else if (!hideShips && cell.Ship != null)
                {
                    symbol = 'S';
                }

                Console.Write(symbol + " ");
            }

            Console.WriteLine();
        }
    }

    static void ShowMessage(string message, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    static void Wait()
    {
        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey();
    }
}