using DominoGame.Interfaces;

namespace DominoGame.UI;

public class ConsoleUI
{
    public void ShowBoard(List<IDomino> boardDominoes, int leftEnd, int rightEnd)
    {
        Console.WriteLine("========== BOARD ==========");
        // cetak domino di papan berjejer
        foreach (IDomino d in boardDominoes)
        {
            Console.Write($"[{d.LeftPips}|{d.RightPips}]");
        }
        Console.WriteLine();  
        Console.WriteLine($"Left Open: {leftEnd} | Right Open: {rightEnd}");
    }

    public void ShowPlayerHand(string playerName, List<IDomino> hand)
    {
        Console.WriteLine("============= HAND DOMINOES ==============");
        Console.WriteLine($"Kartu {playerName}:");
        for (int i = 0; i < hand.Count; i++)
        {
            Console.WriteLine($"  {i + 1}. [{hand[i].LeftPips}|{hand[i].RightPips}]");
        }
    }

    public void ShowInfo(string currentPlayer, int pileCount)
    {
        Console.WriteLine("============= GAME INFO ==============");
        Console.WriteLine($"Giliran: {currentPlayer}   Sisa pile: {pileCount}");
    }

    public void ShowMenu()
    {
        Console.WriteLine("(1) Main domino");
        Console.WriteLine("(2) Ambil kartu");
        Console.WriteLine("(3) Lewati");
        Console.WriteLine("============= GAME ACTION ==============");
        Console.Write("Pilih: ");
    }
}