using DominoGame.Entities;
using DominoGame.Enums;
using DominoGame.Interfaces;

namespace DominoGame;

public class GameLogic
{
    private readonly Dictionary<IPlayer, List<IDomino>> _hands;
    private List<IPlayer> _players;
    private IBoard _board;
    private IPlayer? _winner;
    private readonly IDrawPile _drawPile;
    private int _currentPlayerIndex;
    private int _consecutiveSkips;
    private GameStatus _gameStatus;
    private WinCondition _winCondition;

    // events
    public event EventHandler? TurnChanged;
    public event EventHandler? GameEnded;

    public GameLogic(List<IPlayer> players, IDrawPile drawPile, IBoard board)
    {
        _players = players;
        _drawPile = drawPile;
        _board = board;
        _hands = new Dictionary<IPlayer, List<IDomino>>();
    }

    // Lifecycle (public)
    public void StartGame()
    {
        // jika pemain tidak valid, tidak memulai game
        if (_players.Count < 2 || _players.Count > 8)
        {
            _gameStatus = GameStatus.NotStarted;
            return;
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
        // _board = new Board(new List<IDomino>());

        // set status berjalan
        _gameStatus = GameStatus.InProgress;

        // (opsional) reset penghitung skip
        _consecutiveSkips = 0;

        // beritahu giliran dimulai
        TurnChanged?.Invoke(this, EventArgs.Empty);
    }

    public IPlayer? GetWinner()
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
        // jika tidak valid, tidak melakukan apa apa
        if (!ValidatePlacement(domino, side))
        {
            return TurnAction.PlayDomino;
        }


        // jika valid taruh domino ke papan
        PlaceDomino(domino, side, orientation);

        // hapus domino dari tangan pemain
        RemoveDominoFromHand(player, domino);

        // reset skip
        _consecutiveSkips = 0;

        // cehck game over
        if (CheckGameOver())
        {
            EndGame();
            return TurnAction.PlayDomino;
        }

        // pindah giliran pemain berikutnya
        AdvanceTurn();

        // return action yang sesuai
        return TurnAction.PlayDomino;
    }

    public void DrawCard(IPlayer player)
    {
        // narik sampai bisa main atau pile habis
        DrawUntilPlayable(player);

        // check hasil narik, apakah sekarang bisa narik?
        if (HasPlayableDomino(player))
        {
            // pemain dapat domino yang bisa dimainkan
            return;
        }

        // tetap ngga bisa main (pile habis) -> SKIP
        _consecutiveSkips++;

        // check apakah skip bikin game BLOCK
        if (IsGameBlocked())
        {
            // semua skip beruntun, Game Berakhir
            EndGame();
            return;
        }

        // belum BLOCKED, pindah giliran pemain (current player terlewatkan)
        AdvanceTurn();
    }

    public void AdvanceTurn()
    {
        // pindah pemain berikutnya
        _currentPlayerIndex = (_currentPlayerIndex + 1) % _players.Count;

        // notifikasi UI giliran berubah
        TurnChanged?.Invoke(this, EventArgs.Empty);
    }

    // Get Info Game and Turn
    public GameStatus GetStatus()
    {
        return _gameStatus;
    }

    public IPlayer GetCurrentPlayer()
    {
        return _players[_currentPlayerIndex];
    }

    // Get Info Player and Hand
    public List<IPlayer> GetPlayers()
    {
        return _players;
    }

    public List<IDomino> GetPlayerHands(IPlayer player)
    {
        return _hands[player];
    }

    public int GetRemainingPips(IPlayer player)
    {
        // return kalukasi total pips
        return CalculateRemainingPips(player);
    }

    public bool CanPlayerPlay(IPlayer player)
    {
        // return play able domino
        return HasPlayableDomino(player);
    }

    // Get Info Board
    public IBoard GetBoard()
    {
        return _board;
    }

    public List<int> GetOpenEndPips()
    {
        // jika papan kosong, kemablikan kosong
        if (_board.BoardDominoes.Count == 0)
        {
            List<int> emptyPips = new List<int>();

            return emptyPips;
        }

        // buat list endsPips
        List<int> openEndPips = new List<int>();

        // tambahkan left dan right pips end
        openEndPips.Add(_board.LeftOpenEnd);
        openEndPips.Add(_board.RightOpenEnd);

        // return ends pips
        return openEndPips;
    }

    public bool CanPlaceDomino(IDomino domino, PlacementSide side)
    {
        return ValidatePlacement(domino, side);
    }

    public int GetDrawPileCount()
    {
        int drawPileCount = _drawPile.Dominoes.Count;

        return drawPileCount;
    }

    // Business Logic Setup (Private)
    private void ShuffleDrawPile()
    {
        List<IDomino> dominoes = _drawPile.Dominoes;
        Random random = new Random();

        for (int i = dominoes.Count - 1; i > 0; i--)
        {
            int randomIndex = random.Next(i + 1);
            (dominoes[i], dominoes[randomIndex]) = (dominoes[randomIndex], dominoes[i]);
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
                if (domino != null)
                {
                    AddDominoToHand(player, domino);
                }
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
            if (playerDouble == null)
            {
                continue;
            }

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
        // langsung kembalikan true jika papan dari awal kosong
        if (_board.BoardDominoes.Count == 0)
        {
            return true;
        }

        // variable penampung untuk side yang relavan
        int openEnd = side == PlacementSide.Left ? _board.LeftOpenEnd : _board.RightOpenEnd;

        // matching domino left pips atau right pips
        bool isMatchingLeftAndRightOpenEnd = domino.LeftPips == openEnd || domino.RightPips == openEnd;

        // return
        return isMatchingLeftAndRightOpenEnd;
    }

    private void PlaceDomino(
        IDomino domino,
        PlacementSide side,
        PlacementOrientation orientation)
    {
        // jika papan kosong, langsung saja
        if (_board.BoardDominoes.Count == 0)
        {
            _board.BoardDominoes.Add(domino);
            _board.LeftOpenEnd = domino.LeftPips;
            _board.RightOpenEnd = domino.RightPips;

            return;
        }

        // pasang kiri
        if (side == PlacementSide.Left)
        {
            int openEnd = _board.LeftOpenEnd;

            // tentukan ujung baru (sisi yang TIDAK menempel)
            int newEnd;
            if (domino.RightPips == openEnd)
            {
                // sisi kanan menempel, kiri jadi ujung baru
                newEnd = domino.LeftPips;
            }
            else
            {
                // sisi kiri menempel, kanan jadi ujung baru
                newEnd = domino.RightPips;
            }

            _board.LeftOpenEnd = newEnd;
            // taruh di awal (kiri)
            _board.BoardDominoes.Insert(0, domino);
        }
        // selain itu, pasang kanan
        else
        {
            int openEnd = _board.RightOpenEnd;
            int newEnd;

            if (domino.LeftPips == openEnd)
            {
                // sisi kiri menempel, kanan jadi ujung baru
                newEnd = domino.RightPips;
            }
            else
            {
                // sisi kanan menempel, kiri jadi ujung baru
                newEnd = domino.LeftPips;
            }

            _board.RightOpenEnd = newEnd;
            // taruh di akhir (kanan)
            _board.BoardDominoes.Add(domino);
        }
    }

    private bool HasPlayableDomino(IPlayer player)
    {
        // check apakah ada kartu domino yang bisa ditaruh
        foreach (IDomino domino in _hands[player])
        {
            if (ValidatePlacement(domino, PlacementSide.Left) || ValidatePlacement(domino, PlacementSide.Right))
            {
                return true;
            }
        }

        return false;
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
        // add kartu domino player ke hand
        List<IDomino> hand = _hands[player];
        hand.Add(domino);
    }

    private void RemoveDominoFromHand(IPlayer player, IDomino domino)
    {
        // remove kartu domino player dari hand
        List<IDomino> hand = _hands[player];
        hand.Remove(domino);
    }

    private void DrawUntilPlayable(IPlayer player)
    {
        // loop pemain belum punya domino yang bisa dimainkan dan pile masih ada isi
        while (!HasPlayableDomino(player) && !IsDrawPileEmpty())
        {
            // tarik 1 domino, masukan ke tangan
            IDomino? domino = DrawFromPile();

            if (domino != null)
            {
                AddDominoToHand(player, domino);
            }
        }
    }

    // Business Logic Game End and Resolution (Private)
    private bool CheckGameOver()
    {
        // loop tiap pemain
        foreach (IPlayer player in _players)
        {
            // jika tangan pemain kosong, makan return true (selesai)
            if (_hands[player].Count == 0)
            {
                return true;
            }
        }

        return false;
    }

    private bool IsGameBlocked()
    {
        // define variable blocked jika skip lebih dari jumlah pemain
        bool isBlocked = _consecutiveSkips >= _players.Count;

        // return blocked
        return isBlocked;
    }

    private bool IsDrawPileEmpty()
    {
        bool isDrawPileEmpty = _drawPile.Dominoes.Count == 0;

        return isDrawPileEmpty;
    }

    private int CalculateRemainingPips(IPlayer player)
    {
        // variable penampung total pips
        int totalPips = 0;

        // loop domino kartu ditangan tiap pemain
        foreach (IDomino domino in _hands[player])
        {
            // untuk tiap domino, tambahkan total dari kedua sisi
            totalPips += domino.LeftPips + domino.RightPips;
        }

        // return total pips
        return totalPips;
    }

    private IPlayer DetermineWinner()
    {
        // jika kartu tangan pemain habis, langsung return pemain untuk menang
        foreach (IPlayer player in _players)
        {
            if (_hands[player].Count == 0)
            {
                // set win kartu habis
                _winCondition = WinCondition.EmptyHand;

                return player;
            }
        }

        // kartu tangan tidak ada yang habis, adu pips paling sedikit
        _winCondition = WinCondition.FewestPips;
        // anggap pemain pertama untuk di kalkulasi
        IPlayer winner = _players[0];
        // pips nya
        int lowestPips = CalculateRemainingPips(_players[0]);

        foreach (IPlayer player in _players)
        {
            int pips = CalculateRemainingPips(player);
            // jika lebih sedikit
            if (pips < lowestPips)
            {
                // update winner pemain dan pips terendah
                winner = player;
                lowestPips = pips;
            }
        }

        // return winner
        return winner;
    }

    private void EndGame()
    {
        // set game status
        _gameStatus = GameStatus.Finished;

        // tentulam pemenang
        _winner = DetermineWinner();

        // beritahu UI Game berakhir
        GameEnded?.Invoke(this, EventArgs.Empty);
    }

}