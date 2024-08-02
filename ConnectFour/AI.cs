using System.Diagnostics;
using static System.Formats.Asn1.AsnWriter;

namespace ConnectFour
{
    public class AI
    {
        public const int MinimumDifficulty = 1;
        public const int MaximumDifficulty = 10;

        //Arbitrary high value:
        public const int WinScore = (int)(int.MaxValue * 0.9);

        //These weights could be tweaked by someone who knows more about Connect 4 strategy to be better.
        private const int PlayerThreatWeight = 6;
        private const int OpponentThreatWeight = 5;
        private const int PlayerCenterWeight = 3;
        private const int OpponentCenterWeight = 2;

        public const int OpenEndedThreatValue = 10000;
        public const int SingleEndedThreatValue = 100;
        public const int PotentialThreatValue = 10;

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
            orderToUse = [0, 1, 2, 3, 4, 5, 6];
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
            for (column = 0; column <= Board.Width - Board.WinningLength; column++)
            {
                int lineScore = EvaluateLine(board, 0, column, 1, 1, player, opponent);
                if (Math.Abs(lineScore) == WinScore) return lineScore;
                score += lineScore;
            }
            for (row = 1; row <= Board.Height - Board.WinningLength; row++)
            {
                int lineScore = EvaluateLine(board, row, 0, 1, 1, player, opponent);
                if (Math.Abs(lineScore) == WinScore) return lineScore;
                score += lineScore;
            }


            //Check all \ diagonals

            for (column = 0; column <= Board.Width - Board.WinningLength; column++)
            {
                int lineScore = EvaluateLine(board, Board.Height - 1, column, -1, 1, player, opponent);
                if (Math.Abs(lineScore) == WinScore) return lineScore;
                score += lineScore;
            }
            for (row = Board.Height - 2; row >= Board.WinningLength - 1; row--)
            {
                int lineScore = EvaluateLine(board, row, 0, -1, 1, player, opponent);
                if (Math.Abs(lineScore) == WinScore) return lineScore;
                score += lineScore;
            }

            //Adjusts how much the weights matter in evaluating the board position based on difficulty - lower difficulty takes them less into effect.
            double weightModifier = 0.5 + _difficultyModifier;



            // Add a small bonus for center control
            score += (CountCenterControl(board, player) - CountCenterControl(board, opponent));


            //score += (int)(CountThreats(board, player) * (PlayerThreatWeight * weightModifier));
            //score -= (int)(CountThreats(board, opponent) * (OpponentThreatWeight * weightModifier));
            //score += (int)(CountCenterControl(board, player) * (PlayerCenterWeight * weightModifier));
            //score -= (int)(CountCenterControl(board, opponent) * (OpponentCenterWeight * weightModifier));

            //Randomly fails at score analysis, introducing errors at lower difficulty levels.
            if (Difficulty < MaximumDifficulty)
            {
                int randomOffset = _random.Next(1, _errorMaximum + 1);
                score += randomOffset;
            }

            return score;
        }


        private int EvaluateLine(Board board, int startRow, int startColumn, int rowDirection, int columnDirection, Board.PlayerState player, Board.PlayerState opponent)
        {
            int bestScore = 0;
            Queue<Board.PlayerState> window = new(Board.WinningLength);

            for (int i = 0; i < Board.Height; i++)
            {
                int row = startRow + i * rowDirection;
                int column = startColumn + i * columnDirection;

                if (!Board.IsValidPosition(row, column))
                    break;

                Board.PlayerState spaceState = board.GetSpace(row, column);
                if (spaceState == Board.PlayerState.Empty && row > board.GetColumnHeight(column))
                    spaceState = Board.PlayerState.Unplayable;

                window.Enqueue(spaceState);


                if (window.Count > Board.WinningLength)
                    window.Dequeue();

                if (window.Count == Board.WinningLength)
                {
                    int score = EvaluateWindow(window, player, opponent);
                    if (score != 0)
                        Debug.WriteLine($"Window: {string.Join(",", window)} | Player: {player} | Score: {score}");

                    if (Math.Abs(score) > Math.Abs(bestScore))
                        bestScore = score;
                }
            }
            if (bestScore!= 0)
                Debug.WriteLine($"EvaluateLine Result: Start({startRow},{startColumn}), Direction({rowDirection},{columnDirection}), Score: {bestScore}");


            return bestScore;
        }



        private int EvaluateWindow(Queue<Board.PlayerState> window, Board.PlayerState player, Board.PlayerState opponent)
        {
            int playerCount = window.Count(state => state == player);
            if (playerCount == Board.WinningLength) return WinScore;


            int opponentCount = window.Count(state => state == opponent);
            if (opponentCount == Board.WinningLength) return -WinScore;

            int emptyCount = window.Count(state => state == Board.PlayerState.Empty);

            var score = EvaluateCount(playerCount, emptyCount) - EvaluateCount(opponentCount, emptyCount);
            if (score != 0)
                Debug.WriteLine($"EvaluateWindow: Player:{playerCount}, Opponent:{opponentCount}, Empty:{emptyCount}, Score:{EvaluateCount(playerCount, emptyCount) - EvaluateCount(opponentCount, emptyCount)}");

            return EvaluateCount(playerCount, emptyCount) - EvaluateCount(opponentCount, emptyCount);

        }

        private int EvaluateCount(int pieceCount, int emptyCount) =>
            pieceCount switch
            {
                4 => WinScore,
                3 when emptyCount > 0 => OpenEndedThreatValue,
                2 when emptyCount > 1 => SingleEndedThreatValue,
                1 when emptyCount > 2 => PotentialThreatValue,
                _ => 0,
            };



        private static int CountCenterControl(Board board, Board.PlayerState player)
        {
            int centerControl = 0;
            for (int column = 2; column <= 4; column++)
                for (int row = 0; row < Board.Height; row++)
                    if (board.GetSpace(row, column) == player)
                        centerControl += 2 - Math.Abs(3 - column);

            return centerControl;
        }

        private static Board.PlayerState GetOpponent(Board.PlayerState player) =>
            player == Board.PlayerState.Player1 ? Board.PlayerState.Player2 : Board.PlayerState.Player1;
    }
}