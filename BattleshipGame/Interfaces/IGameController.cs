using Battleship.Models;
using Battleship.Enum;


namespace Battleship.Interfaces;

public interface IGameController
{

    //Bakal di trigger when turn changed from one player to another
    event Action<IPlayer>? OnTurnChanged;
    // Ter trigger after a move is made, inituh attack result for U/Game flow
    event Action<ICell>? OnMoveProcessed;
    //Trigger when a ship is completely destroyed
    event Action<IShip>? OnShipSunk;
    // Trigger when a game ends and winner is decided
    event Action<IPlayer>? OnGameOver;

    // Start the game and moves status from SETUP to IN_PROGRESS
    void StartGame();
    // Place ship on board
    bool PlaceShip(IPlayer player, ShipType shipType, Position position, Orientation orientation);
    // execute attack on opponent board
    bool MakeMove(Position position);
    // end game 
    void EndGame();

    // Returns current active player
    IPlayer GetCurrentPlayer();
    // Returns opponent of current player
    IPlayer GetOpponent();
    // Returns board of a specific player
    IBoard GetBoard(IPlayer p);
    IReadOnlyList<IShip> GetShips(IPlayer p);
    // Returns current game status
    GameStatus GetStatus();
    // Returns winner if game is finished, otherwise null
    IPlayer? GetWinner();
}