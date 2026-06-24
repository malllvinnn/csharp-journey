using DominoGame.Entities;
using DominoGame.Interfaces;
using DominoGame.UI;

ConsoleUI consoleUI = new ConsoleUI();

// dummy board
List<IDomino> dummyBoard = new List<IDomino>
{
    new Domino(2, 2),
    new Domino(2, 5),
    new Domino(5, 6),
};

// dummy data
var dummyHand = new List<IDomino>
{
    new Domino(6, 2),
    new Domino(3, 5),
    new Domino(3, 3),
};

consoleUI.ShowBoard(dummyBoard,2, 6);
consoleUI.ShowPlayerHand("John Doe", dummyHand);
consoleUI.ShowInfo("John Doe", 45);
consoleUI.ShowMenu();