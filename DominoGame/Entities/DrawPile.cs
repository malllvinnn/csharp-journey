using DominoGame.Interfaces;

namespace DominoGame.Entities;

public class DrawPile : IDrawPile
{
    public List<IDomino> Dominoes { get; }

    public DrawPile(List<IDomino> dominoes)
    {
        Dominoes = dominoes;
    }
}