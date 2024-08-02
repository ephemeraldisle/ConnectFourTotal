using System.Text;

namespace ConnectFour
{
    public class Board
    {
        public ConsoleColor BoardColor = ConsoleColor.Blue;
        public ConsoleColor HighlightColor = ConsoleColor.Green;
        public ConsoleColor Player1Color = ConsoleColor.Red;
        public ConsoleColor Player2Color = ConsoleColor.Yellow;

        public string Player1Symbol = "■";
        public string Player2Symbol = "●";

        private HashSet<(int row, int col)> _winningPositions = [];

        public enum PlayerState { Empty, Player1, Player2 }

        public const int Width = 7;
        public const int Height = 6;

        public const int WinningLength = 4;
        private const int InitialAnimationFrameTime = 150;
        private const double AnimationFrameGravity = 9.8;
        private const int AnimationStartRow = 10;
        private const int AnimationStartColumnOffset = 8;
        private const int GridCellSize = 4;

        public static readonly (int rowDirection, int columnDirection)[] DirectionsToCheck =
        [
            (1, 0),  // Vertical
            (0, 1),  // Horizontal 
            (1, 1),  // Diagonal \
            (1, -1),  // Diagonal /
        ];


        private readonly PlayerState[,] _boardState = new PlayerState[Height, Width];
        private readonly int[] _columnHeights = new int[Width];

        public static bool IsValidPosition(int row, int column) => row is >= 0 and < Height && column is >= 0 and < Width;

        public void ResetBoardState()
        {
            Array.Clear(_boardState, 0, _boardState.Length);
            Array.Clear(_columnHeights, 0, _columnHeights.Length);
            _winningPositions = [];
        }

        public PlayerState GetSpace(int row, int column) =>
            IsValidPosition(row, column) ? _boardState[row, column] : throw new ArgumentOutOfRangeException(nameof(row));


        private void SetSpace(int row, int column, PlayerState newState)
        {
            if (!IsValidPosition(row, column)) throw new ArgumentOutOfRangeException(nameof(row));
            _boardState[row, column] = newState;
        }

        public int GetColumnHeight(int column) =>
            column is >= 0 and < Width ? _columnHeights[column] : throw new ArgumentOutOfRangeException(nameof(column));

        public bool CanMakeMove(int column) => GetColumnHeight(column) < Height;

        public void MakeMove(PlayerState player, int column)
        {
            if (!CanMakeMove(column)) throw new InvalidOperationException("Column is full");

            int row = _columnHeights[column];
            SetSpace(row, column, player);
            _columnHeights[column]++;
        }

        public bool CheckVictory(PlayerState player, int column)
        {
            if (player == PlayerState.Empty) throw new ArgumentException("Invalid player state", nameof(player));

            int row = _columnHeights[column] - 1;
            bool isVictory = false;
            _winningPositions.Clear();

            foreach ((int rowDirection, int columnDirection) in DirectionsToCheck)
            {
                if (CountConsecutive(row, column, rowDirection, columnDirection, player) >= WinningLength)
                    isVictory = true;
            }

            return isVictory;
        }

        private int CountConsecutive(int placedRow, int placedColumn, int rowDirection, int columnDirection, PlayerState player)
        {
            int count = 0;
            int maxCount = 0;
            HashSet<(int, int)> currentStreak = new();

            int rowMaxSteps = rowDirection switch
            {
                1  => placedRow,
                -1 => Height - 1 - placedRow,
                _  => int.MaxValue,
            };

            int columnMaxSteps = columnDirection switch
            {
                1  => placedColumn,
                -1 => Width - 1 - placedColumn,
                _  => int.MaxValue,
            };

            int steps = Math.Min(rowMaxSteps, columnMaxSteps);
            
            int row = placedRow - steps * rowDirection;
            int column = placedColumn - steps * columnDirection;


            while (IsValidPosition(row, column))
            {

                if (_boardState[row, column] == player)
                {
                    count++;
                    currentStreak.Add((row, column));
                    maxCount = Math.Max(count, maxCount);

                        if (maxCount >= WinningLength)
                        {
                            _winningPositions.UnionWith(currentStreak);
                        }
                    
                }
                else
                {
                    if (NotEnoughStepsLeftForVictory(rowDirection, columnDirection, row, column)) break;

                    count = 0;
                    currentStreak.Clear();
                }

                row += rowDirection;
                column += columnDirection;


            }

            return maxCount;
        }

        private static bool NotEnoughStepsLeftForVictory(int rowDirection, int columnDirection, int row, int column)
        {
            int remainingRows = rowDirection == 0 ? int.MaxValue : (rowDirection > 0 ? Height - 1 - row : row);
            int remainingColumns = columnDirection == 0 ? int.MaxValue : (columnDirection > 0 ? Width - 1 - column : column);
            int remainingSpaces = Math.Min(remainingRows, remainingColumns) + 1;

            return remainingSpaces < WinningLength;
        }

        public bool IsFull() => _columnHeights.All(height => height == Height);


        public Board Clone()
        {
            Board newBoard = new();
            Array.Copy(_boardState, newBoard._boardState, _boardState.Length);
            Array.Copy(_columnHeights, newBoard._columnHeights, _columnHeights.Length);
            return newBoard;
        }

        public override string ToString()
        {
            StringBuilder sb = new(Width * Height);
            for (int row = 0; row < Height; row++)
            {
                for (int column = 0; column < Width; column++) sb.Append((int)_boardState[row, column]);
            }

            return sb.ToString();
        }


        public void AnimateMoveAndPrint(PlayerState player, int column)
        {
            int row = GetColumnHeight(column);

            int columnPosition = AnimationStartColumnOffset + (column * GridCellSize);

            string pieceChar = GetSpaceStateCharacter(player);
            ConsoleColor pieceColor = player == PlayerState.Player1 ? Player1Color : Player2Color;


            for (int i = AnimationStartRow; i < ((AnimationStartRow + Height) - row); i++)
            {
                Console.SetCursorPosition(columnPosition, i);
                Console.Write(" ");

                Console.SetCursorPosition(columnPosition, i + 1);
                ConsoleUtil.WriteWithColor(pieceChar, pieceColor);

                double adjustedFrameTime = InitialAnimationFrameTime - (AnimationFrameGravity * i);
                Thread.Sleep(Math.Max(1, (int)adjustedFrameTime));
            }

            //Clear animation artifacts.
            Console.SetCursorPosition(0, AnimationStartRow + 1);
            Console.Write(new string(' ', Console.WindowWidth));

            SetSpace(row, column, player);
            _columnHeights[column]++;

            Console.SetCursorPosition(0, 0);
            PrintBoard(player == PlayerState.Player1 ? -1 : column);
            Console.SetCursorPosition(0, Program.GameMessageRow - 1);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, Program.GameMessageRow);
        }

        public void PrintBoard(int lastAIColumn = -1, int winningPlayer = 0)
        {
            ConsoleColor headerColor = HighlightColor;
            if (winningPlayer != 0) headerColor = winningPlayer == 1 ? Player1Color : Player2Color;
            Program.DisplayConnectFourHeader(headerColor);
            DisplayBoardHeader();
            Console.Write("        ");
            for (int i = 0; i < Width; i++)
                ConsoleUtil.WriteWithColor("↓   ", i != lastAIColumn ? Player1Color : Player2Color);

            Console.WriteLine("\n");

            for (int i = 0; i < Height; i++)
            {
                //Build the outside of the board frame.
                ConsoleUtil.WriteWithColor(i == (Height - 1) ? "   ┌  |" : "      |", BoardColor);

                //Fill out all the grid spaces within the board.
                for (int j = 0; j < Width; j++) FillGridSpace(i, j);

                if (i == (Height - 1)) ConsoleUtil.WriteWithColor("  ┐", BoardColor);

                Console.WriteLine();
            }

            ConsoleUtil.WriteLineWithColor("   └‾‾|‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾|‾‾┘", BoardColor);
            ConsoleUtil.WriteLineWithColor("    _/▲\\_                       _/▲\\_\n", BoardColor);
        }

        private void FillGridSpace(int i, int j)
        {
            Console.Write(' ');
            PlayerState currentState = GetSpace(Height - i - 1, j);
            bool isWinningPosition = _winningPositions.Contains((Height - i - 1, j));
            ConsoleColor color = isWinningPosition ? HighlightColor : GetSpaceStateColor(currentState);
            ConsoleUtil.WriteWithColor(GetSpaceStateCharacter(currentState), color);
            ConsoleUtil.WriteWithColor(" |", BoardColor);
        }

        private void DisplayBoardHeader()
        {
            ConsoleUtil.WriteLineWithColor("   ‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾", BoardColor);
            ConsoleUtil.WriteLineWithColor("      C H O O S E   A   C O L U M N\n", HighlightColor);
            ConsoleUtil.WriteLineWithColor("        1   2   3   4   5   6   7", Player1Color);
        }

        private ConsoleColor GetSpaceStateColor(PlayerState state)
        {
            return state switch
            {
                PlayerState.Empty   => BoardColor,
                PlayerState.Player1 => Player1Color,
                PlayerState.Player2 => Player2Color,
                _                   => BoardColor,
            };
        }


        private string GetSpaceStateCharacter(PlayerState state)
        {
            return state switch
            {
                PlayerState.Empty   => " ",
                PlayerState.Player1 => Player1Symbol,
                PlayerState.Player2 => Player2Symbol,
                _                   => " ",
            };
        }
    }
}