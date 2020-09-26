using System;
using System.Collections.Generic;
using System.Linq;

namespace BGAP.web.Client.Core
{
    public class NumbersManager
    {
        #region Internal parameters

        private static readonly int MaxNumOfNumbers = 16;
        private static readonly int MaxNumOfRows = 4;
        private static readonly int MaxNumOfColumns = 4;
        private static readonly int ProbabilityToGetTwo = 80;

        #endregion

        #region Internal properties

        private List<NumberTile> NumbersList = new List<NumberTile>();

        #endregion

        #region Public properties

        public int Score = 0;
        public bool Moved = false;
        public bool GameOver = false;

        #endregion

        #region Initial Method

        public List<NumberTile> GenerateTwoInitialNumbers()
        {
            if (NumbersList == null || NumbersList.Count < 1)
            {
                NumbersList = new List<NumberTile>();

                Random rnd = new Random();

                var FirstItem = new NumberTile
                {
                    Row = rnd.Next(0, MaxNumOfRows - 1),
                    Column = rnd.Next(0, MaxNumOfColumns - 1),

                    Number = GetNewNumber(rnd)
                };
                FirstItem.BackgroundColor = GetBackgroundColor(FirstItem.Number);

                NumbersList.Add(FirstItem);

                var SecondItem = new NumberTile();

                do
                {
                    SecondItem.Row = rnd.Next(0, MaxNumOfRows - 1);
                    SecondItem.Column = rnd.Next(0, MaxNumOfColumns - 1);
                }
                while (SecondItem.Row == FirstItem.Row && SecondItem.Column == FirstItem.Column);

                SecondItem.Number = GetNewNumber(rnd);
                SecondItem.BackgroundColor = GetBackgroundColor(SecondItem.Number);

                NumbersList.Add(SecondItem);

                // Riempimento matrice
                int row = 0;
                int column = 0;
                int nextRow = 3;
                for (int index = 1; index <= MaxNumOfNumbers; index++)
                {
                    if (!((row == FirstItem.Row && column == FirstItem.Column) || (row == SecondItem.Row && column == SecondItem.Column)))
                    {
                        var item = new NumberTile
                        {
                            Row = row,
                            Column = column,
                            Number = 0
                        };
                        item.BackgroundColor = GetBackgroundColor(item.Number);

                        NumbersList.Add(item);
                    }

                    column += 1;
                    if (column > nextRow)
                    {
                        column = 0;
                        row += 1;
                    }
                }
            }

            return GetNumbers();
        }

        #endregion

        #region Public Methods

        public List<NumberTile> GenerateNewNumber()
        {
            Random rnd = new Random();
            int NewRow = -1;
            int NewColumn = -1;

            do
            {
                NewRow = rnd.Next(0, MaxNumOfRows);
                NewColumn = rnd.Next(0, MaxNumOfColumns);
            }
            while (NumbersList.Where(n => n.Row == NewRow && n.Column == NewColumn && n.Number != 0).Any());

            NumberTile NewNumber = NumbersList.Where(n => n.Row == NewRow && n.Column == NewColumn).FirstOrDefault();
            NewNumber.Number = GetNewNumber(rnd);
            NewNumber.BackgroundColor = GetBackgroundColor(NewNumber.Number);

            this.GameOver = CheckSolution();

            return GetNumbers();
        }

        private List<NumberTile> GetNumbers()
        {
            List<NumberTile> SortedList = new List<NumberTile>();

            SortedList = NumbersList.OrderBy(n => n.Row).ThenBy(n => n.Column).ToList();

            return SortedList;
        }

        public List<NumberTile> Restart()
        {
            GameOver = false;
            NumbersList = null;
            return GenerateTwoInitialNumbers();
        }

        #endregion

        #region Movement Methods

        public List<NumberTile> TryMoveNumber(Direction direzione)
        {
            bool squashed = false;
            this.Moved = false;

            // Movimenti alto-basso
            // Per ogni colonna
            if (direzione == Direction.Up || direzione == Direction.Down)
                for (int indexColumn = 0; indexColumn < MaxNumOfColumns; indexColumn++)
                {
                    this.Moved |= CompactColumn(indexColumn, direzione);
                    this.Moved |= MergeColumn(indexColumn, direzione, ref squashed);
                }

            // Movimenti destra-sinistra
            // Per ogni riga
            if (direzione == Direction.Left || direzione == Direction.Right)
                for (int indexRow = 0; indexRow < MaxNumOfRows; indexRow++)
                {
                    this.Moved |= CompactRow(indexRow, direzione);
                    this.Moved |= MergeRow(indexRow, direzione, ref squashed);
                }

            return GetNumbers();
        }

        #endregion

        #region Internal Methods

        private bool CompactColumn(int indexColumn, Direction direzione, int startingRow = 1)
        {
            bool moved = false;
            bool done = false;

            while (!done)
            {
                done = true;

                switch (direzione)
                {
                    case Direction.Up:
                        for (int indexRow = startingRow; indexRow < MaxNumOfRows; indexRow++)
                        {
                            // Confronto la cella corrente con quella precedente
                            NumberTile ThisNumber = NumbersList.Where(n => n.Row == indexRow && n.Column == indexColumn).First();
                            NumberTile PreviousNumber = NumbersList.Where(n => n.Row == indexRow - 1 && n.Column == indexColumn).First();

                            if (ThisNumber.Number != 0 && PreviousNumber.Number == 0)
                            {
                                for (int indexRowToSlide = indexRow; indexRowToSlide < MaxNumOfRows; indexRowToSlide++)
                                {
                                    MoveNumberByColumn(indexRowToSlide, indexColumn, direzione);
                                    moved = true;
                                    done = false;
                                }
                            }
                        }
                        break;

                    case Direction.Down:
                        for (int indexRow = MaxNumOfRows - 2; indexRow >= 0; indexRow--)
                        {
                            // Confronto la cella corrente con quella precedente
                            NumberTile ThisNumber = NumbersList.Where(n => n.Row == indexRow && n.Column == indexColumn).First();
                            NumberTile PreviousNumber = NumbersList.Where(n => n.Row == indexRow + 1 && n.Column == indexColumn).First();

                            if (ThisNumber.Number != 0 && PreviousNumber.Number == 0)
                            {
                                for (int indexRowToSlide = indexRow; indexRowToSlide >= 0; indexRowToSlide--)
                                {
                                    MoveNumberByColumn(indexRowToSlide, indexColumn, direzione);
                                    moved = true;
                                    done = false;
                                }
                            }
                        }
                        break;
                }
            }

            return moved;
        }

        private bool CompactRow(int indexRow, Direction direzione, int startingColumn = 1)
        {
            bool moved = false;
            bool done = false;

            while (!done)
            {
                done = true;

                switch (direzione)
                {
                    case Direction.Left:
                        for (int indexColumn = startingColumn; indexColumn < MaxNumOfColumns; indexColumn++)
                        {
                            // Confronto la cella corrente con quella precedente
                            NumberTile ThisNumber = NumbersList.Where(n => n.Row == indexRow && n.Column == indexColumn).First();
                            NumberTile PreviousNumber = NumbersList.Where(n => n.Row == indexRow && n.Column == indexColumn - 1).First();

                            if (ThisNumber.Number != 0 && PreviousNumber.Number == 0)
                            {
                                for (int indexColumnToSlide = indexColumn; indexColumnToSlide < MaxNumOfColumns; indexColumnToSlide++)
                                {
                                    MoveNumberByRow(indexRow, indexColumnToSlide, direzione);
                                    moved = true;
                                    done = false;
                                }
                            }
                        }
                        break;

                    case Direction.Right:
                        for (int indexColumn = MaxNumOfColumns - 2; indexColumn >= 0; indexColumn--)
                        {
                            // Confronto la cella corrente con quella precedente
                            NumberTile ThisNumber = NumbersList.Where(n => n.Row == indexRow && n.Column == indexColumn).First();
                            NumberTile PreviousNumber = NumbersList.Where(n => n.Row == indexRow && n.Column == indexColumn + 1).First();

                            if (ThisNumber.Number != 0 && PreviousNumber.Number == 0)
                            {
                                for (int indexColumnToSlide = indexColumn; indexColumnToSlide >= 0; indexColumnToSlide--)
                                {
                                    MoveNumberByRow(indexRow, indexColumnToSlide, direzione);
                                    moved = true;
                                    done = false;
                                }
                            }
                        }
                        break;
                }
            }

            return moved;
        }

        private bool MergeColumn(int indexColumn, Direction direction, ref bool squashed)
        {
            switch (direction)
            {
                case Direction.Up:
                    for (int indexRow = 1; indexRow < MaxNumOfRows; indexRow++)
                    {
                        // Confronto la cella corrente con quella precedente
                        NumberTile ThisNumber = NumbersList.Where(n => n.Row == indexRow && n.Column == indexColumn).First();
                        NumberTile PreviousNumber = NumbersList.Where(n => n.Row == indexRow - 1 && n.Column == indexColumn).First();

                        if (ThisNumber.Number != 0)
                        {
                            // Stesso numero -> Squash
                            if (ThisNumber.Number == PreviousNumber.Number)
                            {
                                SquashNumbers(indexRow, indexColumn, direction);
                                squashed = Moved = true;
                            }
                            else if (PreviousNumber.Number == 0)
                            {
                                MoveNumberByColumn(indexRow, indexColumn, direction);
                                if (!squashed)
                                    indexRow--;
                                else
                                    squashed = false;
                            }
                        }
                    }
                    break;

                case Direction.Down:
                    for (int indexRow = MaxNumOfRows - 2; indexRow >= 0; indexRow--)
                    {
                        // Confronto la cella corrente con quella precedente
                        NumberTile ThisNumber = NumbersList.Where(n => n.Row == indexRow && n.Column == indexColumn).First();
                        NumberTile PreviousNumber = NumbersList.Where(n => n.Row == indexRow + 1 && n.Column == indexColumn).First();

                        if (ThisNumber.Number != 0)
                        {
                            // Stesso numero -> Squash
                            if (ThisNumber.Number == PreviousNumber.Number)
                            {
                                SquashNumbers(indexRow, indexColumn, direction);
                                squashed = Moved = true;
                            }
                            else if (PreviousNumber.Number == 0)
                            {
                                MoveNumberByColumn(indexRow, indexColumn, direction);
                                if (!squashed)
                                    indexRow++;
                                else
                                    squashed = false;
                            }
                        }
                    }
                    break;
            }

            return Moved;
        }

        private bool MergeRow(int indexRow, Direction direction, ref bool squashed)
        {
            switch (direction)
            {
                case Direction.Left:
                    for (int indexColumn = 1; indexColumn < MaxNumOfRows; indexColumn++)
                    {
                        // Confronto la cella corrente con quella precedente
                        NumberTile ThisNumber = NumbersList.Where(n => n.Column == indexColumn && n.Row == indexRow).First();
                        NumberTile PreviousNumber = NumbersList.Where(n => n.Column == indexColumn - 1 && n.Row == indexRow).First();

                        if (ThisNumber.Number != 0)
                        {
                            // Stesso numero -> Squash
                            if (ThisNumber.Number == PreviousNumber.Number)
                            {
                                SquashNumbers(indexRow, indexColumn, direction);
                                squashed = Moved = true;
                            }
                            else if (PreviousNumber.Number == 0)
                            {
                                MoveNumberByRow(indexColumn, indexRow, direction);
                                if (!squashed)
                                    indexColumn--;
                                else
                                    squashed = false;
                            }
                        }
                    }
                    break;

                case Direction.Right:
                    for (int indexColumn = MaxNumOfColumns - 2; indexColumn >= 0; indexColumn--)
                    {
                        // Confronto la cella corrente con quella precedente
                        NumberTile ThisNumber = NumbersList.Where(n => n.Column == indexColumn && n.Row == indexRow).First();
                        NumberTile PreviousNumber = NumbersList.Where(n => n.Column == indexColumn + 1 && n.Row == indexRow).First();

                        if (ThisNumber.Number != 0)
                        {
                            // Stesso numero -> Squash
                            if (ThisNumber.Number == PreviousNumber.Number)
                            {
                                SquashNumbers(indexRow, indexColumn, direction);
                                squashed = Moved = true;
                            }
                            else if (PreviousNumber.Number == 0)
                            {
                                MoveNumberByRow(indexColumn, indexRow, direction);
                                if (!squashed)
                                    indexColumn--;
                                else
                                    squashed = false;
                            }
                        }
                    }
                    break;
            }

            return Moved;
        }

        private void SquashNumbers(int row, int column, Direction direction)
        {
            NumberTile tmpFrom = NumbersList.Where(n => n.Row == row && n.Column == column).FirstOrDefault();
            NumberTile tmpTo = new NumberTile();
            NumberTile tmp = new NumberTile();

            switch (direction)
            {
                case Direction.Up:
                    tmpTo = NumbersList.Where(n => n.Row == (row - 1) && n.Column == column).FirstOrDefault();
                    SquashNumbers(tmpFrom, tmpTo);
                    CompactColumn(column, direction, tmpFrom.Row);
                    break;

                case Direction.Down:
                    tmpTo = NumbersList.Where(n => n.Row == (row + 1) && n.Column == column).FirstOrDefault();
                    SquashNumbers(tmpFrom, tmpTo);
                    CompactColumn(column, direction, tmpFrom.Row);
                    break;

                case Direction.Left:
                    tmpTo = NumbersList.Where(n => n.Row == row && n.Column == (column - 1)).FirstOrDefault();
                    SquashNumbers(tmpFrom, tmpTo);
                    CompactRow(row, direction, tmpFrom.Column);
                    break;

                case Direction.Right:
                    tmpTo = NumbersList.Where(n => n.Row == row && n.Column == (column + 1)).FirstOrDefault();
                    SquashNumbers(tmpFrom, tmpTo);
                    CompactRow(row, direction, tmpFrom.Column);
                    break;
            }
        }

        private void MoveNumberByColumn(int row, int column, Direction direction)
        {
            NumberTile tmpFrom = NumbersList.Where(n => n.Row == row && n.Column == column).FirstOrDefault();
            NumberTile tmpTo = new NumberTile();
            NumberTile tmp = new NumberTile();

            switch (direction)
            {
                case Direction.Up:
                    tmpTo = NumbersList.Where(n => n.Row == (row - 1) && n.Column == column).FirstOrDefault();
                    break;

                case Direction.Down:
                    tmpTo = NumbersList.Where(n => n.Row == (row + 1) && n.Column == column).FirstOrDefault();
                    break;
            }

            MoveTiles(tmpFrom, tmpTo);
        }

        private void MoveNumberByRow(int row, int column, Direction direction)
        {
            NumberTile tmpFrom = NumbersList.Where(n => n.Row == row && n.Column == column).FirstOrDefault();
            NumberTile tmpTo = new NumberTile();
            NumberTile tmp = new NumberTile();

            switch (direction)
            {
                case Direction.Left:
                    tmpTo = NumbersList.Where(n => n.Row == row && n.Column == (column - 1)).FirstOrDefault();
                    break;

                case Direction.Right:
                    tmpTo = NumbersList.Where(n => n.Row == row && n.Column == (column + 1)).FirstOrDefault();
                    break;
            }

            MoveTiles(tmpFrom, tmpTo);
        }

        /// <summary>
        /// Checks is the puzzle has been successfully solved
        /// </summary>
        /// <returns></returns>
        private bool CheckSolution()
        {
            for (int row = 0; row < 4; row++)
            {
                for (int column = 0; column < 4; column++)
                {
                    if (NumbersList.Where(n => n.Row == row && n.Column == column).FirstOrDefault().Number == 0)
                        return false;
                }
            }

            return NoPossibleMoves();
        }

        private bool NoPossibleMoves()
        {
            // Horizontal Moves
            for (int row = 0; row < MaxNumOfRows; row++)
            {
                for (int column = 0; column < MaxNumOfColumns - 1; column++)
                {
                    int Num1 = NumbersList.Where(n => n.Row == row && n.Column == column).FirstOrDefault().Number;
                    int Num2 = NumbersList.Where(n => n.Row == row && n.Column == column + 1).FirstOrDefault().Number;

                    if (Num1 == Num2)
                        return false;
                }
            }

            // Vertical Moves
            for (int column = 0; column < MaxNumOfColumns; column++)
            {
                for (int row = 0; row < MaxNumOfRows - 1; row++)
                {
                    int Num1 = NumbersList.Where(n => n.Row == row && n.Column == column).FirstOrDefault().Number;
                    int Num2 = NumbersList.Where(n => n.Row == row + 1 && n.Column == column).FirstOrDefault().Number;

                    if (Num1 == Num2)
                        return false;
                }
            }

            return true;
        }

        private void SquashNumbers(NumberTile From, NumberTile Into)
        {
            Into.Number += From.Number;
            From.Number = 0;
            From.BackgroundColor = GetBackgroundColor(From.Number);
            Into.BackgroundColor = GetBackgroundColor(Into.Number);

            Score += Into.Number;
        }

        private void MoveTiles(NumberTile From, NumberTile To)
        {
            To.Number = From.Number;
            From.Number = 0;
            From.BackgroundColor = GetBackgroundColor(From.Number);
            To.BackgroundColor = GetBackgroundColor(To.Number);
        }

        private int GetNewNumber(Random rnd)
        {
            // Possibili valori: 2 e 4.
            // 80% probabilità per il 2, 20% per il 4.
            return rnd.Next(0, 100) < ProbabilityToGetTwo ? 2 : 4;
        }

        private string GetBackgroundColor(int value)
        {
            return "g2048BG_" + value.ToString();
        }

        #endregion
    }
}
