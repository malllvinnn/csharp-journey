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
            int before = _game.GetPlayerHands(current).Count;
            
            Console.WriteLine($"\n\u26a0 {current.Name} tidak punya kartu yang bisa dimainkan..");
            Console.WriteLine("Menarik kartu dari pile...");
            
            _game.DrawCard(current);
            
            int afterCount = _game.GetPlayerHands(current).Count;
            int drawPileCount = afterCount - before;

            if (_game.CanPlayerPlay(current))
            {
                Console.WriteLine($"Menarik {drawPileCount} kartu. Sekarang ada kartu yang bisa dimainkan!");
            }
            else
            {
                Console.WriteLine("Pile habis dan tetap tidak bisa main. Giliran dilewati.");
            }
            
            PauseBeforeNext();
            // layar privasi kalau giliran pindah
            ShowTransitionIfPlayerChange(current);
            return;
        }

        // bisa main -> langsung pilih domino
        Console.WriteLine("\n============= GAME ACTION ==============");
        Console.WriteLine("Mainkan domino kamu:");
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
           Padding = new Padding(4, 3),
       };

       panel.Expand = true;
       AnsiConsole.Write(panel);
    }

    private string CreateBoardContent()
    {
        IBoard board = _game.GetBoard();
        List<IDomino> dominoes = board.BoardDominoes;
        string result;

        if (dominoes.Count == 0)
        {
            result = "[grey](Table Empty)[/]\n\n" +
                     $"Left Open: [cyan]{board.LeftOpenEnd}[/]    |    Right Open: [cyan]{board.RightOpenEnd}[/]";
            
            return result;
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
        result = $"[white]{cards}[/]\n\n" +
                 $"Left Open: [cyan]{board.LeftOpenEnd}[/]  |  Right Open: [cyan]{board.RightOpenEnd}[/]";

        return result;
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
        int playerCount;
        while (true)
        {
            Console.Write("Jumlah player (2-8): ");
            string? playerInput = Console.ReadLine();
        
            if (int.TryParse(playerInput, out playerCount) && playerCount >= 2 && playerCount <= 8)
            {
                // valid -> keluar loop
                break;
            }
            
            Console.WriteLine("Jumlah player harus antara 2-8");
        }
        
        List<string> names = new List<string>();
        
        for (int i = 0; i < playerCount; i++)
        {
            Console.Write($"Nama Pemain {i + 1}: ");
            string? name = Console.ReadLine();
            names.Add(name ?? $"Player {i + 1}");
        }

        return names;
    }
    
    // main domino (minta nomor + sisi, panggil PlayTurn)
    private void HandlePlayDomino(IPlayer player)
    {
        List<IDomino> hand = _game.GetPlayerHands(player);

        // minta nomor domino
        Console.Write("Pilih nomor domino: ");
        string? numInput = Console.ReadLine();
        bool convertedNum = int.TryParse(numInput, out int number);

        if (!convertedNum || number < 1 || number > hand.Count)
        {
            Console.WriteLine("Nomor tidak valid.");
            return;
        }

        IDomino domino = hand[number - 1];   // -1 karena user mulai dari 1, index mulai 0

        // minta sisi
        Console.Write("Taruh di sisi (L/R): ");
        string? sideInput = Console.ReadLine();

        PlacementSide side;
        if (sideInput?.ToUpper() == "L")
        {
            side = PlacementSide.Left;
        }
        else if (sideInput?.ToUpper() == "R")
        {
            side = PlacementSide.Right;
        }
        else
        {
            Console.WriteLine("Sisi tidak valid (ketik L atau R).");
            return;
        }

        // validasi penempatan sebelum PlayTurn
        if (!_game.CanPlaceDomino(domino, side))
        {
            Console.WriteLine("Domino tidak cocok dengan side. Coba lagi.");
            return;
        }
        
        _game.PlayTurn(player, domino, side, PlacementOrientation.Horizontal);
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
        Console.Clear();
        Console.WriteLine("========================================");
        Console.WriteLine($"   Giliran: {nextPlayer.Name}");
        Console.WriteLine("========================================");
        Console.WriteLine("\nPemain lain harap tidak melihat layar.");
        Console.WriteLine("Tekan Enter saat sudah siap...");
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