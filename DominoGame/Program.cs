using DominoGame;
using DominoGame.Entities;
using DominoGame.Interfaces;
using DominoGame.UI;

// minta nama players
List<string> names = ConsoleUI.AskPlayerNames();

// bikin players
List<IPlayer> players = new List<IPlayer>();
foreach (string name in names)
{
    players.Add(new Player(name));
}

// bikin semua domino (91 untuk Double-12)
List<IDomino> dominoes = new List<IDomino>();
for (int left = 0; left <= 12; left++)
{
    for (int right = left; right <= 12; right++)
    {
        dominoes.Add(new Domino(left, right));
    }
}

// bikin drawPile dari domino
DrawPile drawPile = new DrawPile(dominoes);

GameLogic game = new GameLogic(players, drawPile);

game.StartGame();

ConsoleUI ui = new ConsoleUI(game);

ui.Run();