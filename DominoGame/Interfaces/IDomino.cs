namespace DominoGame.Interfaces;

public interface IDomino
{
    public int LeftPips { get; }
    public int RightPips { get; }
    public bool IsDouble => LeftPips == RightPips;
}