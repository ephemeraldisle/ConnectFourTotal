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
            _ai = new AI(new Random(42)); // Use a fixed seed for reproducibility
            _ai.Difficulty = 10;
            _board = new Board();
        }

        [TestMethod]
        public void GetBestMove_EmptyBoard_ReturnsMiddleColumn()
        {
            int bestMove = _ai.GetBestMove(_board, Board.PlayerState.Player1);
            Assert.IsTrue(bestMove == 3 || bestMove == 4, "AI should choose a middle column (3 or 4) for an empty board");
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
        public void CountThreats_ThreeInARow_ReturnsExpectedValue()
        {
            _board.MakeMove(Board.PlayerState.Player1, 0);
            _board.MakeMove(Board.PlayerState.Player1, 1);
            _board.MakeMove(Board.PlayerState.Player1, 2);

            int threats = AI.CountThreats(_board, Board.PlayerState.Player1);
            Assert.AreEqual(AI.SingleEndedThreatValue, threats, "Three in a row should be counted as a single-ended threat");
        }

        [TestMethod]
        public void EvaluateDirection_ThreeInARow_ReturnsExpectedValue()
        {
            _board.MakeMove(Board.PlayerState.Player1, 0);
            _board.MakeMove(Board.PlayerState.Player1, 1);
            _board.MakeMove(Board.PlayerState.Player1, 2);

            int threat = AI.EvaluateDirection(_board, 0, 0, 0, 1, Board.PlayerState.Player1);
            Assert.AreEqual(AI.SingleEndedThreatValue, threat, "Three in a row should be evaluated as a single-ended threat");
        }

        [TestMethod]
        public void EvaluateConsecutive_FourInARow_ReturnsWinScore()
        {
            int score = AI.EvaluateConsecutive(4, 1, 1);
            Assert.AreEqual(AI.WinScore, score);
        }

        [TestMethod]
        public void EvaluateConsecutive_ThreeOpenEnded_ReturnsOpenEndedThreatValue()
        {
            int score = AI.EvaluateConsecutive(3, 1, 1);
            Assert.AreEqual(AI.OpenEndedThreatValue, score);
        }

        [TestMethod]
        public void EvaluateConsecutive_ThreeSingleEnded_ReturnsSingleEndedThreatValue()
        {
            int score = AI.EvaluateConsecutive(3, 1, 0);
            Assert.AreEqual(AI.SingleEndedThreatValue, score);
        }

        [TestMethod]
        public void EvaluateConsecutive_TwoOpenEnded_ReturnsPotentialThreatValue()
        {
            int score = AI.EvaluateConsecutive(2, 1, 1);
            Assert.AreEqual(AI.PotentialThreatValue, score);
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
            AI easyAI = new(new Random(42)) { Difficulty = -10 };
            AI hardAI = new(new Random(42)) { Difficulty = 10 };

            Board board = new();
            // Set up a board state where there's a clear best move, but also a tempting center move
            board.MakeMove(Board.PlayerState.Player1, 0);
            board.MakeMove(Board.PlayerState.Player1, 0);
            board.MakeMove(Board.PlayerState.Player1, 0);
            board.MakeMove(Board.PlayerState.Player2, 3);
            board.MakeMove(Board.PlayerState.Player2, 3);

            int easyMove = easyAI.GetBestMove(board, Board.PlayerState.Player2);
            int hardMove = hardAI.GetBestMove(board, Board.PlayerState.Player2);

            Assert.AreEqual(0, hardMove, "Hard AI should block the winning move");
            Assert.AreNotEqual(0, easyMove, "Easy AI should make a different move than the hard AI");
        }
    }
}