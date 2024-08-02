using System.Diagnostics;
using System.Text;

namespace ConnectFour
{
    public class AI
    {
        public const int MinimumDifficulty = 1;
        public const int MaximumDifficulty = 10;

        //Arbitrary high value:
        public const int WinScore = 1000000;

        //These weights could be tweaked by someone who knows more about Connect 4 strategy to be better.
        public const int PlayerThreatWeight = 4;
        public const int OpponentThreatWeight = 3;
        public const int PlayerCenterWeight = 2;
        public const int OpponentCenterWeight = 1;

        public const int AlmostScoredValue = 100;
        public const int PotentialPairValue = 10;
        public const int MeaningfulMoveValue = 1;

        private int _difficulty = 5;
        public double DifficultyModifier = 0.5;
        private int _errorMaximum = 52;
        public double WeightModifier = 1;

        public int Difficulty
        {
            get => _difficulty;
            set
            {
                _difficulty = value;
                DifficultyModifier = value * 0.1;
                _errorMaximum = 102 - (_difficulty * 10);
                WeightModifier = DifficultyModifier + 0.5;
                _transpositionTable.Clear();
            }
        }

        private static readonly int[] StandardColumnOrder = [3, 2, 4, 1, 5, 0, 6];
        private static readonly int[] AlternateColumnOrder = [3, 4, 2, 5, 1, 6, 0];
        private readonly Dictionary<string, int> _transpositionTable = new();

        private readonly Random _random;

        public AI(Random? random = null) => _random = random ?? Random.Shared;

        public int GetBestMove(Board board, Board.PlayerState player)
        {
            int bestMove = -1;
            int bestScore = int.MinValue;

            StringBuilder debug = new();
            debug.AppendLine("GetBestMove Debug Output:");


            //By varying which order moves are checked, slightly, should lead to more varied play.
            int[] orderToUse = _random.Next(0, 2) == 0 ? StandardColumnOrder : AlternateColumnOrder;

            if (Difficulty <= 2) _random.Shuffle(orderToUse);


            foreach (int column in orderToUse)
            {
                if (!board.CanMakeMove(column)) continue;

                Board newBoard = board.Clone();
                newBoard.MakeMove(player, column);

                if (newBoard.CheckVictory(player, column)) return column;
                int immediateScore = EvaluateBoard(newBoard, player);
                int score = -Negamax(newBoard, Difficulty, int.MinValue, int.MaxValue, GetOpponent(player), 0, true);


                debug.AppendLine($"Column {column}:");
                debug.AppendLine($"  Negamax Score: {score}");
                debug.AppendLine($"  Immediate Evaluation: {immediateScore}");

                if (score <= bestScore) continue;

                bestScore = score;
                bestMove = column;
                debug.AppendLine($"  New best move: {bestMove}");
            }

            debug.AppendLine($"Final best move: {bestMove}");
            Debug.WriteLine(debug.ToString());
            return bestMove;
        }

        public int Negamax(
            Board board, int depth, int alpha, int beta, Board.PlayerState player, int debugIndent = 0,
            bool log = false)
        {
            //if (depth is 9 or 8)
            //{
            //    log = true;
            //}
            string indentation = new(' ', debugIndent * 2);
            if (log) Debug.WriteLine($"{indentation}Negamax: depth={depth}, player={player}");

            string boardString = board.ToString();
            if (_transpositionTable.TryGetValue(boardString, out int cachedScore))
            {
                //Randomly misremembers score analysis, introducing errors at lower difficulty levels.
                int randomOffset = (int)(_random.Next(1, _errorMaximum) * (1 - DifficultyModifier));
                if (log) Debug.WriteLine($"{indentation}Using cached score: {cachedScore} with offset: {randomOffset}");
                return cachedScore + randomOffset;
            }

            if (depth == 0 || board.IsFull())
            {
                int score = EvaluateBoard(board, player);
                if (log) Debug.WriteLine($"{indentation}Leaf node, score: {score}");
                return score;
            }

            int maxScore = int.MinValue;

            //By randomly picking between two equally optimal orders to check in, should introduce additional variety to the play.
            int[] orderToUse = _random.Next(0, 2) == 0 ? StandardColumnOrder : AlternateColumnOrder;

            if (Difficulty <= 2) _random.Shuffle(orderToUse);

            foreach (int column in orderToUse)
            {
                if (!board.CanMakeMove(column)) continue;

                Board newBoard = board.Clone();
                newBoard.MakeMove(player, column);

                if (newBoard.CheckVictory(player, column))
                {
                    if (log) Debug.WriteLine($"{indentation}Winning move found: column {column}");
                    return WinScore - (10000 - (1000 * depth));
                }

                int score = -Negamax(newBoard, depth - 1, -beta, -alpha, GetOpponent(player));
                if (log) Debug.WriteLine($"{indentation}Column {column}, score: {score}");
                maxScore = Math.Max(maxScore, score);
                alpha = Math.Max(alpha, score);

                if (alpha >= beta)
                {
                    if (log) Debug.WriteLine($"{indentation}Beta cutoff");
                    break;
                }
            }

            _transpositionTable[boardString] = maxScore;

            if (log) Debug.WriteLine($"{indentation}Returning maxScore: {maxScore}");
            return maxScore;
        }

        public int EvaluateBoard(Board board, Board.PlayerState player)
        {
            int score = 0;
            int column, row;
            Board.PlayerState opponent = GetOpponent(player);


            // Check all columns
            for (column = 0; column < Board.Width; column++)
            {
                int lineScore = EvaluateLine(board, 0, column, 1, 0, player, opponent);
                if (Math.Abs(lineScore) == WinScore) return lineScore;
                score += lineScore;
            }

            // Check all rows

            for (row = 0; row < Board.Height; row++)
            {
                int lineScore = EvaluateLine(board, row, 0, 0, 1, player, opponent);
                if (Math.Abs(lineScore) == WinScore) return lineScore;
                score += lineScore;
            }

            // Check all / diagonals
            for (column = 0; column <= (Board.Width - Board.WinningLength); column++)
            {
                int lineScore = EvaluateLine(board, 0, column, 1, 1, player, opponent);
                if (Math.Abs(lineScore) == WinScore) return lineScore;
                score += lineScore;
            }

            for (row = 1; row <= (Board.Height - Board.WinningLength); row++)
            {
                int lineScore = EvaluateLine(board, row, 0, 1, 1, player, opponent);
                if (Math.Abs(lineScore) == WinScore) return lineScore;
                score += lineScore;
            }


            //Check all \ diagonals

            for (column = 0; column <= (Board.Width - Board.WinningLength); column++)
            {
                int lineScore = EvaluateLine(board, Board.Height - 1, column, -1, 1, player, opponent);
                if (Math.Abs(lineScore) == WinScore) return lineScore;
                score += lineScore;
            }

            for (row = Board.Height - 2; row >= (Board.WinningLength - 1); row--)
            {
                int lineScore = EvaluateLine(board, row, 0, -1, 1, player, opponent);
                if (Math.Abs(lineScore) == WinScore) return lineScore;
                score += lineScore;
            }

            score += (int)(CountCenterControl(board, player) * (PlayerCenterWeight * WeightModifier));
            score -= (int)(CountCenterControl(board, opponent) * (OpponentCenterWeight * WeightModifier));

            //Randomly fails at score analysis, introducing errors at lower difficulty levels.
            if (Difficulty < MaximumDifficulty)
            {
                int randomOffset = _random.Next(1, _errorMaximum + 1);
                score += randomOffset;
            }

            return score;
        }


        private int EvaluateLine(
            Board board, int startRow, int startColumn, int rowDirection, int columnDirection, Board.PlayerState player,
            Board.PlayerState opponent)
        {
            int bestScore = 0;
            Queue<Board.PlayerState> window = new(Board.WinningLength);

            for (int i = 0; i < Board.Height; i++)
            {
                int row = startRow + (i * rowDirection);
                int column = startColumn + (i * columnDirection);

                if (!Board.IsValidPosition(row, column)) break;

                Board.PlayerState spaceState = board.GetSpace(row, column);
                if (spaceState == Board.PlayerState.Empty && row > board.GetColumnHeight(column))
                    spaceState = Board.PlayerState.Unplayable;

                window.Enqueue(spaceState);


                if (window.Count > Board.WinningLength) window.Dequeue();

                if (window.Count == Board.WinningLength)
                {
                    int score = EvaluateWindow(window, player, opponent);
                    if (Math.Abs(score) > Math.Abs(bestScore)) bestScore = score;
                }
            }

            return bestScore;
        }


        private int EvaluateWindow(
            Queue<Board.PlayerState> window, Board.PlayerState player, Board.PlayerState opponent)
        {
            int playerCount = window.Count(state => state == player);
            if (playerCount == Board.WinningLength) return WinScore;


            int opponentCount = window.Count(state => state == opponent);
            if (opponentCount == Board.WinningLength) return -WinScore;

            int emptyCount = window.Count(state => state == Board.PlayerState.Empty);

            int score = (int)(EvaluateCount(playerCount, emptyCount) * (PlayerThreatWeight * WeightModifier));
            score -= (int)(EvaluateCount(opponentCount, emptyCount) * (OpponentThreatWeight * WeightModifier));

            return score;
        }

        private int EvaluateCount(int pieceCount, int emptyCount) =>
            pieceCount switch
            {
                4                     => WinScore,
                3 when emptyCount > 0 => AlmostScoredValue,
                2 when emptyCount > 1 => PotentialPairValue,
                1 when emptyCount > 2 => MeaningfulMoveValue,
                _                     => 0,
            };


        private static int CountCenterControl(Board board, Board.PlayerState player)
        {
            int centerControl = 0;
            for (int column = 2; column <= 4; column++)
            {
                for (int row = 0; row < Board.Height; row++)
                {
                    if (board.GetSpace(row, column) == player) centerControl += 2 - Math.Abs(3 - column);
                }
            }

            return centerControl;
        }

        private static Board.PlayerState GetOpponent(Board.PlayerState player) =>
            player == Board.PlayerState.Player1 ? Board.PlayerState.Player2 : Board.PlayerState.Player1;
    }
}