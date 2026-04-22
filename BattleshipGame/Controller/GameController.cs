
using Battleship.Enum;
using Battleship.Interfaces;
using Battleship.Models;
namespace Battleship.Controller;

public class GameController : IGameController
{
    private Player _currentPlayer;
    private Player? _winner;
    private GameStatus _status;

    private Dictionary<Player, IBoard> _playerBoards;
    private Dictionary<Player, List<IShip>> _playerShips;

    public event Action<IPlayer>? OnTurnChanged;
    public event Action<ICell>? OnMoveProcessed;
    public event Action<IShip>? OnShipSunk;
    public event Action<IPlayer>? OnGameOver;

    public GameController(Player p1, Player p2)
    {
        _currentPlayer = p1;
        _status = GameStatus.Setup;

        _playerBoards = new Dictionary<Player, IBoard>
        {
            { p1, new Board(10) },
            { p2, new Board(10) }
        };
        _playerShips = new Dictionary<Player, List<IShip>>
        {
            { p1, new List<IShip>() },
            { p2, new List<IShip>() }
        };
    }

    public void StartGame()
    {
        if (_status != GameStatus.Setup) return;
        _status = GameStatus.InProgress;
        OnTurnChanged?.Invoke(_currentPlayer);
    }

    public bool PlaceShip(IPlayer player, ShipType shipType, Position position, Orientation orientation)
    {
        if (_status != GameStatus.Setup) return false;

        var p = (Player)player;
        if (!ValidatePlacement(p, shipType, position, orientation)) return false;

        var board = (Board)_playerBoards[p];
        int size = GetShipSize(shipType);

        var ship = new Ship(shipType, position, orientation);

        for (int i = 0; i < size; i++)
        {
            int x = orientation == Orientation.Horizontal ? position.X + i : position.X;
            int y = orientation == Orientation.Vertical ? position.Y + i : position.Y;

            var cell = (Cell)board.GetCell(new Position(x, y));

            cell.Ship = ship;
            cell.State = CellState.Occupied;
        }

        _playerShips[p].Add(ship);
        return true;
    }

    public bool MakeMove(Position position)
    {
        if (_status != GameStatus.InProgress) return false;
        if (!ValidateAttack(position)) return false;

        var opponent = (Player)GetOpponent();
        var board = (Board)_playerBoards[opponent];
        var cell = (Cell)board.GetCell(position);

        bool isHit = cell.Ship != null;

        cell.State = isHit ? CellState.Hit : CellState.Miss;

        if (isHit)
        {
            var ship = (Ship)cell.Ship!;
            ship.Hits++;
            UpdateShipStatus(ship);
        }

        OnMoveProcessed?.Invoke(cell);

        if (!CheckWinner() && !isHit)
            SwitchTurn();

        return true;
    }

    public void EndGame() => _status = GameStatus.End;
    public GameStatus GetStatus() => _status;
    public IPlayer? GetWinner() => _winner;
    public IPlayer GetCurrentPlayer() => _currentPlayer;
    public IPlayer GetOpponent()
    {
        return _playerBoards.Keys.First(p => p != _currentPlayer);
    }
    public IBoard GetBoard(IPlayer p) => _playerBoards[(Player)p];
    public IReadOnlyList<IShip> GetShips(IPlayer p)
    {
        return _playerShips[(Player)p];
    }

    private int GetShipSize(ShipType shipType) => shipType switch
    {
        ShipType.Carrier => 5,
        ShipType.Battleship => 4,
        ShipType.Cruiser => 3,
        ShipType.Destroyer => 3,
        ShipType.PatrolBoat => 2,
        _ => 0
    };

    private bool IsInsideBoard(Position position)
    {
        int size = _playerBoards.Values.First().Size;
        
        return position.X >= 0 && position.X < size && position.Y >= 0 && position.Y < size;
    }

    private bool IsCellOccupied(Board board, Position position)
    {
        return board.GetCell(position).State == CellState.Occupied;
    }

    private bool ValidatePlacement(Player player, ShipType shipType, Position position, Orientation orientation)
    {
        int size = GetShipSize(shipType);
        var board = (Board)_playerBoards[player];

        for (int i = 0; i < size; i++)
        {
            int x = orientation == Orientation.Horizontal ? position.X + i : position.X;
            int y = orientation == Orientation.Vertical ? position.Y + i : position.Y;
            var pos = new Position(x, y);

            if (!IsInsideBoard(pos) || IsCellOccupied(board, pos)) 
                return false;
        }
        return true;
    }

    private bool ValidateAttack(Position pos)
    {
        if (!IsInsideBoard(pos)) return false;
        var board = (Board)_playerBoards[(Player)GetOpponent()];
        var cell = board.GetCell(pos);
        return cell.State != CellState.Hit && cell.State != CellState.Miss;
    }

    private void UpdateShipStatus(IShip s)
    {
        if (s.Hits >= s.Size) OnShipSunk?.Invoke(s);
    }

    private bool CheckWinner()
    {
        var opponent = (Player)GetOpponent();
        if (_playerShips[opponent].All(s => s.Hits >= s.Size))
        {
            _winner = _currentPlayer;
            _status = GameStatus.End;
            OnGameOver?.Invoke(_winner);
            return true;
        }
        return false;
    }

    private void SwitchTurn()
    {   
        _currentPlayer = _playerBoards.Keys
            .First(p => p != _currentPlayer);

        OnTurnChanged?.Invoke(_currentPlayer);
    }
}