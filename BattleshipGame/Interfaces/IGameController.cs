using Battleship.Models;
using Battleship.Enum;


namespace Battleship.Interfaces;

public interface IGameController
{
    event Action<IPlayer>? OnTurnChanged;
    event Action<ICell>? OnMoveProcessed;
    event Action<IShip>? OnShipSunk;
    event Action<IPlayer>? OnGameOver;

    void StartGame();
    bool PlaceShip(IPlayer player, ShipType shipType, Position position, Orientation orientation);
    bool MakeMove(Position position);
    void EndGame();

    IPlayer GetCurrentPlayer();
    IPlayer GetOpponent();
    IBoard GetBoard(IPlayer player);
    IReadOnlyList<IShip> GetShips(IPlayer player);
    GameStatus GetStatus();
    IPlayer? GetWinner();
}