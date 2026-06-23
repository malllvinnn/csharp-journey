using DominoGame.Interfaces;

namespace DominoGame.Entities;

public class Player : IPlayer
{
    public string Name { get; }

    public Player(string name)
    {
        Name = name;
    }
}