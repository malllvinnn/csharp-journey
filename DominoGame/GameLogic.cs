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
    public void StartGame(){}

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
    ){}
    public void DrawCard(IPlayer player){}
    public void AdvanceTurn(){}
    
    // Get Info Game and Turn
    public GameStatus GetStatus()
    {
        return _gameStatus;
    }

    public IPlayer GetCurrentPlayer(){}
    
    // Get Info Player and Hand
    public List<IPlayer> GetPlayers()
    {
        return _players;
    }
    public List<IDomino> GetPlayerHands(IPlayer player){}

    public int GetRemainingPips(IPlayer player)
    {
        return 0;
    }
    
    // Get Info Board
    public IBoard GetBoard()
    {
        return _board;
    }
    public List<int> GetOpenEndPips(){}

    public int GetDrawPileCount()
    {
        return 0;
    }
    
    // Business Logic Setup (Private)
    private void ShuffleDrawPile(){}
    private void DealInitialHands(){}
    private void DetermineStartingPlayer(){}
    private IDomino GetHighestDouble(IPlayer player){}
    private bool HasAnyDouble(){}
    private void RedealAllHands(){}
    
    // Business Logic Turn and Placement (Private)
    private bool ValidatePlacement(IDomino domino, PlacementSide side)
    {
        return false;
    }
    private void PlaceDomino(IDomino domino, PlacementSide side, PlacementOrientation orientation){}
    private bool HasPlayableDomino(IPlayer player){}
    
    // Business Logic Pile and Hand (Private)
    private IDomino DrawFromPile(){}
    private void AddDominoToHand(IPlayer player, IDomino domino){}
    private void RemoveDominoFromHand(IPlayer player, IDomino domino){}
    private void DrawUntilPlayable(IPlayer player){}
    
    // Business Logic Game End and Resolution (Private)
    private bool CheckGameOver(){}

    private bool IsGameBlocked()
    {
        return false;
    }
    private bool IsDrawPileEmpty(){}
    private int CalculateRemainingPips(IPlayer player){}
    private IPlayer DetermineWinner(){}
    private void EndGame(){}
    
}