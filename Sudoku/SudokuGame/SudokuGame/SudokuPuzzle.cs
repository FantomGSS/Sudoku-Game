using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SudokuGame
{
    /// <summary>
    /// Functionality for validating and solving a sudoku puzzle.
    /// Implemented verification that the puzzle is entirely completed.
    /// </summary>
    public class SudokuPuzzle
    {
        // Check if the puzzle has empty cells.
        public static bool PuzzleIsComplete(char[,] puzzle)
        {
            bool result = true;

            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    if (puzzle[row, col] == '0')
                    {
                        result = false;
                        break;
                    }
                }
            }

            return result;
        }

        // Check if the puzzle is valid.
        public static bool CheckPuzzle(char[,] puzzle)
        {
            bool rowsAreValid = Validate((int row, int col) => puzzle[row, col] - '0');         // Verify that the rows are valid
            bool colsAreValid = Validate((int row, int col) => puzzle[col, row] - '0');         // Verify that the columns are valid

            // Verify that the 3x3 matrices are valid.
            bool squaresAreValid = Validate((int square, int position) =>
            {
                int col = 3 * (square % 3) + position % 3;
                int row = position / 3 + 3 * (square / 3);
                return puzzle[row, col] - '0';
            });

            // All rules must be followed.
            // If even one of the above checks fails, the function returns false.
            return rowsAreValid && colsAreValid && squaresAreValid;
        }

        // Auxiliary function for traversing the 9x9 matrix in order to check whether the rules are followed.
        private static bool Validate(Func<int, int, int> getNumberFunc)
        {
            // An array showing which numbers have already been used.
            var uniqueNumbers = new bool[10];

            for (var row = 0; row < 9; row++)
            {
                for (var col = 0; col < 9; col++)
                {
                    var number = getNumberFunc(row, col);

                    if (uniqueNumbers[number] && number != 0)
                    {
                        return false;
                    }

                    // The use of the number is noted. The number 0, which represents
                    // the empty cell, is also noted, but it has no effect.
                    uniqueNumbers[number] = true;
                }

                uniqueNumbers = new bool[10];
            }

            return true;
        }

        // Solve the puzzle.
        public static void SolvePuzzle(ref char[,] puzzle)
        {
            SolvePuzzleHelper(ref puzzle);
        }

        // Auxiliary function for solving the puzzle in order to provide recursion.
        // The function is performed until the entire 9x9 matrix is filled correctly.
        private static bool SolvePuzzleHelper(ref char[,] puzzle)
        {
            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    if (puzzle[row, col] == '0')
                    {
                        for (char character = '1'; character <= '9'; character++)
                        {
                            if (isValidNote(puzzle, row, col, character))
                            {
                                // If all is well, the empty cell is filled and the function recursively continues by filling the others.
                                puzzle[row, col] = character;

                                if (SolvePuzzleHelper(ref puzzle))
                                {
                                    return true;
                                }

                                // If a problem occurs, the cell in which the value is stored must be cleared.
                                puzzle[row, col] = '0';
                            }
                        }

                        return false;
                    }
                }
            }

            return true;
        }

        // Check if a number can be placed in a cell without breaking any rules of the game.
        private static bool isValidNote(char[,] puzzle, int row, int col, char character)
        {
            for (int i = 0; i < 9; i++)
            {
                // Check if the number belongs to the row.
                if (puzzle[i, col] == character && puzzle[i, col] != '0')
                {
                    return false;
                }

                // Check if the number belongs to the column.
                if (puzzle[row, i] == character && puzzle[row, i] != '0')
                {
                    return false;
                }

                // Check if the number belongs to a 3x3 matrix.
                if (puzzle[3 * (row / 3) + i / 3, 3 * (col / 3) + i % 3] != '0'
                    && puzzle[3 * (row / 3) + i / 3, 3 * (col / 3) + i % 3] == character)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
