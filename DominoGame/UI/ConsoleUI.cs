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

        foreach (IDomino d in board.BoardDominoes)
        {
            Console.Write($"[{d.LeftPips}|{d.RightPips}]");
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

    public void ShowMenu()
    {
        Console.WriteLine("============= GAME ACTION ==============");
        Console.WriteLine("(1) Main domino");
        Console.WriteLine("(2) Ambil kartu");
        Console.Write("Pilih: ");
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
            Console.WriteLine($"\n{current.Name} tidak bisa main, menarik kartu...");
            _game.DrawCard(current);
            return;
        }

        // bisa main → tampilkan menu
        ShowMenu();
        string? choice = Console.ReadLine();

        if (choice == "1")
        {
            HandlePlayDomino(current);
        }
        else if (choice == "2")
        {
            _game.DrawCard(current);
        }
        else
        {
            Console.WriteLine("Pilihan tidak valid.");
        }
    }
    
    // main domino (minta nomor + sisi, panggil PlayTurn)
    private void HandlePlayDomino(IPlayer player)
    {
        List<IDomino> hand = _game.GetPlayerHands(player);

        // minta nomor domino
        Console.Write("Pilih nomor domino: ");
        string? numInput = Console.ReadLine();

        if (!int.TryParse(numInput, out int number) || number < 1 || number > hand.Count)
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

}