using DominoGame.Entities;
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
        _hands = new Dictionary<IPlayer, List<IDomino>>();
    }
    
    // Lifecycle (public)
    public void StartGame()
    {
        // validasi jumlah pemain
        if (_players.Count < 2 || _players.Count > 8)
        {
            throw new ArgumentException("Players minimal 2 dan maksimal 8", nameof(_players));
        }

        // kocok pile
        ShuffleDrawPile();

        // pembagian kartu awal
        DealInitialHands();

        // ulangi bagi-ulang SELAMA belum ada double
        while (HasAnyDouble() == false)
        {
            RedealAllHands();
        }

        // tentukan pemain pembuka (double tertinggi)
        DetermineStartingPlayer();

        // buat papan kosong
        _board = new Board(new List<IDomino>());

        // set status berjalan
        _gameStatus = GameStatus.InProgress;

        // (opsional) reset penghitung skip
        _consecutiveSkips = 0;

        // beritahu giliran dimulai
        TurnChanged?.Invoke(this, EventArgs.Empty);
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
    private void ShuffleDrawPile()
    {
        List<IDomino> dominoes = _drawPile.Dominoes;
        Random rnd = new Random();
        
        for (int i = dominoes.Count - 1; i > 0; i--)
        {
            int j = rnd.Next(i + 1);
            (dominoes[i], dominoes[j]) = (dominoes[j], dominoes[i]);
        }
    }

    private void DealInitialHands()
    {
        // konstanta jumlah kartu per pemain
        const int cardsPerPlayer = 7;

        // loop tiap pemain
        foreach (IPlayer player in _players)
        {
            // mendaftarkan pemain ke dictionary sebelum diisi
            _hands[player] = new List<IDomino>();

            // loop sebanyak cards per-player
            for (int i = 0; i < cardsPerPlayer; i++)
            {
                // ambil 1 domino
                IDomino? domino = DrawFromPile();
                
                // tambahkan ke hand
                AddDominoToHand(player, domino);
            }
        }
    }

    private void DetermineStartingPlayer()
    {
        // penampung 
        int startingPlayerIndex = -1;
        IDomino? highestOverall = null;

        // loop tiap pemain dengan index
        for (int i = 0; i < _players.Count; i++)
        {
            // untuk tiap pemain, ambil double yg tertinggi
            IDomino? playerDouble = GetHighestDouble(_players[i]);
            
            // jika playerDOuble null, SKIP
            if (playerDouble == null) continue;

            // jika punya highest overall belum ada (null) atau player double lebih tinggi maka update
            if (highestOverall == null || playerDouble.LeftPips > highestOverall.LeftPips)
            {
                highestOverall = playerDouble;
                startingPlayerIndex = i;
            }
        }
        
        _currentPlayerIndex = startingPlayerIndex;
    }

    private IDomino? GetHighestDouble(IPlayer player)
    {
        // variable penampung highest
        IDomino? highestDomino = null;

        // loop tiap domino
        foreach (IDomino domino in _hands[player])
        {
            if (domino.IsDouble)
            {
                // apakah ini double pertama (highest masih null) ATAU lebih tinggi?
                if (highestDomino == null || domino.LeftPips > highestDomino.LeftPips)
                {
                    // update highest domino
                    highestDomino = domino;
                }
            }
        }
        
        // return highest
        return highestDomino;
    }

    private bool HasAnyDouble()
    {
        // loop tiap pemain
        foreach (IPlayer player in _players)
        {
            // untuk setiap pemain, loop domino 
            foreach (IDomino domino in _hands[player])
            {
                // jika domino double
                if (domino.IsDouble)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private void RedealAllHands()
    {
        foreach (IPlayer player in _players)
        {
            // kembalikan kartu dari tangan pemain ke pile
            _drawPile.Dominoes.AddRange(_hands[player]);
        }
        
        // kocok ulang 
        ShuffleDrawPile();
        
        // bagi ulang
        DealInitialHands();
    }
    
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
    private IDomino? DrawFromPile()
    {
        // cek pile apakah kosong
        if (IsDrawPileEmpty())
        {
            return null;
        }

        // tentukan indeks domino yang diambil
        int lastIndex = _drawPile.Dominoes.Count - 1;

        // ambil domino di indeks, simpan ke variabel
        IDomino drawnDomino = _drawPile.Dominoes[lastIndex];

        // hapus domino dari pile sesuai index
        _drawPile.Dominoes.RemoveAt(lastIndex);

        // return domino yang di ambil
        return drawnDomino;
    }

    private void AddDominoToHand(IPlayer player, IDomino domino)
    {
        List<IDomino> hand = _hands[player];
        hand.Add(domino);
    }
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
        return _drawPile.Dominoes.Count == 0;
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