using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SudokuGame
{
    /// <summary>
    /// Record action when solving the puzzle.
    /// Information about the location of the cell, its old and new value is saved.
    /// </summary>
    public class CellRecord
    {
        public int Row { get; }
        public int Col { get; }
        public char OldCharacter { get; }
        public char NewCharacter { get; }

        public CellRecord(int row, int col, char oldCharacter, char newCharacter)
        {
            Row = row;
            Col = col;
            OldCharacter = oldCharacter;
            NewCharacter = newCharacter;
        }
    }
}
