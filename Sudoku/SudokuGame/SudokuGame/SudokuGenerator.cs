using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SudokuGame
{
    /// <summary>
    /// Sudoku puzzle generator depending on the selected difficulty level.
    /// </summary>
    public class SudokuGenerator
    {
        private readonly int[,] sudokuGrid;                                                     // A matrix 9x9 that represents the puzzle
        private static Random generator;                                                        // Random number generator

        public SudokuGenerator()
        {
            sudokuGrid = new int[9, 9];
            generator = new Random();
        }

        // A function that generates the puzzle.
        public void Generate(ref char[,] puzzle, int emptyCells)
        {
            FillBoardMainDiagonalSquares();

            // Update the puzzle that needs to be provided for solving.
            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    puzzle[row, col] = Convert.ToChar(sudokuGrid[row, col] + '0');
                }
            }

            // Solve the puzzle with the randomly generated main diagonal of 3x3 matrices.
            SudokuPuzzle.SolvePuzzle(ref puzzle);

            RemoveCharacters(ref puzzle, emptyCells);
        }
        
        // Fill in the 3x3 matrices on the main diagonal of the puzzle.
        private void FillBoardMainDiagonalSquares()
        {
            for (int i = 0; i < 9; i += 3)
            {
                FillSquare(i, i);
            }
        }

        // Fill in 3x3 matrix. The beginning of the matrix depends on the cell coordinates of the upper left corner.
        private void FillSquare(int upperLeftCornerRow, int upperLeftCornerCol)
        {
            int number = generator.Next(9) + 1;

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    while (!CheckNumInSquare(upperLeftCornerRow, upperLeftCornerCol, number))
                    {
                        number = generator.Next(9) + 1;
                    }

                    // If the number is not entered in the 3x3 matrix, it is entered.
                    sudokuGrid[upperLeftCornerRow + i, upperLeftCornerCol + j] = number;
                }
            }
        }

        // Check if a number is already written in the 3x3 matrix. If it is written, the function returns false. Otherwise, true.
        private bool CheckNumInSquare(int upperLeftCornerRow, int upperLeftCornerCol, int num)
        {
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (sudokuGrid[upperLeftCornerRow + i, upperLeftCornerCol + j] == num)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        // Remove a certain number of numbers (characters) from the puzzle.
        // This defines the level of difficulty. The more empty cells, the more reflection on the game.
        private void RemoveCharacters(ref char[,] puzzle, int emptyCells)
        {
            while (emptyCells != 0)
            {
                int indexOfBoardCell = generator.Next(81);

                int row = indexOfBoardCell / 9;
                int col = indexOfBoardCell % 9;

                if (puzzle[row, col] != '0')
                {
                    puzzle[row, col] = '0';
                    emptyCells--;
                }
            }
        }
    }
}
