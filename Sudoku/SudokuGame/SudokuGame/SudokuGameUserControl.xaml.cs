using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.Win32;

namespace SudokuGame
{
    /// <summary>
    /// Interaction logic for SudokuGameUserControl.xaml
    /// A User Control (WPF) for representing the interactive GUI of the game.
    /// </summary>
    public partial class SudokuGameUserControl : UserControl
    {
        private readonly DispatcherTimer dispatcherTimer;
        private readonly Stopwatch stopWatch;

        private TextBox txtBoardCell;                                                           // A text box with which we will enter values on the board
        private char[,] initialBoard;                                                           // The puzzle at the beginning without any changes by the user
        private char[,] board;                                                                  // The puzzle the user thinks about

        private List<CellRecord> archive;                                                       // A list that stores changes made to the puzzle
        private int archivePosition;                                                            // A position of the last change

        public SudokuGameUserControl()
        {
            InitializeComponent();

            // Initialization of the chronometer
            dispatcherTimer = new DispatcherTimer();
            stopWatch = new Stopwatch();
            dispatcherTimer.Tick += Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 1, 0);

            WaitingPuzzle();
        }

        // Help the user to start a game.
        private void CreateToolTipBtnStartGame()
        {
            List<string> difficultyLevelsList = new List<string>();
            foreach (MenuItem item in btnDifficultyLevel.Items)
            {
                difficultyLevelsList.Add(item.Header.ToString());
            }

            btnStartGame.ToolTip = "To start a game, you need to load a puzzle file or generate one" +
                                   $"\nusing the provided difficulty levels: {string.Join(" | ", difficultyLevelsList)}";
        }

        // Initialize private fields and prepare an appropriate view of the application.
        private void WaitingPuzzle()
        {
            txtBoardCell = null;
            initialBoard = new char[9, 9];
            board = new char[9, 9];

            archive = new List<CellRecord>();
            archivePosition = 0;

            CreateToolTipBtnStartGame();
            grdBoard.IsEnabled = false;
            btnStartGame.IsEnabled = false;
            btnClearBoard.IsEnabled = false;
            btnStartChronometer.IsEnabled = false;
            btnStopChronometer.IsEnabled = false;
            lblChronometer.IsEnabled = false;
            btnCheckPuzzle.IsEnabled = false;
            btnSolvePuzzle.IsEnabled = false;

            btnSave.IsEnabled = false;
            btnUndo.IsEnabled = false;
            btnRedo.IsEnabled = false;
        }

        // Chronometer Event handler in order to display the elapsed seconds after starting the game.
        private void Tick(object sender, EventArgs e)
        {
            if (stopWatch.IsRunning)
            {
                TimeSpan timeSpan = stopWatch.Elapsed;
                lblChronometer.Content = $"{timeSpan.Hours:00}:{timeSpan.Minutes:00}:{timeSpan.Seconds:00}";
            }
        }

        // Start the chronometer.
        private void BtnStartChronometer_OnClick(object sender, RoutedEventArgs e)
        {
            stopWatch.Start();
            dispatcherTimer.Start();

            btnStartChronometer.IsEnabled = false;
            btnStopChronometer.IsEnabled = true;
        }

        // Stop the chronometer.
        private void BtnStopChronometer_OnClick(object sender, RoutedEventArgs e)
        {
            stopWatch.Stop();

            btnStartChronometer.IsEnabled = true;
            btnStopChronometer.IsEnabled = false;
        }

        // Get the currently selected cell in the puzzle.
        private void TxtBoardCell_OnLostFocus(object sender, RoutedEventArgs e)
        {
            txtBoardCell = e.Source as TextBox;
        }

        // Fill in or erase a cell in the puzzle and enter an entry in the list of changes.
        private void BtnNumberInputOrErase_OnClick(object sender, RoutedEventArgs e)
        {
            if (txtBoardCell == null || grdBoard.IsEnabled == false)
            {
                return;
            }
            
            if (txtBoardCell.Text != string.Empty && txtBoardCell.Foreground.ToString() != "#FF1482FA")
            {
                return;
            }

            string content = ((Button)sender).Content.ToString();
            if (content == "Erase")
            {
                content = "0";
            }

            int[] txtBoardCellCoordinates = FindCoordinatesOfCell();
            int cellRow = txtBoardCellCoordinates[0];
            int cellCol = txtBoardCellCoordinates[1];
            CellRecord cellRecord = new CellRecord(cellRow, cellCol, board[cellRow, cellCol], Convert.ToChar(content));

            if (archivePosition < archive.Count - 1)
            {
                archive.RemoveRange(archivePosition + 1, archive.Count - archivePosition - 1);
                btnRedo.IsEnabled = false;
            }

            archive.Add(cellRecord);
            archivePosition = archive.Count - 1;
            btnUndo.IsEnabled = true;

            board[cellRow, cellCol] = Convert.ToChar(content);
            txtBoardCell.Text = content == "0" ? string.Empty : content;
        }

        // Load a file that stores a puzzle. Different puzzle inputs are supported.
        // Messages for invalid file or invalid content.
        private void btnOpen_OnClick(object sender, RoutedEventArgs e)
        {      
            bool? result;
            string fileName;

            StreamReader fileReader;
            OpenFileDialog fileChooser = new OpenFileDialog();

            result = fileChooser.ShowDialog();
            fileName = fileChooser.FileName;
            
            if (result.HasValue)
            {
                if (fileName == string.Empty)
                {
                    MessageBox.Show("No file received!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    try
                    {
                        FileStream input = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                        fileReader = new StreamReader(input);

                        List<char> cells = new List<char>();
                        while (!fileReader.EndOfStream)
                        {
                            string line = fileReader.ReadLine();

                            foreach (char character in line)
                            {
                                if (char.IsDigit(character))
                                {
                                    cells.Add(character);
                                }
                                else if (character == '.')
                                {
                                    cells.Add('0');
                                }
                            }
                        }

                        if (cells.Count != 9 * 9)
                        {
                            MessageBox.Show("The file must consist of 81 characters.\nEmpty cells: {. 0} and Numbers: {1 2 3 4 5 6 7 8 9}.",
                                "Invalid content", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                            return;
                        }

                        int row = 0;
                        int column;
                        for (int i = 0; i < cells.Count; i++)
                        {
                            column = i % 9;
                            board[row, column] = cells[i];

                            if (column == 8)
                            {
                                row++;
                            }
                        }

                        if (!SudokuPuzzle.CheckPuzzle(board))
                        {
                            MessageBox.Show("The puzzle is filled with incorrect data! Please fix the file and try again.",
                                "Incorrect data", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                            board = new char[9, 9];
                            return;
                        }

                        fileReader?.Close();

                        copyInitialBoard();

                        PresentBoard();
                        ChangeTextColorEmptyCells();

                        PuzzleIsChosen();
                    }
                    catch (IOException)
                    {
                        MessageBox.Show("Error reading from file!", "File Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        // Save a copy of the puzzle at the beginning of the game.
        private void copyInitialBoard()
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    initialBoard[i, j] = board[i, j];
                }
            }
        }

        // Save a puzzle to a file.
        // Error and successful operation messages.
        private void btnSave_OnClick(object sender, RoutedEventArgs e)
        {
            bool? result;
            string fileName;

            StreamWriter fileWriter;
            SaveFileDialog fileChooser = new SaveFileDialog
            {
                CheckFileExists = false
            };

            result = fileChooser.ShowDialog();
            fileName = fileChooser?.FileName;

            if (result.HasValue)
            {
                if (fileName == string.Empty)
                {
                    MessageBox.Show("No file specified!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    try
                    {   
                        FileStream output = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write);
                        fileWriter = new StreamWriter(output);

                        StringBuilder stringBuilderBoard = new StringBuilder();
                        for (int i = 0; i < 9; i++)
                        {
                            for (int j = 0; j < 9; j++)
                            {
                                stringBuilderBoard.Append(board[i, j]);
                            }

                            stringBuilderBoard.Append('\n');
                        }

                        fileWriter.Write(stringBuilderBoard);

                        fileWriter?.Close();

                        MessageBox.Show("File saved successfully!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (IOException)
                    {
                        MessageBox.Show("Error saving file!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        // Exit the application.
        private void btnExit_OnClick(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        // Check if the rules of the game are followed.
        private void BtnCheckPuzzle_OnClick(object sender, RoutedEventArgs e)
        {
            if (SudokuPuzzle.PuzzleIsComplete(board) && SudokuPuzzle.CheckPuzzle(board))
            {
                PuzzleIsSolved();

                MessageBox.Show("Congratulations! You managed to solve the puzzle!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (SudokuPuzzle.CheckPuzzle(board))
            {
                MessageBox.Show("No errors found.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("An error has occurred!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // Solve the puzzle.
        private void BtnSolvePuzzle_OnClick(object sender, RoutedEventArgs e)
        {
            SudokuPuzzle.SolvePuzzle(ref initialBoard);

            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    board[i, j] = initialBoard[i, j];
                }
            }

            PuzzleIsSolved();

            PresentBoard();
        }

        // Refresh the board.
        private void PresentBoard()
        {
            int row = 0;
            int column = 0;
            foreach (UIElement element in grdBoard.Children)
            {
                if (element is TextBox)
                {
                    ((TextBox)element).Text = board[row, column] == '0' ? string.Empty : board[row, column].ToString();

                    column++;
                    if (column == 9)
                    {
                        row++;
                        column = 0;
                    }
                }
            }
        }

        // These numbers that the user have to enter in the empty cells will be displayed in blue.
        private void ChangeTextColorEmptyCells()
        {
            int row = 0;
            int column = 0;
            foreach (UIElement element in grdBoard.Children)
            {
                if (element is TextBox)
                {
                    if (board[row, column] == '0')
                    {
                        ((TextBox)element).Foreground = new SolidColorBrush(Color.FromRgb(20, 130, 250));
                    }

                    column++;
                    if (column == 9)
                    {
                        row++;
                        column = 0;
                    }
                }
            }
        }

        // Find the coordinates of a cell on the board.
        private int[] FindCoordinatesOfCell()
        {
            int counter = 0;
            foreach (UIElement element in grdBoard.Children)
            {
                if (element is TextBox)
                {
                    if (((TextBox)element).Equals(txtBoardCell))
                    {
                        break;
                    }

                    counter++;
                }
            }

            // [row, col]
            return new int[]{counter / 9, counter % 9};
        }

        // Go one step back and update the position of the last change.
        private void btnUndo_OnClick(object sender, RoutedEventArgs e)
        {
            CellRecord cellRecord = archive[archivePosition];
            archivePosition--;

            if (archivePosition == -1)
            {
                btnUndo.IsEnabled = false;
            }

            int row = cellRecord.Row;
            int col = cellRecord.Col;
            char oldCharacter = cellRecord.OldCharacter;

            btnRedo.IsEnabled = true;
            board[row, col] = oldCharacter;
            PresentBoard();
        }

        // Go one step forward and update the position of the last change.
        private void btnRedo_OnClick(object sender, RoutedEventArgs e)
        {
            archivePosition++;
            if (archivePosition == archive.Count - 1)
            {
                btnRedo.IsEnabled = false;
            }

            CellRecord cellRecord = archive[archivePosition];
            int row = cellRecord.Row;
            int col = cellRecord.Col;
            char newCharacter = cellRecord.NewCharacter;

            btnUndo.IsEnabled = true;
            board[row, col] = newCharacter;
            PresentBoard();
        }

        // Start the game, start the chronometer and unlock the board.
        // The buttons for checking and solving the puzzle are unlocked.
        private void BtnStartGame_OnClick(object sender, RoutedEventArgs e)
        {
            btnStartGame.IsEnabled = false;

            grdBoard.IsEnabled = true;

            stopWatch.Start();
            dispatcherTimer.Start();
            btnStopChronometer.IsEnabled = true;
            lblChronometer.IsEnabled = true;
            btnCheckPuzzle.IsEnabled = true;
            btnSolvePuzzle.IsEnabled = true;
        }

        // Restart the game.
        private void BtnClearBoard_OnClick(object sender, RoutedEventArgs e)
        {
            stopWatch.Reset();
            lblChronometer.Content = "00:00:00";

            WaitingPuzzle();

            btnOpen.IsEnabled = true;
            mnuItemEasy.IsEnabled = true;
            mnuItemNormal.IsEnabled = true;
            mnuItemHard.IsEnabled = true;

            mnuItemEasy.IsChecked = false;
            mnuItemNormal.IsChecked = false;
            mnuItemHard.IsChecked = false;

            foreach (UIElement element in grdBoard.Children)
            {
                if (element is TextBox)
                {
                    ((TextBox)element).Text = string.Empty;
                    ((TextBox)element).ClearValue(ForegroundProperty);
                }
            }
        }

        // Help the user to understand the game.
        private void BtnAbout_OnClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("This is the popular Japanese game SUDOKU. " +
                            "Sudoku puzzles are 9 x 9 grids, and each square in the grid consists of a 3 x 3 subgrid called a minigrid. " +
                            "Your goal is to fill in the squares so that each column, row, and minigrid contains the numbers 1 through 9 exactly once.",
                "About", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // Generate an easy puzzle.
        private void MnuItemEasy_OnClick(object sender, RoutedEventArgs e)
        {
            PuzzleIsChosen();

            SudokuGenerator sudokuGenerator = new SudokuGenerator();
            sudokuGenerator.Generate(ref board, (int)DifficultyLevel.EASY);

            copyInitialBoard();

            PresentBoard();
            ChangeTextColorEmptyCells();
        }

        // Generate a normal puzzle.
        private void MnuItemNormal_OnClick(object sender, RoutedEventArgs e)
        {
            PuzzleIsChosen();

            SudokuGenerator sudokuGenerator = new SudokuGenerator();
            sudokuGenerator.Generate(ref board, (int)DifficultyLevel.NORMAL);

            copyInitialBoard();

            PresentBoard();
            ChangeTextColorEmptyCells();
        }

        // Generate a hard puzzle.
        private void MnuItemHard_OnClick(object sender, RoutedEventArgs e)
        {
            PuzzleIsChosen();

            SudokuGenerator sudokuGenerator = new SudokuGenerator();
            sudokuGenerator.Generate(ref board, (int)DifficultyLevel.HARD);

            copyInitialBoard();

            PresentBoard();
            ChangeTextColorEmptyCells();
        }

        // Once the puzzle is solved, the board is deactivated and no more changes can be made.
        private void PuzzleIsSolved()
        {
            grdBoard.IsEnabled = false;

            btnUndo.IsEnabled = false;
            btnRedo.IsEnabled = false;
            btnStartChronometer.IsEnabled = false;
            btnStopChronometer.IsEnabled = false;
            btnSolvePuzzle.IsEnabled = false;
            stopWatch.Stop();
        }

        // Once the puzzle is presented on the board, the game can be started or the board cleared if necessary.
        // The buttons associated with a new puzzle are deactivated.
        private void PuzzleIsChosen()
        {
            btnStartGame.IsEnabled = true;
            btnStartGame.ClearValue(ToolTipProperty);
            btnClearBoard.IsEnabled = true;

            btnOpen.IsEnabled = false;
            btnSave.IsEnabled = true;

            mnuItemEasy.IsEnabled = false;
            mnuItemNormal.IsEnabled = false;
            mnuItemHard.IsEnabled = false;
        }
    }
}
