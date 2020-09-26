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

        private List<Number> NumbersList = new List<Number>();

        #endregion

        #region Public properties

        public int Score = 0;
        public bool Moved = false;
        public bool GameOver = false;

        #endregion

        #region Initial Method

        public List<Number> GenerateTwoInitialNumbers()
        {
            if (NumbersList == null || NumbersList.Count < 1)
            {
                NumbersList = new List<Number>();

                Random rnd = new Random();

                var FirstItem = new Number
                {
                    Row = rnd.Next(0, MaxNumOfRows - 1),
                    Column = rnd.Next(0, MaxNumOfColumns - 1),

                    number = GetNewNumber(rnd)
                };
                FirstItem.backgroundColor = GetBackgroundColor(FirstItem.number);

                NumbersList.Add(FirstItem);

                var SecondItem = new Number();

                do
                {
                    SecondItem.Row = rnd.Next(0, MaxNumOfRows - 1);
                    SecondItem.Column = rnd.Next(0, MaxNumOfColumns - 1);
                }
                while (SecondItem.Row == FirstItem.Row && SecondItem.Column == FirstItem.Column);

                SecondItem.number = GetNewNumber(rnd);
                SecondItem.backgroundColor = GetBackgroundColor(SecondItem.number);

                NumbersList.Add(SecondItem);

                // Riempimento matrice
                int row = 0;
                int column = 0;
                int nextRow = 3;
                for (int index = 1; index <= MaxNumOfNumbers; index++)
                {
                    if (!((row == FirstItem.Row && column == FirstItem.Column) || (row == SecondItem.Row && column == SecondItem.Column)))
                    {
                        var item = new Number
                        {
                            Row = row,
                            Column = column,
                            number = 0
                        };
                        item.backgroundColor = GetBackgroundColor(item.number);

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

        public List<Number> GenerateNewNumber()
        {
            Random rnd = new Random();
            int NewRow = -1;
            int NewColumn = -1;

            do
            {
                NewRow = rnd.Next(0, MaxNumOfRows);
                NewColumn = rnd.Next(0, MaxNumOfColumns);
            }
            while (NumbersList.Where(n => n.Row == NewRow && n.Column == NewColumn && n.number != 0).Any());

            Number NewNumber = NumbersList.Where(n => n.Row == NewRow && n.Column == NewColumn).FirstOrDefault();
            NewNumber.number = GetNewNumber(rnd);
            NewNumber.backgroundColor = GetBackgroundColor(NewNumber.number);

            this.GameOver = CheckSolution();

            return GetNumbers();
        }

        private List<Number> GetNumbers()
        {
            List<Number> SortedList = new List<Number>();

            SortedList = NumbersList.OrderBy(n => n.Row).ThenBy(n => n.Column).ToList();

            return SortedList;
        }

        public List<Number> Restart()
        {
            GameOver = false;
            NumbersList = null;
            return GenerateTwoInitialNumbers();
        }

        #endregion

        #region Movement Methods

        public List<Number> TryMoveNumber(Direction direzione)
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
                            Number ThisNumber = NumbersList.Where(n => n.Row == indexRow && n.Column == indexColumn).First();
                            Number PreviousNumber = NumbersList.Where(n => n.Row == indexRow - 1 && n.Column == indexColumn).First();

                            if (ThisNumber.number != 0 && PreviousNumber.number == 0)
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
                            Number ThisNumber = NumbersList.Where(n => n.Row == indexRow && n.Column == indexColumn).First();
                            Number PreviousNumber = NumbersList.Where(n => n.Row == indexRow + 1 && n.Column == indexColumn).First();

                            if (ThisNumber.number != 0 && PreviousNumber.number == 0)
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
                            Number ThisNumber = NumbersList.Where(n => n.Row == indexRow && n.Column == indexColumn).First();
                            Number PreviousNumber = NumbersList.Where(n => n.Row == indexRow && n.Column == indexColumn - 1).First();

                            if (ThisNumber.number != 0 && PreviousNumber.number == 0)
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
                            Number ThisNumber = NumbersList.Where(n => n.Row == indexRow && n.Column == indexColumn).First();
                            Number PreviousNumber = NumbersList.Where(n => n.Row == indexRow && n.Column == indexColumn + 1).First();

                            if (ThisNumber.number != 0 && PreviousNumber.number == 0)
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
                        Number ThisNumber = NumbersList.Where(n => n.Row == indexRow && n.Column == indexColumn).First();
                        Number PreviousNumber = NumbersList.Where(n => n.Row == indexRow - 1 && n.Column == indexColumn).First();

                        if (ThisNumber.number != 0)
                        {
                            // Stesso numero -> Squash
                            if (ThisNumber.number == PreviousNumber.number)
                            {
                                SquashNumbers(indexRow, indexColumn, direction);
                                squashed = Moved = true;
                            }
                            else if (PreviousNumber.number == 0)
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
                        Number ThisNumber = NumbersList.Where(n => n.Row == indexRow && n.Column == indexColumn).First();
                        Number PreviousNumber = NumbersList.Where(n => n.Row == indexRow + 1 && n.Column == indexColumn).First();

                        if (ThisNumber.number != 0)
                        {
                            // Stesso numero -> Squash
                            if (ThisNumber.number == PreviousNumber.number)
                            {
                                SquashNumbers(indexRow, indexColumn, direction);
                                squashed = Moved = true;
                            }
                            else if (PreviousNumber.number == 0)
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
                        Number ThisNumber = NumbersList.Where(n => n.Column == indexColumn && n.Row == indexRow).First();
                        Number PreviousNumber = NumbersList.Where(n => n.Column == indexColumn - 1 && n.Row == indexRow).First();

                        if (ThisNumber.number != 0)
                        {
                            // Stesso numero -> Squash
                            if (ThisNumber.number == PreviousNumber.number)
                            {
                                SquashNumbers(indexRow, indexColumn, direction);
                                squashed = Moved = true;
                            }
                            else if (PreviousNumber.number == 0)
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
                        Number ThisNumber = NumbersList.Where(n => n.Column == indexColumn && n.Row == indexRow).First();
                        Number PreviousNumber = NumbersList.Where(n => n.Column == indexColumn + 1 && n.Row == indexRow).First();

                        if (ThisNumber.number != 0)
                        {
                            // Stesso numero -> Squash
                            if (ThisNumber.number == PreviousNumber.number)
                            {
                                SquashNumbers(indexRow, indexColumn, direction);
                                squashed = Moved = true;
                            }
                            else if (PreviousNumber.number == 0)
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
            Number tmpFrom = NumbersList.Where(n => n.Row == row && n.Column == column).FirstOrDefault();
            Number tmpTo = new Number();
            Number tmp = new Number();

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
            Number tmpFrom = NumbersList.Where(n => n.Row == row && n.Column == column).FirstOrDefault();
            Number tmpTo = new Number();
            Number tmp = new Number();

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
            Number tmpFrom = NumbersList.Where(n => n.Row == row && n.Column == column).FirstOrDefault();
            Number tmpTo = new Number();
            Number tmp = new Number();

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
                    if (NumbersList.Where(n => n.Row == row && n.Column == column).FirstOrDefault().number == 0)
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
                    int Num1 = NumbersList.Where(n => n.Row == row && n.Column == column).FirstOrDefault().number;
                    int Num2 = NumbersList.Where(n => n.Row == row && n.Column == column + 1).FirstOrDefault().number;

                    if (Num1 == Num2)
                        return false;
                }
            }

            // Vertical Moves
            for (int column = 0; column < MaxNumOfColumns; column++)
            {
                for (int row = 0; row < MaxNumOfRows - 1; row++)
                {
                    int Num1 = NumbersList.Where(n => n.Row == row && n.Column == column).FirstOrDefault().number;
                    int Num2 = NumbersList.Where(n => n.Row == row + 1 && n.Column == column).FirstOrDefault().number;

                    if (Num1 == Num2)
                        return false;
                }
            }

            return true;
        }

        private void SquashNumbers(Number From, Number Into)
        {
            Into.number += From.number;
            From.number = 0;
            From.backgroundColor = GetBackgroundColor(From.number);
            Into.backgroundColor = GetBackgroundColor(Into.number);

            Score += Into.number;
        }

        private void MoveTiles(Number From, Number To)
        {
            To.number = From.number;
            From.number = 0;
            From.backgroundColor = GetBackgroundColor(From.number);
            To.backgroundColor = GetBackgroundColor(To.number);
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
