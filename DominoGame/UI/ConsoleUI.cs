using DominoGame.Enums;
using DominoGame.Interfaces;

namespace DominoGame.UI;

public class ConsoleUI
{
    private GameLogic _game;

    public ConsoleUI(GameLogic game)
    {
        _game = game;
    }
    
    // loop utama
    public void Run()
    {
        while (_game.GetStatus() == GameStatus.InProgress)
        {
            PlayOneTurn();
        }
        
        ShowResult();
    }
    
    // tampilan
    public void ShowBoard()
    {
        Console.WriteLine("========== BOARD ==========");
        IBoard board = _game.GetBoard();
        List<IDomino> dominoes = board.BoardDominoes;

        if (dominoes.Count == 0)
        {
            Console.WriteLine("(papan kosong)");
            Console.WriteLine($"Left Open: {board.LeftOpenEnd} | Right Open: {board.RightOpenEnd}");
            return;
        }

        int connectingPip = board.LeftOpenEnd;
        
        foreach (IDomino domino in dominoes)
        {
            if (domino.LeftPips == connectingPip)
            {
                Console.Write($"[{domino.LeftPips}|{domino.RightPips}]");
                connectingPip = domino.RightPips;
            }
            else
            {
                Console.Write($"[{domino.RightPips}|{domino.LeftPips}]");
                connectingPip = domino.LeftPips;
            }
        }
        Console.WriteLine();
        Console.WriteLine($"Left Open: {board.LeftOpenEnd} | Right Open: {board.RightOpenEnd}");
    }

    public void ShowPlayerHand(IPlayer player)
    {
        Console.WriteLine("============= HAND DOMINOES ==============");
        Console.WriteLine($"Kartu {player.Name}:");

        List<IDomino> hand = _game.GetPlayerHands(player);
        for (int i = 0; i < hand.Count; i++)
        {
            Console.WriteLine($"  {i + 1}. [{hand[i].LeftPips}|{hand[i].RightPips}]");
        }
    }

    public void ShowInfo()
    {
        Console.WriteLine("============= GAME INFO ==============");
        IPlayer current = _game.GetCurrentPlayer();
        Console.WriteLine($"Giliran: {current.Name}   Sisa pile: {_game.GetDrawPileCount()}");
    }
    
    // input player
    public static List<string> AskPlayerNames()
    {
        int playerCount;
        while (true)
        {
            Console.Write("Jumlah player (2-8): ");
            string? playerInput = Console.ReadLine();
        
            if (int.TryParse(playerInput, out playerCount) && playerCount >= 2 && playerCount <= 8)
            {
                // valid -> keluar loop
                break;
            }
            
            Console.WriteLine("Jumlah player harus antara 2 - 8");
        }
        
        List<string> names = new List<string>();
        
        for (int i = 0; i < playerCount; i++)
        {
            Console.Write($"Nama Pemain {i + 1}: ");
            string? name = Console.ReadLine();
            names.Add(name ?? $"Player {i + 1}");
        }

        return names;
    }
    
    // satu giliran
    private void PlayOneTurn()
    {
        Console.Clear();
        
        IPlayer current = _game.GetCurrentPlayer();

        Console.WriteLine();
        ShowBoard();
        ShowPlayerHand(current);
        ShowInfo();
        
        // tidak bisa main → narik otomatis
        if (!_game.CanPlayerPlay(current))
        {
            int before = _game.GetPlayerHands(current).Count;
            
            Console.WriteLine($"\n\u26a0 {current.Name} tidak punya kartu yang bisa dimainkan..");
            Console.WriteLine("Menarik kartu dari pile...");
            
            _game.DrawCard(current);
            
            int afterCount = _game.GetPlayerHands(current).Count;
            int drawPileCount = afterCount - before;

            if (_game.CanPlayerPlay(current))
            {
                Console.WriteLine($"Menarik {drawPileCount} kartu. Sekarang ada kartu yang bisa dimainkan!");
            }
            else
            {
                Console.WriteLine("Pile habis dan tetap tidak bisa main. Giliran dilewati.");
            }
            
            PauseBeforeNext();
            // layar privasi kalau giliran pindah
            ShowTransitionIfPlayerChange(current);
            return;
        }

        // bisa main -> langsung pilih domino
        Console.WriteLine("\n============= GAME ACTION ==============");
        Console.WriteLine("Mainkan domino kamu:");
        HandlePlayDomino(current);
        
        PauseBeforeNext();
        ShowTransitionIfPlayerChange(current);
    }
    
    // main domino (minta nomor + sisi, panggil PlayTurn)
    private void HandlePlayDomino(IPlayer player)
    {
        List<IDomino> hand = _game.GetPlayerHands(player);

        // minta nomor domino
        Console.Write("Pilih nomor domino: ");
        string? numInput = Console.ReadLine();
        bool convertedNum = int.TryParse(numInput, out int number);

        if (!convertedNum || number < 1 || number > hand.Count)
        {
            Console.WriteLine("Nomor tidak valid.");
            return;
        }

        IDomino domino = hand[number - 1];   // -1 karena user mulai dari 1, index mulai 0

        // minta sisi
        Console.Write("Taruh di sisi (L/R): ");
        string? sideInput = Console.ReadLine();

        PlacementSide side;
        if (sideInput?.ToUpper() == "L")
        {
            side = PlacementSide.Left;
        }
        else if (sideInput?.ToUpper() == "R")
        {
            side = PlacementSide.Right;
        }
        else
        {
            Console.WriteLine("Sisi tidak valid (ketik L atau R).");
            return;
        }

        // panggil PlayTurn, tangkap kalau gagal
        try
        {
            _game.PlayTurn(player, domino, side, PlacementOrientation.Horizontal);
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Gagal: {ex.Message}");
        }
    }

    // hasil akhir
    private void ShowResult()
    {
        Console.WriteLine("\n========== GAME OVER ==========");

        IPlayer? winner = _game.GetWinner();
        WinCondition condition = _game.GetWinConditions();

        if (winner != null)
        {
            if (condition == WinCondition.EmptyHand)
            {
                Console.WriteLine($"{winner.Name} menang — semua kartu habis!");
            }
            else
            {
                Console.WriteLine($"Game blocked! {winner.Name} menang dengan pip paling sedikit.");
            }
        }
    }
    
    private void PauseBeforeNext()
    {
        Console.WriteLine("\nTekan Enter untuk lanjut...");
        Console.ReadLine();
    }

    private void ShowTransitionIfPlayerChange(IPlayer previous)
    {
        // jika game sudah selesai, tidak perlu transisi
        if (_game.GetStatus() != GameStatus.InProgress) return;
        
        IPlayer nextPlayer = _game.GetCurrentPlayer();
        
        // jika pemain sama (misal habis narik lalu bisa main lagi), tidak perlu transisi
        if (nextPlayer == previous) return;
        
        // pemain berbeda -> tampilkan layar bersiap
        Console.Clear();
        Console.WriteLine("========================================");
        Console.WriteLine($"   Giliran: {nextPlayer.Name}");
        Console.WriteLine("========================================");
        Console.WriteLine("\nPemain lain harap tidak melihat layar.");
        Console.WriteLine("Tekan Enter saat sudah siap...");
        Console.ReadLine();
    }
    
    

}