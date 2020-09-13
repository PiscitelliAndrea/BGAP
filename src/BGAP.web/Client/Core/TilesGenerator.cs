using System;
using System.Collections.Generic;
using System.Linq;
using static BGAP.web.Client.Core.Enums;

namespace BGAP.web.Client.Core
{
    public class TilesGenerator
    {
        #region Private Properties

        private int NumOfColumns = 0;
        private int MaxNumOfTiles = 0;
        private List<Tile> TilesList = null;

        #endregion

        #region Public Properties

        public bool Done { get; set; }

        #endregion

        #region Public Methods

        public List<Tile> GenerateTiles(int numOfColumns = 4)
        {
            NumOfColumns = numOfColumns;
            MaxNumOfTiles = NumOfColumns * NumOfColumns;

            int row = 0;
            int column = 0;
            //int nextRow = numOfColumns - 1;

            if (TilesList == null)
            {
                Random rnd = new Random();
                int nextNum = 0;
                TilesList = new List<Tile>();

                for (int index = 1; index < MaxNumOfTiles; index++)
                {
                    var item = new Tile();
                    item.Row = row;
                    item.Column = column;

                    do
                    {
                        nextNum = rnd.Next(1, MaxNumOfTiles);
                    }
                    while (TilesList.Where(n => n.number == nextNum).Any());

                    item.number = nextNum;
                    item.backgroundColor = ((nextNum % 2) == 0) ? "darkBackground" : "lightBackground";

                    TilesList.Add(item);

                    column += 1;
                    if (column >= numOfColumns)
                    {
                        column = 0;
                        row += 1;
                    }
                }

                var emptyTile = new Tile();
                emptyTile.Row = emptyTile.Column = numOfColumns - 1;
                emptyTile.number = 0;
                emptyTile.backgroundColor = "blackBackground";

                TilesList.Add(emptyTile);
            }

            return GetAllTiles();
        }

        public List<Tile> TryMoveTile(int row, int column)
        {
            // UP
            if (row > 0
                && TilesList.Where(n => (n.Row == (row - 1) && n.Column == column && n.number == 0)).Any())
            {
                MoveTile(row, column, Direction.Up);
            }   // DOWN
            else if (row < NumOfColumns
                && TilesList.Where(n => (n.Row == (row + 1) && n.Column == column && n.number == 0)).Any())
            {
                MoveTile(row, column, Direction.Down);
            }   // LEFT
            else if (column > 0
                && TilesList.Where(n => (n.Row == row && n.Column == (column - 1) && n.number == 0)).Any())
            {
                MoveTile(row, column, Direction.Left);
            }   // RIGHT
            else if (column < NumOfColumns
                && TilesList.Where(n => (n.Row == row && n.Column == (column + 1) && n.number == 0)).Any())
            {
                MoveTile(row, column, Direction.Right);
            }

            return GetAllTiles();
        }

        public List<Tile> Restart()
        {
            Done = false;
            TilesList = null;
            return GenerateTiles(NumOfColumns);
        }

        #endregion

        #region Private Methods

        private List<Tile> GetAllTiles()
        {
            List<Tile> SortedList = new List<Tile>();

            SortedList = TilesList.Where(t => t != null).OrderBy(n => n.Row).ThenBy(n => n.Column).ToList();

            return SortedList;
        }

        private void MoveTile(int row, int column, Direction direction)
        {
            Tile tmpFrom = TilesList.Where(n => n.Row == row && n.Column == column).FirstOrDefault();
            Tile tmpTo = new Tile();
            Tile tmp = new Tile();

            switch (direction)
            {
                case Direction.Up:
                    tmpTo = TilesList.Where(n => n.Row == (row - 1) && n.Column == column).FirstOrDefault();
                    break;

                case Direction.Down:
                    tmpTo = TilesList.Where(n => n.Row == (row + 1) && n.Column == column).FirstOrDefault();
                    break;

                case Direction.Left:
                    tmpTo = TilesList.Where(n => n.Row == row && n.Column == (column - 1)).FirstOrDefault();
                    break;

                case Direction.Right:
                    tmpTo = TilesList.Where(n => n.Row == row && n.Column == (column + 1)).FirstOrDefault();
                    break;
            }

            SwapTiles(tmpFrom, tmpTo);

            this.Done = CheckSolution();
        }

        /// <summary>
        /// Checks is the puzzle has been successfully solved
        /// </summary>
        /// <returns></returns>
        private bool CheckSolution()
        {
            int num = 1;
            int counter = 1;

            for (int row = 0; row < NumOfColumns; row++)
            {
                for (int column = 0; column < NumOfColumns; column++)
                {
                    num = TilesList.Where(n => n.Row == row && n.Column == column).FirstOrDefault().number;
                    if (num != counter++)
                        return false;

                    if (counter > (MaxNumOfTiles - 1))
                        counter = 0;
                }
            }

            return true;
        }

        private void SwapTiles(Tile From, Tile To)
        {
            int value = From.number;
            string background = From.backgroundColor;
            From.number = 0;
            From.backgroundColor = "blackBackground";
            To.number = value;
            To.backgroundColor = background;
        }

        #endregion
    }
}
