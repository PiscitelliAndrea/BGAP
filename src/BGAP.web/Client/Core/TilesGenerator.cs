using System;
using System.Collections.Generic;
using System.Linq;

namespace BGAP.web.Client.Core
{
    public class TilesGenerator
    {
        #region Private Constants

        private const int MINNUMOFCOLUMNS = 3;
        private const int DEFAULTNUMOFCOLUMNS = 3;
        private const int MAXNUMOFCOLUMNS = 6;

        #endregion

        #region Private Properties

        private int NumOfColumns = 0;
        private int MaxNumOfTiles = 0;
        private List<NumberTile> TilesList = null;

        #endregion

        #region Public Properties

        public bool Done { get; set; }
        public int Moves { get; set; } = 0;

        #endregion

        #region Public Methods

        /// <summary>
        /// Generates all the tiles of the game.
        /// An empty tile is added in the last position
        /// </summary>
        /// <param name="numOfColumns"></param>
        /// <returns></returns>
        public List<NumberTile> GenerateTiles(int inputNumOfColumns = DEFAULTNUMOFCOLUMNS)
        {
            if (TilesList == null)
            {
                if (inputNumOfColumns <= MAXNUMOFCOLUMNS && inputNumOfColumns >= MINNUMOFCOLUMNS)
                    NumOfColumns = inputNumOfColumns;
                else
                    NumOfColumns = DEFAULTNUMOFCOLUMNS;

                MaxNumOfTiles = NumOfColumns * NumOfColumns;

                int row = 0;
                int column = 0;
                int nextNum = 0;

                Random rnd = new Random();
                TilesList = new List<NumberTile>();

                for (int index = 1; index < MaxNumOfTiles; index++)
                {
                    var item = new NumberTile
                    {
                        Row = row,
                        Column = column
                    };

                    do
                    {
                        nextNum = rnd.Next(1, MaxNumOfTiles);
                    }
                    while (TilesList.Where(n => n.NumberValue == nextNum.ToString()).Any());

                    item.SetNumber(nextNum);
                    item.BackgroundColor = ((nextNum % 2) == 0) ? "darkBackground" : "lightBackground";

                    TilesList.Add(item);

                    column += 1;
                    if (column >= NumOfColumns)
                    {
                        column = 0;
                        row += 1;
                    }
                }

                var emptyTile = new NumberTile();
                emptyTile.Row = emptyTile.Column = NumOfColumns - 1;
                emptyTile.ClearNumber();
                emptyTile.BackgroundColor = "blackBackground";

                TilesList.Add(emptyTile);
            }

            return GetAllTiles();
        }

        /// <summary>
        /// Checks if the selected tile (in the input coordinates) can be moved.
        /// If so, it is swapped with the empty tile
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public List<NumberTile> TryMoveTile(int row, int column)
        {
            // UP
            if (row > 0
                && TilesList.Where(n => (n.Row == (row - 1) && n.Column == column && n.NumberValue == "")).Any())
            {
                MoveTile(row, column, Direction.Up);
            }   // DOWN
            else if (row < NumOfColumns
                && TilesList.Where(n => (n.Row == (row + 1) && n.Column == column && n.NumberValue == "")).Any())
            {
                MoveTile(row, column, Direction.Down);
            }   // LEFT
            else if (column > 0
                && TilesList.Where(n => (n.Row == row && n.Column == (column - 1) && n.NumberValue == "")).Any())
            {
                MoveTile(row, column, Direction.Left);
            }   // RIGHT
            else if (column < NumOfColumns
                && TilesList.Where(n => (n.Row == row && n.Column == (column + 1) && n.NumberValue == "")).Any())
            {
                MoveTile(row, column, Direction.Right);
            }

            return GetAllTiles();
        }

        /// <summary>
        /// Restart the game
        /// </summary>
        /// <returns></returns>
        public List<NumberTile> Restart()
        {
            Done = false;
            TilesList = null;
            return GenerateTiles(NumOfColumns);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Sort the tiles according to thei coordinates, in order to be displayed
        /// and returns the entire list
        /// </summary>
        /// <returns></returns>
        private List<NumberTile> GetAllTiles()
        {
            List<NumberTile> SortedList = new List<NumberTile>();

            SortedList = TilesList.Where(t => t != null).OrderBy(n => n.Row).ThenBy(n => n.Column).ToList();

            return SortedList;
        }

        /// <summary>
        /// Move the tile currently in the position indicated by the coordinates
        /// in the specified direction
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <param name="direction"></param>
        private void MoveTile(int row, int column, Direction direction)
        {
            NumberTile tmpFrom = TilesList.Where(n => n.Row == row && n.Column == column).FirstOrDefault();
            NumberTile tmpTo = new NumberTile();
            NumberTile tmp = new NumberTile();

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
            string num = "";
            int counter = 1;

            for (int row = 0; row < NumOfColumns; row++)
            {
                for (int column = 0; column < NumOfColumns; column++)
                {
                    num = TilesList.Where(n => n.Row == row && n.Column == column).FirstOrDefault().NumberValue;
                    if (num != counter.ToString())
                        return false;
                    
                    counter++;
                    if (counter > (MaxNumOfTiles - 1))
                        counter = 0;
                }
            }

            return true;
        }

        /// <summary>
        /// Swap the number of the selected tile (From) with the empty one (To)
        /// </summary>
        /// <param name="From"></param>
        /// <param name="To"></param>
        private void SwapTiles(NumberTile From, NumberTile To)
        {
            string value = From.NumberValue;
            string background = From.BackgroundColor;
            From.ClearNumber();
            From.BackgroundColor = "blackBackground";
            To.SetNumber(Convert.ToInt32(value));
            To.BackgroundColor = background;

            Moves++;
        }

        #endregion
    }
}
