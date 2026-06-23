using DominoGame.Enums;
using DominoGame.Interfaces;

namespace DominoGame;

public class GameLogic
{
    private Dictionary<IPlayer, List<IDomino>> _hands;
    private List<IPlayer> _players;
    private IBoard _board;
    private IPlayer _winner;
    private IDrawPile _drawPile;
    private int _currentPlayerIndex;
    private int _consecutiveSkips;
    private GameStatus _gameStatus;
    private WinCondition _winCondition;
    
    // events
    public event EventHandler TurnChanged;
    public event EventHandler GameEnded;

    public GameLogic(List<IPlayer> players, IDrawPile drawPile)
    {
        _players = players;
        _drawPile = drawPile;
    }
    
    // Lifecycle (public)
    public void StartGame()
    {
        Console.WriteLine("Game started"); // sementara
    }

    public IPlayer GetWinner()
    {
        return _winner;
    }

    public WinCondition GetWinConditions()
    {
        return _winCondition;
    }
    
    // Actions (public)
    public TurnAction PlayTurn(
        IPlayer player,
        IDomino domino,
        PlacementSide side,
        PlacementOrientation orientation
    )
    {
        Console.WriteLine($"{player.Name} plays a domino with turn action");
        return TurnAction.PlayDomino; // sementara
    }

    public void DrawCard(IPlayer player)
    {
        Console.WriteLine($"{player.Name} draw a card"); // sementara
    }

    public void AdvanceTurn()
    {
        Console.WriteLine("Advance Turn"); // sementara
    }
    
    // Get Info Game and Turn
    public GameStatus GetStatus()
    {
        return _gameStatus;
    }

    public IPlayer GetCurrentPlayer()
    {
        return _players[_currentPlayerIndex]; // sementara (tapi tergantung)
    }
    
    // Get Info Player and Hand
    public List<IPlayer> GetPlayers()
    {
        return _players;
    }

    public List<IDomino> GetPlayerHands(IPlayer player)
    {
        return _hands[player]; // sementara (tapi tergantung)
    }

    public int GetRemainingPips(IPlayer player)
    {
        return 0; // sementara
    }
    
    // Get Info Board
    public IBoard GetBoard()
    {
        return _board;
    }

    public List<int> GetOpenEndPips()
    {
        return new List<int>(); // sementara
    }

    public int GetDrawPileCount()
    {
        return _drawPile.Dominoes.Count; // sementara (tapi tergantung)
    }
    
    // Business Logic Setup (Private)
    private void ShuffleDrawPile(){}
    private void DealInitialHands(){}
    private void DetermineStartingPlayer(){}

    private IDomino GetHighestDouble(IPlayer player)
    {
        return null; // sementara
    }

    private bool HasAnyDouble()
    {
        return false; // sementara
    }
    private void RedealAllHands(){}
    
    // Business Logic Turn and Placement (Private)
    private bool ValidatePlacement(IDomino domino, PlacementSide side)
    {
        return false; // sementara
    }
    private void PlaceDomino(IDomino domino, PlacementSide side, PlacementOrientation orientation){}

    private bool HasPlayableDomino(IPlayer player)
    {
        return false; // sementara
    }
    
    // Business Logic Pile and Hand (Private)
    private IDomino DrawFromPile()
    {
        return null; // sementara
    }
    private void AddDominoToHand(IPlayer player, IDomino domino){}
    private void RemoveDominoFromHand(IPlayer player, IDomino domino){}
    private void DrawUntilPlayable(IPlayer player){}
    
    // Business Logic Game End and Resolution (Private)
    private bool CheckGameOver()
    {
        return false; // sementara
    }

    private bool IsGameBlocked()
    {
        return false; // sementara
    }

    private bool IsDrawPileEmpty()
    {
        return false; // sementara
    }

    private int CalculateRemainingPips(IPlayer player)
    {
        return 0; // sementara
    }

    private IPlayer DetermineWinner()
    {
        return null; // sementara
    }
    private void EndGame(){}
    
}