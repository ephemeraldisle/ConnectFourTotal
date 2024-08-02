using System.Text;

namespace ConnectFour
{
    internal abstract partial class Program
    {
        public const int GameMessageRow = 22;
        private const int GameInputRow = 23;
        private const string MenuIndent = "         ";
        private const int MenuInputRow = 15;
        private const int CustomizeInputRow = 20;
        private const int WindowWidth = 80;

        private const int WindowHeight = 25;
        private const int ColorMenuStart = 4;
        private const int ColorMenuIndent = 16;
        private const int CustomizeMenuSymbolRepetitions = 24;

        private static readonly AI _ai = new();
        private static readonly Board _board = new();
        private static int _lastAIColumn = -1;
        private static bool _randomStartPlayer;

        private static readonly string[] SymbolOptions = ["■", "●", "▲", "▼", "♦", "♥", "♠", "♣", "☺"];

        private static readonly Dictionary<int, ConsoleColor> ColorOptions = new()
        {
            { 1, ConsoleColor.Red },
            { 2, ConsoleColor.Green },
            { 3, ConsoleColor.Blue },
            { 4, ConsoleColor.Yellow },
            { 5, ConsoleColor.DarkYellow },
            { 6, ConsoleColor.Cyan },
            { 7, ConsoleColor.White },
            { 8, ConsoleColor.Magenta },
            { 9, ConsoleColor.DarkGray },
        };

        private static void Main()
        {
            Console.SetWindowSize(Math.Max(Console.WindowWidth, WindowWidth), Math.Max(Console.WindowHeight, WindowHeight));
            Console.SetCursorPosition(0, 0);
            Console.Clear();
            Console.OutputEncoding = Encoding.UTF8;

            while (true)
            {
                switch (ShowMainMenu())
                {
                    case 1:
                        PlayGame();
                        break;
                    case 2:
                        ToggleRandom();
                        break;
                    case 3:
                        SetAIDifficulty();
                        break;
                    case 4:
                        CustomizeGame();
                        break;
                    case 5:
                        ConsoleUtil.WriteLineWithColor("\nThank you for playing!", _board.HighlightColor);
                        return;
                }
            }
        }

        private static void ToggleRandom()
        {
            _randomStartPlayer = !_randomStartPlayer;
        }

        private static int ShowMainMenu()
        {
            Console.CursorVisible = true;
            Console.Clear();

            DisplayConnectFourHeader(_board.HighlightColor);
            Console.WriteLine();

            ConsoleUtil.WriteLineWithColors(($"{MenuIndent} 1. ", _board.HighlightColor),
                                            ("Play Game", _board.BoardColor));

            ConsoleUtil.WriteLineWithColors(
                                            ($"{MenuIndent} 2. ", _board.HighlightColor),
                                            ("Player always goes first? (", _board.BoardColor),
                                            ("Currently: ", _board.HighlightColor),
                                            ($"{!_randomStartPlayer}",
                                             _randomStartPlayer ? _board.Player2Color : _board.Player1Color),
                                            (")", _board.BoardColor)
                                           );

            ConsoleUtil.WriteLineWithColors(
                                            ($"{MenuIndent} 3. ", _board.HighlightColor),
                                            ("Set AI Difficulty (", _board.BoardColor),
                                            ("Currently: ", _board.HighlightColor),
                                            ($"{_ai.Difficulty}", _board.Player2Color),
                                            (" / ", _board.BoardColor),
                                            ($"{AI.MaximumDifficulty}", _board.Player2Color),
                                            (")", _board.BoardColor)
                                           );

            ConsoleUtil.WriteLineWithColors(
                                            ($"{MenuIndent} 4. ", _board.HighlightColor),
                                            ("Customize Game", _board.BoardColor)
                                           );

            ConsoleUtil.WriteLineWithColors(
                                            ($"{MenuIndent} 5. ", _board.HighlightColor),
                                            ("Quit\n\n", _board.BoardColor)
                                           );


            while (true)
            {
                string input = ConsoleUtil.GetUserInput(MenuInputRow, "Enter your choice (1-5): ", _board.HighlightColor, _board.Player1Color);
                if (int.TryParse(input, out int choice) && choice is >= 1 and <= 5) return choice;
                Console.SetCursorPosition(0, MenuInputRow - 1);
                ConsoleUtil.WriteLineWithColor("Invalid choice. Please try again.", _board.HighlightColor);
            }
        }


        private static void CustomizeGame()
        {
            bool playerMistake = false;
            while (true)
            {
                DisplayCustomizeGameMenu();
                if (playerMistake)
                {
                    Console.SetCursorPosition(0, CustomizeInputRow - 1);
                    ConsoleUtil.WriteLineWithColor("Invalid choice. Please try again.", _board.HighlightColor);

                }
                string input = ConsoleUtil.GetUserInput(CustomizeInputRow, "Enter your choice (1-7): ", _board.HighlightColor, _board.Player1Color);
                if (int.TryParse(input, out int choice))
                {
                    playerMistake = false;
                    switch (choice)
                    {
                        case 1:
                            _board.BoardColor = ChooseColor("Choose the board color:", _board.BoardColor);
                            break;
                        case 2:
                            _board.HighlightColor = ChooseColor("Choose the text color:", _board.HighlightColor);
                            break;
                        case 3:
                            _board.Player1Color = ChooseColor("Choose Player 1 color:", _board.Player1Color);
                            break;
                        case 4:
                            _board.Player1Symbol = ChooseSymbol("Choose Player 1 symbol:", _board.Player1Symbol);
                            break;
                        case 5:
                            _board.Player2Color = ChooseColor("Choose Player 2 color:", _board.Player2Color);
                            break;
                        case 6:
                            _board.Player2Symbol = ChooseSymbol("Choose Player 2 symbol:", _board.Player2Symbol);
                            break;
                        case 7:
                            return;
                    }
                }
                else
                {
                    playerMistake = true;
                }
            }
        }

        private static void DisplayCustomizeGameMenu()
        {
            Console.Clear();
            DisplayConnectFourHeader(_board.BoardColor);
            for (int i = 0; i < CustomizeMenuSymbolRepetitions; i++)
            {
                if ((i % 2) == 0)
                    ConsoleUtil.WriteWithColor($"{_board.Player1Symbol} ", _board.Player1Color);
                else
                    ConsoleUtil.WriteWithColor($"{_board.Player2Symbol} ", _board.Player2Color);
            }

            Console.WriteLine("\n");

            ConsoleUtil.WriteLineWithColor("          ~* Customize Game *~\n", _board.HighlightColor);

            ConsoleUtil.WriteLineWithColors(
                                            ($"{MenuIndent} 1. ", _board.HighlightColor),
                                            ("Board Color", _board.BoardColor)
                                           );

            ConsoleUtil.WriteLineWithColor($"{MenuIndent} 2. Text Color", _board.HighlightColor);

            ConsoleUtil.WriteLineWithColors(
                                            ($"{MenuIndent} 3. ", _board.HighlightColor),
                                            ("Player 1 Color", _board.Player1Color)
                                           );

            ConsoleUtil.WriteLineWithColors(
                                            ($"{MenuIndent} 4. ", _board.HighlightColor),
                                            ($"Player 1 Symbol: {_board.Player1Symbol}", _board.Player1Color)
                                           );

            ConsoleUtil.WriteLineWithColors(
                                            ($"{MenuIndent} 5. ", _board.HighlightColor),
                                            ("Player 2 Color", _board.Player2Color)
                                           );

            ConsoleUtil.WriteLineWithColors(
                                            ($"{MenuIndent} 6. ", _board.HighlightColor),
                                            ($"Player 2 Symbol: {_board.Player2Symbol}", _board.Player2Color)
                                           );

            ConsoleUtil.WriteLineWithColors(
                                            ($"{MenuIndent} 7. ", _board.HighlightColor),
                                            ("Back to Main Menu\n\n", _board.BoardColor)
                                           );

        }

        private static void SetAIDifficulty()
        {
            Console.Clear();
            DisplayConnectFourHeader(_board.HighlightColor);

            ConsoleUtil.
                WriteLineWithColor($"\n{MenuIndent}The higher the level, the more threatening the AI is. Examples: \n",
                                   _board.Player2Color);
            ConsoleUtil.
                WriteLineWithColor($"{MenuIndent}Level 1 - \"Popcorn\" - Not a complete pushover, but makes mistakes.",
                                   _board.BoardColor);
            ConsoleUtil.
                WriteLineWithColor($"{MenuIndent}Level 3 - \"Standard\" - A decent player, who knows a thing or two.",
                                   _board.Player2Color);
            ConsoleUtil.
                WriteLineWithColor($"{MenuIndent}Level 5 - \"Brute\" - A serious player who poses a real challenge.",
                                   _board.Player1Color);
            ConsoleUtil.WriteLineWithColor($"{MenuIndent}Level 8 - \"Champion\" - A nearly flawless player.",
                                           _board.HighlightColor);
            ConsoleUtil.WriteLineWithColor($"{MenuIndent}Level 10 - \"Master\" - Good luck.\n",
                                           _board.Player2Color);


            while (true)
            {
                string input = ConsoleUtil.GetUserInput(MenuInputRow+1, $"Enter AI difficulty ({AI.MinimumDifficulty}-{AI.MaximumDifficulty}): ", _board.HighlightColor,
                                                        _board.Player1Color);

                if (int.TryParse(input, out int difficulty) &&
                    difficulty is >= AI.MinimumDifficulty and <= AI.MaximumDifficulty)
                {
                    _ai.Difficulty = difficulty;
                    break;
                }

                {
                    Console.SetCursorPosition(0, MenuInputRow);
                    ConsoleUtil.WriteLineWithColor("Invalid input. Please try again.", _board.HighlightColor);
                }
            }
        }


        private static ConsoleColor ChooseColor(string prompt, ConsoleColor currentColor)
            {
                Console.Clear();
                DisplayConnectFourHeader(currentColor);
                Console.Write(MenuIndent);
                ConsoleUtil.WriteLineWithColor(prompt + "\n", _board.HighlightColor);
                ConsoleUtil.WriteWithColor($"{MenuIndent} Current color: ", _board.BoardColor);
                ConsoleUtil.WriteWithColor($"{currentColor}\n\n", currentColor);

                for (int row = 0; row < 3; row++)
                {
                    for (int column = 0; column < 3; column++)
                    {
                        Console.CursorLeft = ColorMenuStart + (ColorMenuIndent * column);
                        int index = (row * 3) + column + 1;

                        ConsoleUtil.WriteWithColor($"{index}: ", _board.HighlightColor);
                        ConsoleUtil.WriteWithColor($"{ColorOptions[index]}", ColorOptions[index]);
                    }

                    Console.WriteLine("\n");
                }

                while (true)
                {
                    string input = ConsoleUtil.GetUserInput(CustomizeInputRow, "Enter your choice (1-9), or 0 to keep current: ",
                                             _board.HighlightColor, _board.Player1Color);
                    if (int.TryParse(input, out int choice))
                    {
                        if (choice == 0) return currentColor;
                        if (ColorOptions.TryGetValue(choice, out ConsoleColor selectedColor)) return selectedColor;
                    }
                    Console.SetCursorPosition(0, CustomizeInputRow - 1);
                    ConsoleUtil.WriteLineWithColor("Invalid choice. Please try again.", _board.HighlightColor);
                }
            }

            private static string ChooseSymbol(string prompt, string currentSymbol)
            {
                ConsoleColor playerColor = currentSymbol == _board.Player1Symbol ? _board.Player1Color : _board.Player2Color;
                Console.Clear();

                DisplayConnectFourHeader(playerColor);
                Console.Write(MenuIndent);
                ConsoleUtil.WriteLineWithColor(prompt + "\n", _board.HighlightColor);
                ConsoleUtil.WriteWithColor($"{MenuIndent} Current color: ", _board.BoardColor);
                ConsoleUtil.WriteWithColor($"{currentSymbol}\n\n", playerColor);

                for (int row = 0; row < 3; row++)
                {
                    Console.Write(MenuIndent);
                    for (int column = 0; column < 3; column++)
                    {
                        int index = (row * 3) + column;
                        ConsoleUtil.WriteWithColor($"{index + 1}.", _board.BoardColor);
                        ConsoleUtil.WriteWithColor($" {SymbolOptions[index]}    ", playerColor);
                    }

                    Console.WriteLine("\n");
                }

                while (true)
                {
                    string input = ConsoleUtil.GetUserInput(CustomizeInputRow, "Enter your choice (1-9), or 0 to keep current: ",
                                                            _board.HighlightColor, _board.Player1Color);
                    if (int.TryParse(input, out int choice))
                    {
                        switch (choice)
                        {
                            case 0:
                                return currentSymbol;
                            case >= 1 and <= 9:
                                return SymbolOptions[choice - 1];
                        }
                    }

                    Console.SetCursorPosition(0, CustomizeInputRow - 1);
                    ConsoleUtil.WriteLineWithColor("Invalid choice. Please try again.", _board.HighlightColor);
                    
                }
            }

            private static void PlayGame()
            {
                while (true)
                {
                    Console.Clear();
                    _board.ResetBoardState();
                    _lastAIColumn = -1;
                    PrintBoard();

                    bool playerTurn = !_randomStartPlayer || Random.Shared.Next(2) == 0;

                    while (true)
                    {
                        if (playerTurn)
                        {
                            if (PlayerTurn())
                            {
                                DisplayWinningState(Board.PlayerState.Player1);
                                break;
                            }
                        }
                        else
                        {
                            Console.CursorLeft = 0;
                            Console.Write(new string(' ', Console.WindowWidth));
                            Console.SetCursorPosition(0, GameMessageRow);
                            ConsoleUtil.WriteWithColor("Player 2 is thinking...", _board.Player2Color);

                            if (AITurn())
                            {
                                DisplayWinningState(Board.PlayerState.Player2);
                                break;
                            }
                        }

                        if (_board.IsFull())
                        {
                            ConsoleUtil.WriteLineWithColor("\nIt's a draw!", _board.HighlightColor);
                            break;
                        }

                        playerTurn = !playerTurn;
                    }


                    if (!PromptPlayAgain()) break;
                }
            }

            private static bool PromptPlayAgain()
            {
                Console.ForegroundColor = _board.HighlightColor;
                Console.WriteLine($"\n{MenuIndent}Play again?\n");
                Console.Write("      Input \"y\": ");
                ConsoleUtil.WriteWithColor("You know it!\n", _board.Player1Color);
                ConsoleUtil.WriteWithColor("      Input \"n\": ", _board.HighlightColor);
                ConsoleUtil.WriteWithColor("Bring me back to the menu...\n\n", _board.Player1Color);

                int cursorRow = Console.CursorTop;

                while (true)
                {
                    Console.SetCursorPosition(0, cursorRow);
                    Console.Write(new string(' ', Console.WindowWidth));
                    Console.CursorLeft = 0;
                    Console.ForegroundColor = _board.Player1Color;
                    Console.CursorVisible = true;

                    string? input = Console.ReadLine()?.ToLower().Trim();
                    switch (input)
                    {
                        case "y":
                            return true;
                        case "n":
                            return false;
                        default:
                            Console.SetCursorPosition(0, cursorRow - 1);
                            ConsoleUtil.WriteWithColor("Invalid choice. Please try again.", _board.HighlightColor);
                            break;
                    }
                }
            }

            private static bool PlayerTurn()
            {
                Console.SetCursorPosition(0, GameMessageRow);
                Console.Write(new string(' ', Console.WindowWidth));
                while (true)
                {
                    Console.ForegroundColor = _board.Player1Color;

                    Console.SetCursorPosition(0, GameInputRow);
                    Console.Write(new string(' ', Console.WindowWidth));

                    Console.SetCursorPosition(0, GameInputRow);
                    Console.CursorVisible = true;

                    int? column = GetRequestedColumn();

                    if (!column.HasValue)
                    {
                        Console.SetCursorPosition(0, GameMessageRow);
                        Console.Write(new string(' ', Console.WindowWidth));

                        Console.SetCursorPosition(0, GameMessageRow);
                        ConsoleUtil.WriteLineWithColor("Please enter a valid column number (1-7).", _board.HighlightColor);
                        continue;
                    }

                    if (!_board.CanMakeMove(column.Value))
                    {
                        Console.SetCursorPosition(0, GameMessageRow);
                        Console.Write(new string(' ', Console.WindowWidth));

                        Console.SetCursorPosition(0, GameMessageRow);
                        ConsoleUtil.WriteLineWithColor("That column is full. Try another one.", _board.HighlightColor);
                        continue;
                    }

                    Console.SetCursorPosition(0, GameInputRow);
                    Console.Write(new string(' ', Console.WindowWidth));
                    Console.CursorVisible = false;
                    _board.AnimateMoveAndPrint(Board.PlayerState.Player1, column.Value);

                    return _board.CheckVictory(Board.PlayerState.Player1, column.Value);
                }
            }

            private static int? GetRequestedColumn()
            {
                int startRow = Console.CursorTop;
                Console.Write(new string(' ', Console.WindowWidth));
                Console.SetCursorPosition(0, startRow);
                Console.Write("Enter column (1-7): ");
                string? input = Console.ReadLine();
                return int.TryParse(input, out int column) && column is >= 1 and <= 7 ? column - 1 : null;
            }

            private static bool AITurn()
            {
                int column = _ai.GetBestMove(_board, Board.PlayerState.Player2);
                _lastAIColumn = column;
                _board.AnimateMoveAndPrint(Board.PlayerState.Player2, column);

                return _board.CheckVictory(Board.PlayerState.Player2, column);
            }

            private static void DisplayWinningState(Board.PlayerState winner)
            {
                Console.Clear();
                _board.PrintBoard(_lastAIColumn, (int)winner);
                Console.ForegroundColor = winner == Board.PlayerState.Player1 ? _board.Player1Color : _board.Player2Color;
                string winnerName = winner == Board.PlayerState.Player1 ? "Player 1" : "Player 2";
                Console.SetCursorPosition(10, GameMessageRow - 3);
                Console.Write("! ! ! ! ! ! ! ! ! ! !");
                Console.SetCursorPosition(3, GameMessageRow - 2);
                Console.Write($"! ! ! ! A winner is {winnerName}! ! ! !\n");
                Console.WriteLine("    ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! !");
            }

            private static void PrintBoard()
            {
                Console.Clear();
                _board.PrintBoard(_lastAIColumn);
                Console.WriteLine();
            }

            public static void DisplayConnectFourHeader(ConsoleColor color)
            {
                Console.ForegroundColor = color;
                Console.WriteLine("\n            N E C T   F O U R");
                Console.WriteLine("          N E         O O");
                Console.WriteLine("        O   C         U   U");
                Console.WriteLine("      C     T         R     R\n");
            }
    }
}