using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConnectFour.Tests
{
    [TestClass]
    public class AITests
    {
        private AI _ai;
        private Board _board;

        [TestInitialize]
        public void Setup()
        {
            _ai = new AI(new Random(553)); // Use a fixed seed for reproducibility
            _ai.Difficulty = 10;
            _board = new Board();
        }

        [TestMethod]
        public void GetBestMove_EmptyBoard_ReturnsMiddleColumn()
        {
            int bestMove = _ai.GetBestMove(_board, Board.PlayerState.Player1);
            Assert.IsTrue(bestMove is 3 or 4, "AI should choose a middle column (3 or 4) for an empty board");
        }

        [TestMethod]
        public void GetBestMove_BlocksOpponentWin()
        {
            // Set up a board where the opponent can win in the next move
            _board.MakeMove(Board.PlayerState.Player2, 0);
            _board.MakeMove(Board.PlayerState.Player2, 0);
            _board.MakeMove(Board.PlayerState.Player2, 0);

            int bestMove = _ai.GetBestMove(_board, Board.PlayerState.Player1);
            Assert.AreEqual(0, bestMove); // Block the win
        }

        [TestMethod]
        public void GetBestMove_TakesWinningMove()
        {
            // Set up a board where the AI can win in the next move
            _board.MakeMove(Board.PlayerState.Player1, 0);
            _board.MakeMove(Board.PlayerState.Player1, 0);
            _board.MakeMove(Board.PlayerState.Player1, 0);

            int bestMove = _ai.GetBestMove(_board, Board.PlayerState.Player1);
            Assert.AreEqual(0, bestMove); // Take the win
        }

        [TestMethod]
        public void Negamax_WinningPosition_ReturnsWinScore()
        {
            // Set up a winning position
            _board.MakeMove(Board.PlayerState.Player1, 0);
            _board.MakeMove(Board.PlayerState.Player1, 0);
            _board.MakeMove(Board.PlayerState.Player1, 0);
            _board.MakeMove(Board.PlayerState.Player1, 0);

            int score = _ai.Negamax(_board, 1, int.MinValue, int.MaxValue, Board.PlayerState.Player1);
            Assert.AreEqual(AI.WinScore, score);
        }

        [TestMethod]
        public void EvaluateBoard_EmptyBoard_ReturnsSmallValue()
        {
            int score = _ai.EvaluateBoard(_board, Board.PlayerState.Player1);
            Assert.IsTrue(Math.Abs(score) < _ai.Difficulty * 10, "Score for empty board should be small");
        }


        [TestMethod]
        public void EvaluateBoard_FourInARow_ReturnsWinScore()
        {
            for (int i = 0; i < 4; i++)
                _board.MakeMove(Board.PlayerState.Player1, i);

            int score = _ai.EvaluateBoard(_board, Board.PlayerState.Player1);
            Assert.AreEqual(AI.WinScore, score, "Four in a row should return WinScore");
        }

        [TestMethod]
        public void EvaluateBoard_ThreeOpenEnded_ReturnsOpenEndedThreatValue()
        {
            _board.MakeMove(Board.PlayerState.Player1, 2);
            _board.MakeMove(Board.PlayerState.Player1, 3);
            _board.MakeMove(Board.PlayerState.Player1, 4);

            int score = _ai.EvaluateBoard(_board, Board.PlayerState.Player1);
            Assert.AreEqual(AI.OpenEndedThreatValue, score, AI.OpenEndedThreatValue*0.1, "Three in a row with open ends should return OpenEndedThreatValue");
        }

        [TestMethod]
        public void EvaluateBoard_ThreeSingleEnded_ReturnsSingleEndedThreatValue()
        {
            _board.MakeMove(Board.PlayerState.Player2, 1);
            _board.MakeMove(Board.PlayerState.Player1, 2);
            _board.MakeMove(Board.PlayerState.Player1, 3);
            _board.MakeMove(Board.PlayerState.Player1, 4);

            int score = _ai.EvaluateBoard(_board, Board.PlayerState.Player1);
            Assert.AreEqual(AI.SingleEndedThreatValue, score, AI.SingleEndedThreatValue * 0.1, "Three in a row with one end blocked should return SingleEndedThreatValue");
        }

        [TestMethod]
        public void EvaluateBoard_TwoOpenEnded_ReturnsSingleEndedThreatValue()
        {
            _board.MakeMove(Board.PlayerState.Player1, 2);
            _board.MakeMove(Board.PlayerState.Player1, 3);

            int score = _ai.EvaluateBoard(_board, Board.PlayerState.Player1);
            Assert.AreEqual(AI.SingleEndedThreatValue, score, "Two in a row with open ends should return SingleEndedThreatValue");
        }

        [TestMethod]
        public void EvaluateBoard_MultipleThreats_ReturnsCorrectScore()
        {
            // Set up two three-in-a-row threats
            _board.MakeMove(Board.PlayerState.Player1, 0);
            _board.MakeMove(Board.PlayerState.Player1, 1);
            _board.MakeMove(Board.PlayerState.Player1, 2);

            _board.MakeMove(Board.PlayerState.Player1, 4);
            _board.MakeMove(Board.PlayerState.Player1, 5);
            _board.MakeMove(Board.PlayerState.Player1, 6);

            int score = _ai.EvaluateBoard(_board, Board.PlayerState.Player1);
            Assert.AreEqual(AI.OpenEndedThreatValue * 2, score, AI.OpenEndedThreatValue * 0.2, "Two open-ended three-in-a-row threats should return twice the OpenEndedThreatValue");
        }

        [TestMethod]
        public void EvaluateBoard_MixedThreats_ReturnsCorrectScore()
        {
            // Set up one three-in-a-row and one two-in-a-row threat
            _board.MakeMove(Board.PlayerState.Player1, 0);
            _board.MakeMove(Board.PlayerState.Player1, 1);
            _board.MakeMove(Board.PlayerState.Player1, 2);

            _board.MakeMove(Board.PlayerState.Player1, 4);
            _board.MakeMove(Board.PlayerState.Player1, 5);

            int expectedScore = AI.OpenEndedThreatValue + AI.SingleEndedThreatValue;
            int actualScore = _ai.EvaluateBoard(_board, Board.PlayerState.Player1);
            Assert.AreEqual(expectedScore, actualScore, 100, "Mixed threats should return the sum of their respective threat values within a tolerance");
        }


        [TestMethod]
        public void EvaluateBoard_OpponentThreats_ReturnsNegativeScore()
        {
            _board.MakeMove(Board.PlayerState.Player2, 1);
            _board.MakeMove(Board.PlayerState.Player2, 2);
            _board.MakeMove(Board.PlayerState.Player2, 3);

            int score = _ai.EvaluateBoard(_board, Board.PlayerState.Player1);
            Assert.IsTrue(score < 0, "Opponent threats should result in a negative score");
            Assert.AreEqual(-AI.OpenEndedThreatValue, score, AI.OpenEndedThreatValue * 0.1, "Opponent's three in a row should return negative OpenEndedThreatValue");
        }

        [TestMethod]
        public void Difficulty_SetValue_UpdatesRelatedFields()
        {
            _ai.Difficulty = 7;
            Assert.AreEqual(7, _ai.Difficulty);
        }


        [TestMethod]
        public void GetBestMove_ConsistentWithSeed()
        {
            AI ai1 = new(new Random(42));
            AI ai2 = new(new Random(42));

            Board board = new();
            board.MakeMove(Board.PlayerState.Player1, 3);
            board.MakeMove(Board.PlayerState.Player2, 3);

            int move1 = ai1.GetBestMove(board, Board.PlayerState.Player1);
            int move2 = ai2.GetBestMove(board, Board.PlayerState.Player1);

            Assert.AreEqual(move1, move2, "AIs with the same seed should make the same move");
        }

        [TestMethod]
        public void GetBestMove_DifferentWithDifferentSeeds()
        {
            AI ai1 = new(new Random(42));
            AI ai2 = new(new Random(43));

            Board board = new();
            board.MakeMove(Board.PlayerState.Player1, 3);
            board.MakeMove(Board.PlayerState.Player2, 3);
            board.MakeMove(Board.PlayerState.Player1, 2);
            board.MakeMove(Board.PlayerState.Player2, 4);

            int differentMoves = 0;
            for (int i = 0; i < 10; i++)
            {
                int move1 = ai1.GetBestMove(board, Board.PlayerState.Player1);
                int move2 = ai2.GetBestMove(board, Board.PlayerState.Player1);
                if (move1 != move2) differentMoves++;
            }

            Assert.IsTrue(differentMoves > 0, "AIs with different seeds should make different moves at least once in 10 tries");
        }


        [TestMethod]
        public void Difficulty_AffectsDecisionMaking()
        {
            AI easyAI = new(new Random(55)) { Difficulty = 1 };
            AI hardAI = new(new Random(55)) { Difficulty = 10 };

            Board board = new();
            // Set up a board state where there's a clear best move, but also a tempting center move
            board.MakeMove(Board.PlayerState.Player1, 0);
            board.MakeMove(Board.PlayerState.Player1, 4);
            board.MakeMove(Board.PlayerState.Player1, 5);
            board.MakeMove(Board.PlayerState.Player2, 6);
            board.MakeMove(Board.PlayerState.Player2, 6);

            int easyMove = easyAI.GetBestMove(board, Board.PlayerState.Player2);
            int hardMove = hardAI.GetBestMove(board, Board.PlayerState.Player2);

            Assert.AreEqual(2, hardMove, "Hard AI should block instead of making the easy 3 in a row.");
            Assert.AreEqual(6, easyMove, "Easy AI should attempt to win instead of blocking");
        }


        [TestMethod]
        public void GetBestMove_BlocksThreatenedWin()
        {
            // Set up the board state
            _board.MakeMove(Board.PlayerState.Player2, 0);
            _board.MakeMove(Board.PlayerState.Player1, 2);
            _board.MakeMove(Board.PlayerState.Player1, 3);
            _board.MakeMove(Board.PlayerState.Player2, 3);

            // AI should block the win by playing in column 4
            int bestMove = _ai.GetBestMove(_board, Board.PlayerState.Player2);
            Assert.AreEqual(4, bestMove, "AI should block the potential win in column 4");
        }

        [TestMethod]
        public void GetBestMove_BlocksThreatenedWin2()
        {
            _board.MakeMove(Board.PlayerState.Player1, 3);
            _board.MakeMove(Board.PlayerState.Player2, 0);
            _board.MakeMove(Board.PlayerState.Player1, 6);
            _board.MakeMove(Board.PlayerState.Player2, 2);
            _board.MakeMove(Board.PlayerState.Player1, 4);

            int bestMove = _ai.GetBestMove(_board, Board.PlayerState.Player2); 
            Assert.AreEqual(4, bestMove, "AI should block the potential win in column 4");
        }

        [TestMethod]
        public void GetBestMove_PrioritizesWinningMove()
        {
            // Set up a board where AI can win or block opponent's win
            _board.MakeMove(Board.PlayerState.Player2, 0);
            _board.MakeMove(Board.PlayerState.Player2, 0);
            _board.MakeMove(Board.PlayerState.Player2, 0);
            _board.MakeMove(Board.PlayerState.Player1, 1);
            _board.MakeMove(Board.PlayerState.Player1, 1);
            _board.MakeMove(Board.PlayerState.Player1, 1);

            // AI should choose to win (column 0) rather than block (column 1)
            int bestMove = _ai.GetBestMove(_board, Board.PlayerState.Player2);
            Assert.AreEqual(0, bestMove, "AI should choose the winning move in column 0");

            // Now test blocking
            _board.ResetBoardState();
            _board.MakeMove(Board.PlayerState.Player1, 0);
            _board.MakeMove(Board.PlayerState.Player1, 0);
            _board.MakeMove(Board.PlayerState.Player1, 0);

            bestMove = _ai.GetBestMove(_board, Board.PlayerState.Player2);
            Assert.AreEqual(0, bestMove, "AI should block the opponent's winning move in column 0");
        }


        [TestMethod]
        public void EvaluateLine_VerticalWin_ReturnsWinScore()
        {
            for (int i = 0; i < 4; i++)
                _board.MakeMove(Board.PlayerState.Player1, 0);

            int score = _ai.EvaluateBoard(_board, Board.PlayerState.Player1);
            Assert.AreEqual(AI.WinScore, score, "Vertical win should return WinScore");
        }

        [TestMethod]
        public void EvaluateLine_HorizontalWin_ReturnsWinScore()
        {
            for (int i = 0; i < 4; i++)
                _board.MakeMove(Board.PlayerState.Player1, i);

            int score = _ai.EvaluateBoard(_board, Board.PlayerState.Player1);
            Assert.AreEqual(AI.WinScore, score, "Horizontal win should return WinScore");
        }

        [TestMethod]
        public void EvaluateLine_DiagonalWin_ReturnsWinScore()
        {
            _board.MakeMove(Board.PlayerState.Player1, 0);
            _board.MakeMove(Board.PlayerState.Player2, 1);
            _board.MakeMove(Board.PlayerState.Player1, 1);
            _board.MakeMove(Board.PlayerState.Player2, 2);
            _board.MakeMove(Board.PlayerState.Player2, 2);
            _board.MakeMove(Board.PlayerState.Player1, 2);
            _board.MakeMove(Board.PlayerState.Player2, 3);
            _board.MakeMove(Board.PlayerState.Player2, 3);
            _board.MakeMove(Board.PlayerState.Player2, 3);
            _board.MakeMove(Board.PlayerState.Player1, 3);

            int score = _ai.EvaluateBoard(_board, Board.PlayerState.Player1);
            Assert.AreEqual(AI.WinScore, score, "Diagonal win should return WinScore");
        }

        [TestMethod]
        public void EvaluateWindow_ThreeInARowOpen_ReturnsOpenEndedThreatValue()
        {
            _board.MakeMove(Board.PlayerState.Player1, 1);
            _board.MakeMove(Board.PlayerState.Player1, 2);
            _board.MakeMove(Board.PlayerState.Player1, 3);

            int score = _ai.EvaluateBoard(_board, Board.PlayerState.Player1);
            Assert.AreEqual(AI.OpenEndedThreatValue, score, AI.OpenEndedThreatValue * 0.1, "Three in a row with open ends should return OpenEndedThreatValue");
        }

        [TestMethod]
        public void EvaluateWindow_ThreeInARowBlocked_ReturnsSingleEndedThreatValue()
        {
            _board.MakeMove(Board.PlayerState.Player2, 0);
            _board.MakeMove(Board.PlayerState.Player1, 1);
            _board.MakeMove(Board.PlayerState.Player1, 2);
            _board.MakeMove(Board.PlayerState.Player1, 3);

            int score = _ai.EvaluateBoard(_board, Board.PlayerState.Player1);
            Assert.AreEqual(AI.SingleEndedThreatValue, score, "Three in a row with one end blocked should return SingleEndedThreatValue");
        }

        [TestMethod]
        public void EvaluateWindow_TwoInARowOpen_ReturnsSingleEndedThreatValue()
        {
            _board.MakeMove(Board.PlayerState.Player1, 1);
            _board.MakeMove(Board.PlayerState.Player1, 2);

            int score = _ai.EvaluateBoard(_board, Board.PlayerState.Player1);
            Assert.AreEqual(AI.SingleEndedThreatValue, score, "Two in a row with open ends should return SingleEndedThreatValue");
        }

        [TestMethod]
        public void EvaluateWindow_TwoInARowBlocked_ReturnsPotentialThreatValue()
        {
            _board.MakeMove(Board.PlayerState.Player2, 0);
            _board.MakeMove(Board.PlayerState.Player1, 1);
            _board.MakeMove(Board.PlayerState.Player1, 2);

            int score = _ai.EvaluateBoard(_board, Board.PlayerState.Player1);
            Assert.AreEqual(AI.PotentialThreatValue, score, "Two in a row with one end blocked should return PotentialThreatValue");
        }

        [TestMethod]
        public void EvaluateBoard_MixedThreats_ReturnsExpectedScore()
        {
            _board.MakeMove(Board.PlayerState.Player1, 0);
            _board.MakeMove(Board.PlayerState.Player1, 1);
            _board.MakeMove(Board.PlayerState.Player2, 3);
            _board.MakeMove(Board.PlayerState.Player2, 4);

            int expectedScore = AI.SingleEndedThreatValue - AI.SingleEndedThreatValue;
            int actualScore = _ai.EvaluateBoard(_board, Board.PlayerState.Player1);
            Assert.AreEqual(expectedScore, actualScore, 20, "Mixed threats should return the difference of threat values within a tolerance");
        }

        [TestMethod]
        public void EvaluateBoard_CenterControl_ReturnsPositiveScore()
        {
            _board.MakeMove(Board.PlayerState.Player1, 3);

            int score = _ai.EvaluateBoard(_board, Board.PlayerState.Player1);
            Assert.IsTrue(score > 0, "Control of center column should result in a positive score");
        }

        [TestMethod]
        public void GetBestMove_PrefersCenterColumns()
        {
            // Empty board
            int move = _ai.GetBestMove(_board, Board.PlayerState.Player1);
            Assert.IsTrue(move == 3 || move == 4, "AI should prefer center columns on an empty board");
        }

        [TestMethod]
        public void GetBestMove_AvoidsSelfBlocking()
        {
            _board.MakeMove(Board.PlayerState.Player1, 0);
            _board.MakeMove(Board.PlayerState.Player1, 0);
            _board.MakeMove(Board.PlayerState.Player2, 1);
            _board.MakeMove(Board.PlayerState.Player2, 1);
            _board.MakeMove(Board.PlayerState.Player2, 1);

            int move = _ai.GetBestMove(_board, Board.PlayerState.Player1);
            Assert.AreNotEqual(0, move, "AI should avoid moves that allow the opponent to win immediately");
        }

        [TestMethod]
        public void Negamax_DepthLimit_ReturnsEvaluationScore()
        {
            // Set up a non-terminal position
            _board.MakeMove(Board.PlayerState.Player1, 3);
            _board.MakeMove(Board.PlayerState.Player2, 4);

            int score = _ai.Negamax(_board, 0, int.MinValue, int.MaxValue, Board.PlayerState.Player1);
            Assert.AreNotEqual(0, score, "Negamax at depth 0 should return a non-zero evaluation score");
        }

        [TestMethod]
        public void Negamax_AlphaBetaPruning_ReturnsConsistentScore()
        {
            // Set up a position
            _board.MakeMove(Board.PlayerState.Player1, 3);
            _board.MakeMove(Board.PlayerState.Player2, 4);

            int scoreWithoutPruning = _ai.Negamax(_board, 5, int.MinValue, int.MaxValue, Board.PlayerState.Player1);
            int scoreWithPruning = _ai.Negamax(_board, 5, -1000, 1000, Board.PlayerState.Player1);

            Assert.AreEqual(scoreWithoutPruning, scoreWithPruning, "Alpha-beta pruning should not affect the final score");
        }
    }
}