using DominoGame.Enums;
using DominoGame.Interfaces;
using Spectre.Console;

namespace DominoGame.UI;

public class ConsoleUi
{
    private GameLogic _game;
    private bool _turnChanged = false;

    public ConsoleUi(GameLogic game)
    {
        _game = game;

        _game.TurnChanged += OnTurnChanged;
        _game.GameEnded += OnGameEnded;
    }

    // loop run utama
    public void Run()
    {
        while (_game.GetStatus() == GameStatus.InProgress)
        {
            PlayOneTurn();
        }
    }

    // satu giliran
    private void PlayOneTurn()
    {
        Console.Clear();
        // reset flag event di setiap awal giliran
        _turnChanged = false;

        IPlayer current = _game.GetCurrentPlayer();

        ShowBoard();
        ShowInfoPanel(current);

        // tidak bisa main → narik otomatis
        if (!_game.CanPlayerPlay(current))
        {
            int handCountBefore = _game.GetPlayerHands(current).Count;

            AnsiConsole.MarkupLine($"\n[yellow]⚠ {current.Name} tidak punya kartu yang bisa dimainkan.[/]");
            AnsiConsole.MarkupLine("[grey]Menarik kartu dari pile...[/]");

            _game.DrawCard(current);

            int handCountAfter = _game.GetPlayerHands(current).Count;
            // jumlah kartu yang ditarik = after - before
            int drawnCount = handCountAfter - handCountBefore;

            if (_game.CanPlayerPlay(current))
            {
                AnsiConsole.MarkupLine($"[green]Menarik {drawnCount} kartu. Sekarang ada kartu yang bisa dimainkan![/]");
            }
            else
            {
                AnsiConsole.MarkupLine("[red]Pile habis dan tetap tidak bisa main. Giliran dilewati.[/]");
            }

            PauseBeforeNext();
            // layar privasi kalau giliran pindah
            ShowTransitionIfPlayerChange(current);
            return;
        }

        // bisa main -> langsung pilih domino
        AnsiConsole.Write(new Rule("[yellow]GAME ACTION[/]").Centered());
        AnsiConsole.MarkupLine("[cyan]Mainkan domino kamu.[/]");
        HandlePlayDomino(current);

        PauseBeforeNext();
        ShowTransitionIfPlayerChange(current);
    }

    // SHOW BOARD
    public void ShowBoard()
    {
        string boardContent = CreateBoardContent();

        var panel = new Panel(boardContent)
        {
            Header = new PanelHeader("[yellow bold] BOARD [/]", Justify.Center),
            Border = BoxBorder.Double,
            Padding = new Padding(4, 5, 4, 3),
        };

        panel.Expand = true;
        AnsiConsole.Write(panel);
    }

    private string CreateBoardContent()
    {
        IBoard board = _game.GetBoard();
        List<IDomino> dominoes = board.BoardDominoes;
        string boardText;

        if (dominoes.Count == 0)
        {
            boardText = "[grey](Table Empty)[/]\n\n" +
                     $"Left Open: [cyan]{board.LeftOpenEnd}[/]    |    Right Open: [cyan]{board.RightOpenEnd}[/]";

            return boardText;
        }

        // susun kartu
        int connectingPip = board.LeftOpenEnd;
        string cards = "";

        foreach (IDomino domino in dominoes)
        {
            int leftShow, rightShow;

            if (domino.LeftPips == connectingPip)
            {
                leftShow = domino.LeftPips;
                rightShow = domino.RightPips;
                connectingPip = domino.RightPips;
            }
            else
            {
                leftShow = domino.RightPips;
                rightShow = domino.LeftPips;
                connectingPip = domino.LeftPips;
            }

            string separator = domino.IsDouble ? "^" : "|";
            cards += $"[[{leftShow}{separator}{rightShow}]]";
        }

        // untuk menggabungkan kartu + info ujung di bawahnya
        boardText = $"[white]{cards}[/]\n\n" +
                 $"Left Open: [cyan]{board.LeftOpenEnd}[/]  |  Right Open: [cyan]{board.RightOpenEnd}[/]";

        return boardText;
    }

    // SHOW GAME INFO
    public void ShowInfoPanel(IPlayer activePlayer)
    {
        // create dua panel kanan kiri
        var playersPanel = CreatePlayersPanel(activePlayer);
        var handPanel = CreateHandPanel(activePlayer);
        var drawPilePanel = CreateDrawPilePanel();

        var playersDrawPileColumn = new Rows(playersPanel, drawPilePanel);

        // taruh bersebelahan
        var columns = new Columns(playersDrawPileColumn, handPanel);
        columns.Collapse();

        var wrapper = new Panel(columns)
        {
            Header = new PanelHeader("[yellow bold] GAME INFO [/]", Justify.Center),
            Border = BoxBorder.Double,
            Padding = new Padding(2, 1, 2, 1)
        };

        wrapper.Expand = true;
        AnsiConsole.Write(wrapper);
    }

    // panel kiri list semua pemain
    private Panel CreatePlayersPanel(IPlayer activePlayer)
    {
        var table = new Table();
        table.Border = TableBorder.None;
        table.AddColumn("Player's turn:");

        foreach (IPlayer player in _game.GetPlayers())
        {
            int handCount = _game.GetPlayerHands(player).Count;

            if (player == activePlayer)
            {
                table.AddRow($"[green bold]> {player.Name} ({handCount})[/]");
            }
            else
            {
                table.AddRow($"[grey]  {player.Name} ({handCount})[/]");
            }
        }

        Panel panel = new Panel(table)
        {
            Header = new PanelHeader("[blue] PLAYERS [/]", Justify.Left),
            Border = BoxBorder.Square,
            Expand = true
        };

        return panel;
    }

    // panel kanan list card pile pemain aktif
    private Panel CreateHandPanel(IPlayer player)
    {
        var table = new Table();
        table.Border = TableBorder.None;
        table.AddColumn("[yellow]No[/]");
        table.AddColumn("[yellow]Domino[/]");

        List<IDomino> hand = _game.GetPlayerHands(player);
        for (int i = 0; i < hand.Count; i++)
        {
            string separator = hand[i].IsDouble ? "^" : "|";
            string dominoText = $"[[{hand[i].LeftPips}{separator}{hand[i].RightPips}]]";

            // tandai double dengan warna
            if (hand[i].IsDouble)
            {
                table.AddRow($"[cyan]{i + 1}[/]", $"[magenta]{dominoText}[/]");
            }
            else
            {
                table.AddRow($"{i + 1}", dominoText);
            }
        }

        Panel panel = new Panel(table)
        {
            Header = new PanelHeader($"[green] HAND PILE [/]", Justify.Left),
            Border = BoxBorder.Square,
        };

        return panel;
    }

    private Panel CreateDrawPilePanel()
    {
        int pileCount = _game.GetDrawPileCount();

        var content = new Markup($"[cyan]{pileCount}[/] dominoes");

        Panel panel = new Panel(content)
        {
            Header = new PanelHeader("[blue] REMAINING [/]", Justify.Left),
            Border = BoxBorder.Square,
            Expand = true
        };

        return panel;
    }

    // input player
    public static List<string> AskPlayerNames()
    {
        // title game domino
        var figlet = new FigletText("DOMINO GAME")
        {
            Color = Color.Red,
        };
        AnsiConsole.Write(figlet);
        AnsiConsole.WriteLine();
        
        // minta jumlah player dengan validasi input
        int playerCount = AnsiConsole.Prompt(
             new TextPrompt<int>("[green]Jumlah player[/] (2-8):")
            .PromptStyle("cyan")
            .ValidationErrorMessage("[red]Jumlah harus antara 2-8[/]")
            .Validate(count =>
            {
                if (count < 2 || count > 8)
                {
                    return ValidationResult.Error();
                }
                return ValidationResult.Success();
            })
        );

        // minta nama player
        List<string> names = new List<string>();

        for (int i = 0; i < playerCount; i++)
        {
            string name = AnsiConsole.Prompt(
                new TextPrompt<string>($"[green]Nama player {i + 1}[/]:")
                .PromptStyle("cyan")
                .AllowEmpty()
            );

            // jika kosong, kasih nama default
            names.Add(string.IsNullOrWhiteSpace(name) ? $"Anonymous {i + 1}" : name);
        }

        return names;
    }

    // main domino (minta nomor + sisi, panggil PlayTurn)
    private void HandlePlayDomino(IPlayer player)
    {
        List<IDomino> hand = _game.GetPlayerHands(player);
        string separator;

        // pilih nomor domino
        IDomino domino = AnsiConsole.Prompt(
            new SelectionPrompt<IDomino>()
                .Title("[cyan]Pilih domino:[/]")
                .UseConverter((isDomino) =>
                {
                    int index = hand.IndexOf(isDomino) + 1;
                    return $"Nomor {index}";
                })
                .AddChoices(hand)
        );
        
        separator = domino.IsDouble ? "^" : "|";
        string dominoInfo = $"[[{domino.LeftPips}{separator}{domino.RightPips}]]";

        // pilih side
        PlacementSide placementSide = AnsiConsole.Prompt(
            new SelectionPrompt<PlacementSide>()
                .Title($"Domino terpilih: [yellow]{dominoInfo}[/]\n[cyan]Taruh di sisi:[/]")
                .UseConverter((side) => side == PlacementSide.Left ? "Left (L)" : "Right (R)")
                .AddChoices(PlacementSide.Left, PlacementSide.Right)
        );

        // validasi placement
        if (!_game.CanPlaceDomino(domino, placementSide))
        {
            AnsiConsole.MarkupLine("[red]Domino tidak cocok dengan side. Coba lagi.[/]");
            return;
        }

        _game.PlayTurn(player, domino, placementSide, PlacementOrientation.Horizontal);
    }

    // hasil akhir
    private void ShowGameOver()
    {
        IPlayer? winner = _game.GetWinner();
        WinCondition condition = _game.GetWinConditions();

        if (winner == null)
        {
            return;
        }

        string winnerMessage;

        if (condition == WinCondition.EmptyHand)
        {
            winnerMessage = $"[green bold]{winner.Name}[/] menang — semua kartu habis!";
        }
        else
        {
            winnerMessage = $"Game blocked! [green bold]{winner.Name}[/] menang dengan pip paling sedikit.";
        }

        // table sisa pip tiap pemain
        var table = new Table();
        table.Border = TableBorder.Rounded;
        table.AddColumn("[yellow]Pemain[/]");
        table.AddColumn("[yellow]Sisa Pip[/]");
        table.AddColumn("[yellow]Sisa Kartu[/]");

        foreach (IPlayer player in _game.GetPlayers())
        {
            int pips = _game.GetRemainingPips(player);
            int cards = _game.GetPlayerHands(player).Count;

            // penandaan pemenang
            if (player == winner)
            {
                table.AddRow(
                    $"[green bold]{player.Name} (menang)[/]",
                    $"[green]{pips}[/]",
                    $"[green]{cards}[/]"
                );
            }
            else
            {
                table.AddRow(
                    $"[grey]{player.Name}[/]",
                    $"[grey]{pips}[/]",
                    $"[grey]{cards}[/]"
                );
            }
        }

        var content = new Rows(
            new Markup(winnerMessage),
            new Text(""),
            table
        );

        var panel = new Panel(content)
        {
            Header = new PanelHeader("[yellow bold] GAME OVER [/]", Justify.Center),
            Border = BoxBorder.Double,
            Padding = new Padding(4, 2),
        };

        panel.Expand = true;

        Console.Clear();
        AnsiConsole.Write(panel);
    }

    private void PauseBeforeNext()
    {
        Console.WriteLine("\nTekan Enter untuk lanjut...");
        Console.ReadLine();
    }

    private void ShowTransitionIfPlayerChange(IPlayer previous)
    {
        // jika game sudah selesai, tidak perlu transisi
        if (_game.GetStatus() != GameStatus.InProgress)
        {
            return;
        }

        // jika pemain sama (misal habis narik lalu bisa main lagi), tidak perlu transisi
        if (!_turnChanged)
        {
            return;
        }

        IPlayer nextPlayer = _game.GetCurrentPlayer();

        // pemain berbeda -> tampilkan layar bersiap
        var panel = new Panel(
            new Rows(
                new Markup($"[green bold]Giliran: {Markup.Escape(nextPlayer.Name)}[/]"),
                new Text(""),
                new Markup("[grey]Pemain lain harap tidak melihat layar.[/]"),
                new Markup("[cyan]Tekan Enter saat sudah siap...[/]")
            )
        )
        {
            Header = new PanelHeader("[yellow bold] GILIRAN BERIKUTNYA [/]", Justify.Center),
            Border = BoxBorder.Double,
            Padding = new Padding(4, 2),
        };

        panel.Expand = true;
        
        Console.Clear();
        AnsiConsole.Write(panel);
        Console.ReadLine();
    }

    // Event Handler / Delegate
    private void OnTurnChanged(object? sender, EventArgs e)
    {
        _turnChanged = true;
    }

    private void OnGameEnded(object? sender, EventArgs e)
    {
        ShowGameOver();
    }

}