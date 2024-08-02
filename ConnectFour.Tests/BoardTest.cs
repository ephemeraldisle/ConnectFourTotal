using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConnectFour.Tests
{
    [TestClass]
    public class BoardTest
    {
        private Board _board;

        [TestInitialize]
        public void SetUp()
        {
            _board = new Board();
        }

        [TestMethod]
        public void IsValidPosition_ValidPositions_ReturnsTrue()
        {
            Assert.IsTrue(Board.IsValidPosition(0, 0));
            Assert.IsTrue(Board.IsValidPosition(Board.Height - 1, Board.Width - 1));
        }

        [TestMethod]
        public void IsValidPosition_InvalidPositions_ReturnsFalse()
        {
            Assert.IsFalse(Board.IsValidPosition(-1, 0));
            Assert.IsFalse(Board.IsValidPosition(0, -1));
            Assert.IsFalse(Board.IsValidPosition(Board.Height, 0));
            Assert.IsFalse(Board.IsValidPosition(0, Board.Width));
        }

        [TestMethod]
        public void ResetBoardState_ClearsBoard()
        {
            // Fill the board
            for (int col = 0; col < Board.Width; col++)
            {
                for (int row = 0; row < Board.Height; row++)
                {
                    _board.MakeMove(Board.PlayerState.Player1, col);
                }
            }

            _board.ResetBoardState();

            for (int row = 0; row < Board.Height; row++)
            {
                for (int col = 0; col < Board.Width; col++)
                {
                    Assert.AreEqual(Board.PlayerState.Empty, _board.GetSpace(row, col));
                }
            }
        }

        [TestMethod]
        public void GetSpace_ReturnsCorrectState()
        {
            _board.MakeMove(Board.PlayerState.Player1, 0);
            Assert.AreEqual(Board.PlayerState.Player1, _board.GetSpace(0, 0));
            Assert.AreEqual(Board.PlayerState.Empty, _board.GetSpace(1, 0));
        }

        [TestMethod]
        public void GetColumnHeight_ReturnsCorrectHeight()
        {
            _board.MakeMove(Board.PlayerState.Player1, 0);
            _board.MakeMove(Board.PlayerState.Player2, 0);
            Assert.AreEqual(2, _board.GetColumnHeight(0));
            Assert.AreEqual(0, _board.GetColumnHeight(1));
        }

        [TestMethod]
        public void CanMakeMove_ValidMove_ReturnsTrue()
        {
            Assert.IsTrue(_board.CanMakeMove(0));
        }

        [TestMethod]
        public void CanMakeMove_FullColumn_ReturnsFalse()
        {
            for (int i = 0; i < Board.Height; i++)
            {
                _board.MakeMove(Board.PlayerState.Player1, 0);
            }
            Assert.IsFalse(_board.CanMakeMove(0));
        }

        [TestMethod]
        public void MakeMove_UpdatesBoardCorrectly()
        {
            _board.MakeMove(Board.PlayerState.Player1, 0);
            Assert.AreEqual(Board.PlayerState.Player1, _board.GetSpace(0, 0));
            Assert.AreEqual(1, _board.GetColumnHeight(0));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void MakeMove_FullColumn_ThrowsException()
        {
            for (int i = 0; i < Board.Height; i++)
            {
                _board.MakeMove(Board.PlayerState.Player1, 0);
            }
            _board.MakeMove(Board.PlayerState.Player1, 0);
        }

        [TestMethod]
        public void CheckVictory_HorizontalWin_ReturnsTrue()
        {
            for (int i = 0; i < Board.WinningLength; i++)
            {
                _board.MakeMove(Board.PlayerState.Player1, i);
            }
            Assert.IsTrue(_board.CheckVictory(Board.PlayerState.Player1, Board.WinningLength - 1));
        }

        [TestMethod]
        public void CheckVictory_VerticalWin_ReturnsTrue()
        {
            for (int i = 0; i < Board.WinningLength; i++)
            {
                _board.MakeMove(Board.PlayerState.Player1, 0);
            }
            Assert.IsTrue(_board.CheckVictory(Board.PlayerState.Player1, 0));
        }

        [TestMethod]
        public void CheckVictory_DiagonalWin_ReturnsTrue()
        {
            for (int i = 0; i < Board.WinningLength; i++)
            {
                for (int j = 0; j < i; j++)
                {
                    _board.MakeMove(Board.PlayerState.Player2, i);
                }
                _board.MakeMove(Board.PlayerState.Player1, i);
            }
            Assert.IsTrue(_board.CheckVictory(Board.PlayerState.Player1, Board.WinningLength - 1));
        }

        [TestMethod]
        public void CheckVictory_NoWin_ReturnsFalse()
        {
            _board.MakeMove(Board.PlayerState.Player1, 0);
            Assert.IsFalse(_board.CheckVictory(Board.PlayerState.Player1, 0));
        }

        [TestMethod]
        public void CheckVictory_WinInMiddle_ReturnsTrue()
        {
            int[][] setup =
            [
                [2, 1, 0, 0, 0, 0],
                [2, 1, 2, 1, 2, 0],
                [1, 2, 1, 2, 2, 2],
                [1, 2, 1, 1, 2, 0],
                [1, 1, 2, 1, 0, 0],
                [2, 1, 1, 0, 0, 0],
                [0, 0, 0, 0, 0, 0],
            ];

            for (int col = 0; col < Board.Width; col++)
            {
                for (int row = 0; row < setup[col].Length; row++)
                {
                    if (setup[col][row] != 0)
                    {
                        _board.MakeMove((Board.PlayerState)setup[col][row], col);
                    }
                }
            }

            // Make the winning move
            _board.MakeMove(Board.PlayerState.Player2, 4);

            Assert.IsTrue(_board.CheckVictory(Board.PlayerState.Player2, 4));
        }

        [TestMethod]
        public void IsFull_EmptyBoard_ReturnsFalse()
        {
            Assert.IsFalse(_board.IsFull());
        }

        [TestMethod]
        public void IsFull_FullBoard_ReturnsTrue()
        {
            for (int col = 0; col < Board.Width; col++)
            {
                for (int row = 0; row < Board.Height; row++)
                {
                    _board.MakeMove(Board.PlayerState.Player1, col);
                }
            }
            Assert.IsTrue(_board.IsFull());
        }

        [TestMethod]
        public void Clone_CreatesCopyWithSameState()
        {
            _board.MakeMove(Board.PlayerState.Player1, 0);
            _board.MakeMove(Board.PlayerState.Player2, 1);

            Board clonedBoard = _board.Clone();

            Assert.AreEqual(_board.GetSpace(0, 0), clonedBoard.GetSpace(0, 0));
            Assert.AreEqual(_board.GetSpace(0, 1), clonedBoard.GetSpace(0, 1));
            Assert.AreNotSame(_board, clonedBoard);
        }

        [TestMethod]
        public void ToString_ReturnsCorrectStringRepresentation()
        {
            _board.MakeMove(Board.PlayerState.Player1, 0);
            _board.MakeMove(Board.PlayerState.Player2, 1);

            string expected = "1200000" + "0000000" + "0000000" + "0000000" + "0000000" + "0000000";
            Assert.AreEqual(expected, _board.ToString());
        }
    }
}