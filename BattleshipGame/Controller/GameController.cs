using Battleship.Enum;
using Battleship.Interfaces;
using Battleship.Models;
namespace Battleship.Controller;

public class GameController : IGameController
{
    private Player _currentPlayer;
    private Player? _winner;
    private GameStatus _status;

    private Dictionary<Player, Board> _playerBoards;
    private Dictionary<Player, List<Ship>> _playerShips;

    public event Action<IPlayer>? OnTurnChanged;
    public event Action<ICell>? OnMoveProcessed;
    public event Action<IShip>? OnShipSunk;
    public event Action<IPlayer>? OnGameOver;

    public GameController(Player p1, Player p2)
    {
        _currentPlayer = p1;
        _status = GameStatus.Setup;

       _playerBoards = new Dictionary<Player, Board>
        {
            { p1, new Board(10) },
            { p2, new Board(10) }
        };

       _playerShips = new Dictionary<Player, List<Ship>>
        {
            { p1, new List<Ship>() },
            { p2, new List<Ship>() }
        };
    }

    public bool StartGame()
    {
        if (_status != GameStatus.Setup) 
            return false;

        _status = GameStatus.InProgress;
        OnTurnChanged?.Invoke(_currentPlayer);
        
        return true;
    }

    public bool PlaceShip(IPlayer player, ShipType shipType, Position position, Orientation orientation)
    {
        if (_status != GameStatus.Setup)
            return false;

        var p = (Player)player;
        var board = _playerBoards[p];
        var ship = new Ship(shipType, position, orientation);

        if (!ValidatePlacement(p, ship))
            return false;

        PlaceShipOnBoard(board, ship);
        _playerShips[p].Add(ship);

        return true;
    }
    

    public bool MakeMove(Position position)
    {
        if (_status != GameStatus.InProgress) 
            return false;
        
        if (!ValidateAttack(position)) 
            return false;

        var opponent = (Player)GetOpponent();
        var board = (Board)_playerBoards[opponent];
        var cell = (Cell)board.GetCell(position);

        bool isHit = cell.Ship != null;

        cell.State = isHit ? CellState.Hit : CellState.Miss;

        if (isHit)
        {
            var ship = (Ship)cell.Ship!;
            ship.Hits++;
            CheckShipSunk(ship);
        }

        OnMoveProcessed?.Invoke(cell);

        if (!CheckWinner() && !isHit)
            SwitchTurn();

        return true;
    }
    private Position GetShipPosition(Position start, Orientation orientation, int i)
    {
        int x = orientation == Orientation.Horizontal ? start.X + i : start.X;
        int y = orientation == Orientation.Vertical ? start.Y + i : start.Y;

        return new Position(x, y);
    }
    private void PlaceShipOnBoard(Board board, Ship ship)
    {
        for (int i = 0; i < ship.Size; i++)
        {
            var pos = GetShipPosition(ship.Position, ship.Orientation, i);

            var cell = (Cell)board.GetCell(pos);
            cell.Ship = ship;
            cell.State = CellState.Occupied;
        }
    }
    public void EndGame()
    {
        if (_status == GameStatus.End)
            return;

        _status = GameStatus.End;

        if (_winner != null)
            OnGameOver?.Invoke(_winner);
    }

    public GameStatus GetStatus()
    {
        return _status;
    }

    public IPlayer? GetWinner()
    {
        return _winner;
    }

    public IPlayer GetCurrentPlayer()
    {
        return _currentPlayer;
    }

    public IPlayer GetOpponent()
    {
        return _playerBoards.Keys.First(player => player != _currentPlayer);
    }

    public IBoard GetBoard(IPlayer player)
    {
        return _playerBoards[(Player)player];
    }
    

    public IReadOnlyList<IShip> GetShips(IPlayer player)
    {
        return _playerShips[(Player)player];
    }
    
    private bool IsInsideBoard(Position position)
    {
        int size = _playerBoards.Values.First().Size;

        return position.X >= 0 && position.X < size &&
               position.Y >= 0 && position.Y < size;
    }

    private bool IsCellOccupied(Board board, Position position)
    {
        return board.GetCell(position).Ship != null;
    }

    private bool ValidatePlacement(Player player, Ship ship)
    {
        var board = _playerBoards[player];

        for (int i = 0; i < ship.Size; i++)
        {
            var pos = GetShipPosition(ship.Position, ship.Orientation, i);

            if (!IsInsideBoard(pos) || IsCellOccupied(board, pos))
                return false;
        }

        return true;
    }

    private bool ValidateAttack(Position pos)
    {
        if (!IsInsideBoard(pos)) 
            return false;
        
        var opponent = (Player)GetOpponent();
        var board = (Board)_playerBoards[opponent];
        
        var cell = board.GetCell(pos);
        
        return cell.State != CellState.Hit && cell.State != CellState.Miss;
    }

    private void CheckShipSunk (IShip s)
    {
        if (s.Hits >= s.Size) 
            OnShipSunk?.Invoke(s);
    }

    private bool CheckWinner()
    {
        var opponent = (Player)GetOpponent();

        if (_playerShips[opponent].All(ship => ship.Hits >= ship.Size))
        {
            _winner = _currentPlayer;
            EndGame();
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