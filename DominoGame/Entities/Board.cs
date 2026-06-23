using DominoGame.Interfaces;

namespace DominoGame.Entities;

public class Board : IBoard
{
    public List<IDomino> BoardDominoes { get; }
    public int LeftOpenEnd { get; set; }
    public int RightOpenEnd { get; set; }

    public Board(List<IDomino> boardDominoes)
    {
        BoardDominoes = boardDominoes;
    }
}