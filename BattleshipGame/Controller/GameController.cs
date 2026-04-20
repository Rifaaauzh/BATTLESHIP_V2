using System.Reflection.Metadata.Ecma335;
using Battleship.Enum;
using Battleship.Interfaces;
namespace Battleship.Models;

public class GameController : IGameController
{
    private Player _currentPlayer;
    private Player? _winner;
    private GameStatus _status;

    private readonly Dictionary<Player, IBoard> _playerBoards;
    private readonly Dictionary<Player, List<IShip>> _playerShips;

    public event Action<IPlayer>? OnTurnChanged;
    public event Action<ICell>? OnMoveProcessed;
    public event Action<IShip>? OnShipSunk;
    public event Action<IPlayer>? OnGameOver;


    // Constructor of the game
    public GameController(Player p1, Player p2)
    {
        _playerBoards = new Dictionary<Player, IBoard>
        {
            {p1, new Board(10) },
            {p2, new Board(10) }
        };
        _playerShips = new Dictionary<Player, List<IShip>>
        {
           { p1, new List<IShip>() },
           { p2, new List<IShip>() }
        };

        _currentPlayer = p1;
        _status = GameStatus.Setup;
    }

    // Ini nanti dipanggil setelah ship diletakkan semua, dan start game
    public void StartGame()
    {
        if (_status != GameStatus.Setup)
            return;
        
        _status = GameStatus.InProgress;

        OnTurnChanged?.Invoke(_currentPlayer);
    }

    // Handle attack logic nya
    public bool PlaceShip(IPlayer player, ShipType shipType, Position position, Orientation orientation)
    {
        /* Flow logic:
        Ambil board player, validasi posisi dalam board, validasi placement, buat ship sesuai type, letakkan ke cell board
        simpan ship ke player dan return true */

       if (_status != GameStatus.Setup)
          return false;
        
        var play = (Player)player;

        var board = _playerBoards[(Player)player];

        if (!IsInsideBoard(position))
            return false;

        if (!ValidatePlacement((Player)player, shipType, position, orientation))
            return false;
        
        return true;
    }
    //handle attack logic nya tu disini akan memanggil 
    public bool MakeMove(Position position)
    {
        return true;
    }
    //End game and trigger gameover event nya
    public void EndGame(){}
    //return current game status
    public GameStatus GetStatus()=>_status;
    // return current player (sekarang giliran siapa)
    public IPlayer? GetWinner()=>_winner;
    // returns current player (whose turn it is)
    public IPlayer GetCurrentPlayer() => _currentPlayer;
    // returns opponent player
    public IPlayer GetOpponent() 
    {
        // Ambil semua pemain dari dictionary
        var players = _playerBoards.Keys.ToList();
        
        // Kembalikan pemain yang bukan _currentPlayer
        return _currentPlayer == players[0] ? players[1] : players[0];
    }
    // returns board of selected player
    public IBoard GetBoard(IPlayer p) => _playerBoards[(Player)p];
    // returns all ships of selected player
    public List<IShip> GetShips(IPlayer p) => _playerShips[(Player)p];

    // Tempatkan ini di bagian paling bawah class GameController
    private int GetShipSize(ShipType shipType)
    {
        return shipType switch
        {
            ShipType.Carrier => 5,
            ShipType.Battleship => 4,
            ShipType.Cruiser => 3,
            ShipType.Destroyer => 3,
            ShipType.PatrolBoat => 2,
            _ => 0
        };
    }

    // checks if position is inside board boundaries
    private bool IsInsideBoard(Position position)
    {
        // Ambil board dari dictionary untuk tau ukurannya
        // (Asumsinya semua pemain punya ukuran board yang sama)
        var board = _playerBoards.Values.First(); 
        
        return position.X >= 0 && position.X < board.Size && 
            position.Y >= 0 && position.Y < board.Size;
    }
    // checks if a cell is already occupied by a ship
    private bool IsCellOccupied(Board board, Position position)
    {
        var cell = (Cell)board.GetCell(position);
        return cell.State == CellState.Occupied;
    }
    // validates if ship placement is legal
    private bool ValidatePlacement(Player player, ShipType shipType, Position position, Orientation orientation)
    {
        int size = GetShipSize(shipType);
        var board = (Board)_playerBoards[player];

        for (int i = 0; i < size; i++)
        {
            int x = orientation == Orientation.Horizontal ? position.X + i : position.X;
            int y = orientation == Orientation.Vertical ? position.Y + i : position.Y;
            Position checkPos = new Position(x, y);

            if (!IsInsideBoard(checkPos) || IsCellOccupied(board, checkPos))
                return false;
        }
        return true;
    }
    // validates if attack position is valid
    private bool ValidateAttack(Position pos)
    {
        if (!IsInsideBoard(pos)) return false;
    
        var opponentBoard = (Board)_playerBoards[(Player)GetOpponent()];
        var cell = (Cell)opponentBoard.GetCell(pos);
        
        // hanya boleh ditembak jika belum pernah ditembak (Hit atau Miss)
        return cell.State != CellState.Hit && cell.State != CellState.Miss;
    }
    private void ApplyAttack(Player target, Position pos)
    {
    } 
    // updates ship status after hit
    private void UpdateShipStatus(Ship s) 
    {
        if (s.Hits >= s.Size)
            OnShipSunk?.Invoke(s);
    }
    // checks if there is a winner
    private bool CheckWinner()
    {
        // Siapa lawan kita?
        var opponent = (Player)GetOpponent();
        var opponentShips = _playerShips[opponent];

        // Cek apakah ada kapal yang belum tenggelam
        // Jika semua kapal (All) sudah tenggelam (Hits >= Size), maka menang
        bool allShipsSunk = opponentShips.All(s => s.Hits >= s.Size);

        if (allShipsSunk)
        {
            _winner = _currentPlayer; // Set pemenang
            _status = GameStatus.End;
            OnGameOver?.Invoke(_winner); // Trigger event
            return true;
        }

        return false;
    }
    // switches turn to next player
    private void SwitchTurn()
    {
        // Ambil list player dari Dictionary
        var players = _playerBoards.Keys.ToList();

        // Kalau current adalah player pertama, ganti ke player kedua. 
        // Kalau bukan, balik ke player pertama.
        _currentPlayer = (_currentPlayer == players[0]) ? players[1] : players[0];

        // Beritau sistem kalau giliran sudah ganti
        OnTurnChanged?.Invoke(_currentPlayer);
    }
    
}