namespace DominoGame.Interfaces;

public interface IBoard
{
    public List<IDomino> BoardDominoes { get; }
    public int LeftOpenEnd { get; }
    public int RightOpenEnd { get; }
}