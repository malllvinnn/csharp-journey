// See https://aka.ms/new-console-template for more information

using DominoGame;
using DominoGame.Entities;
using DominoGame.Interfaces;

// test sementara buat domino
var dominoes = new List<IDomino>();
for (int left = 0; left <= 12; left++)
{
    for (int right = left; right <= 12; right++)
    {
        dominoes.Add(new Domino(left, right));
    }
}

// test sementara buat draw pile
var drawPile = new DrawPile(dominoes);

// test sementara buat players
var players = new List<IPlayer>
{
    new Player("John Doe"),   
    new Player("Jane Doe"),   
};

// test sementara game logic
var game = new GameLogic(players, drawPile);

// test sementara events
game.GameEnded += (sender, e) => Console.WriteLine("Game Selesai!!");

game.StartGame();

foreach (IPlayer player in game.GetPlayers())
{
    Console.WriteLine(player.Name);
}
