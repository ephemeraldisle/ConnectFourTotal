namespace ConnectFour
{
    public class AI
    {
        public const int MinimumDifficulty = 1;
        public const int MaximumDifficulty = 10;

        //Arbitrary high value:
        public const int WinScore = 1000000;

        //These weights could be tweaked by someone who knows more about Connect 4 strategy to be better.
        private const int PlayerThreatWeight = 6;
        private const int OpponentThreatWeight = 5;
        private const int PlayerCenterWeight = 3;
        private const int OpponentCenterWeight = 2;

        public const int OpenEndedThreatValue = 1000;
        public const int SingleEndedThreatValue = 10;
        public const int PotentialThreatValue = 1;

        private int _difficulty = 5;
        private double _difficultyModifier = 0.5;
        private int _errorMaximum = 52;

        public int Difficulty
        {
            get => _difficulty;
            set
            {
                _difficulty = value;
                _difficultyModifier = value * 0.1;
                _errorMaximum = 102 - _difficulty * 10;
                _transpositionTable.Clear();
            }
        }

        private readonly static int[] StandardColumnOrder = [3, 2, 4, 1, 5, 0, 6];
        private readonly static int[] AlternateColumnOrder = [3, 4, 2, 5, 1, 6, 0];
        private readonly Dictionary<string, int> _transpositionTable = new();


        private readonly Random _random;

        public AI(Random? random = null)
        {
            _random = random ?? Random.Shared;
        }

        public int GetBestMove(Board board, Board.PlayerState player)
        {
            int bestMove = -1;
            int bestScore = int.MinValue;

            //By varying which order moves are checked, slightly, should lead to more varied play.
            int[] orderToUse = _random.Next(0, 2) == 0 ? StandardColumnOrder : AlternateColumnOrder;

            foreach (int column in orderToUse)
            {
                if (!board.CanMakeMove(column)) continue;

                Board newBoard = board.Clone();
                newBoard.MakeMove(player, column);

                if (newBoard.CheckVictory(player, column)) return column;

                int score = -Negamax(newBoard, Difficulty, int.MinValue, int.MaxValue, GetOpponent(player));

                if (score <= bestScore) continue;

                bestScore = score;
                bestMove = column;
            }

            return bestMove;
        }

        public int Negamax(Board board, int depth, int alpha, int beta, Board.PlayerState player)
        {
            string boardString = board.ToString();
            if (_transpositionTable.TryGetValue(boardString, out int cachedScore))
            {
                //Randomly misremembers score analysis, introducing errors at lower difficulty levels.
                int randomOffset = (int)(_random.Next(1, _errorMaximum) * (1 - _difficultyModifier));
                return cachedScore + randomOffset;
            }

            if (depth == 0 || board.IsFull()) return EvaluateBoard(board, player);

            int maxScore = int.MinValue;

            //By randomly picking between two equally optimal orders to check in, should introduce additional variety to the play.
            int[] orderToUse = _random.Next(0, 2) == 0 ? StandardColumnOrder : AlternateColumnOrder;
            foreach (int column in orderToUse)
            {
                if (!board.CanMakeMove(column)) continue;

                Board newBoard = board.Clone();
                newBoard.MakeMove(player, column);

                if (newBoard.CheckVictory(player, column)) return WinScore;

                int score = -Negamax(newBoard, depth - 1, -beta, -alpha, GetOpponent(player));

                maxScore = Math.Max(maxScore, score);
                alpha = Math.Max(alpha, score);

                if (alpha >= beta) break;
            }

            _transpositionTable[boardString] = maxScore;
            return maxScore;
        }


        public int EvaluateBoard(Board board, Board.PlayerState player)
        {
            int score = 0;
            Board.PlayerState opponent = GetOpponent(player);

            //Adjusts how much the weights matter in evaluating the board position based on difficulty - lower difficulty takes them less into effect.
            double weightModifier = 0.5 + _difficultyModifier;

            score += (int)(CountThreats(board, player) * (PlayerThreatWeight * weightModifier));
            score -= (int)(CountThreats(board, opponent) * (OpponentThreatWeight * weightModifier));
            score += (int)(CountCenterControl(board, player) * (PlayerCenterWeight * weightModifier));
            score -= (int)(CountCenterControl(board, opponent) * (OpponentCenterWeight * weightModifier));

            //Randomly fails at score analysis, introducing errors at lower difficulty levels.
            if (Difficulty < MaximumDifficulty)
            {
                int randomOffset = _random.Next(1, _errorMaximum + 1);
                score += randomOffset;
            }

            return score;
        }

        public static int CountThreats(Board board, Board.PlayerState player)
        {
            int threats = 0;

            // Horizontal threats
            for (int row = 0; row < Board.Height; row++)
            {
                threats += EvaluateDirection(board, row, 0, 0, 1, player);
            }

            // Vertical threats
            for (int column = 0; column < Board.Width; column++)
            {
                threats += EvaluateDirection(board, 0, column, 1, 0, player);
            }

            // Diagonal threats (/)
            for (int row = 0; row < Board.Height - 3; row++)
            {
                threats += EvaluateDirection(board, row, 0, 1, 1, player);
            }

            for (int column = 1; column < Board.Width - 3; column++)
            {
                threats += EvaluateDirection(board, 0, column, 1, 1, player);
            }

            // Diagonal threats (\)
            for (int row = 3; row < Board.Height; row++)
            {
                threats += EvaluateDirection(board, row, 0, -1, 1, player);
            }

            for (int column = 1; column < Board.Width - 3; column++)
            {
                threats += EvaluateDirection(board, Board.Height - 1, column, -1, 1, player);
            }

            return threats;
        }

        public static int EvaluateDirection(Board board, int startRow, int startColumn, int rowDirection, int columnDirection,
            Board.PlayerState player)
        {
            int threats = 0;
            int consecutive = 0;
            int emptyBefore = 0;
            int emptyAfter = 0;

            int row = startRow;
            int column = startColumn;

            while (Board.IsValidPosition(row, column))
            {
                Board.PlayerState currentState = board.GetSpace(row, column);

                if (currentState == player)
                {
                    consecutive++;
                }
                else if (currentState == Board.PlayerState.Empty)
                {
                    if (consecutive > 0)
                    {
                        emptyAfter++;
                        threats += EvaluateConsecutive(consecutive, emptyBefore, emptyAfter);
                        emptyBefore = emptyAfter;
                        emptyAfter = 0;
                        consecutive = 0;
                    }
                    else
                    {
                        emptyBefore++;
                    }
                }
                else // Opponent's piece
                {
                    if (consecutive > 0)
                        threats += EvaluateConsecutive(consecutive, emptyBefore, emptyAfter);

                    emptyBefore = 0;
                    emptyAfter = 0;
                    consecutive = 0;
                }

                row += rowDirection;
                column += columnDirection;
            }

            if (consecutive > 0)
                threats += EvaluateConsecutive(consecutive, emptyBefore, emptyAfter);

            return threats;
        }

        public static int EvaluateConsecutive(int consecutive, int emptyBefore, int emptyAfter)
        {
            return consecutive switch
            {
                >= Board.WinningLength                   => WinScore,
                3 when emptyBefore > 0 && emptyAfter > 0 => OpenEndedThreatValue,
                3 when emptyBefore > 0 || emptyAfter > 0 => SingleEndedThreatValue,
                2 when emptyBefore > 0 && emptyAfter > 0 => PotentialThreatValue,
                _                                        => 0,
            };
        }

        private static int CountCenterControl(Board board, Board.PlayerState player) =>
            Enumerable.Range(2, 3).
                       Sum(column => Enumerable.Range(0, Board.Height).
                                                Count(row => board.GetSpace(row, column) == player));

        private static Board.PlayerState GetOpponent(Board.PlayerState player) =>
            player == Board.PlayerState.Player1 ? Board.PlayerState.Player2 : Board.PlayerState.Player1;
    }
}