using DominoGame.Interfaces;

namespace DominoGame.Entities;

public class Domino : IDomino
{
    public int LeftPips { get; }
    public int RightPips { get; }
    public bool IsDouble => LeftPips == RightPips;

    public Domino(int leftPips, int rightPips)
    {
        LeftPips = leftPips;
        RightPips = rightPips;
    }
}