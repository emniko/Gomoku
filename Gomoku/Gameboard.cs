using System;
using System.Collections.Generic;

namespace Gomoku
{
    //This is the most significant class in the game which handles both game and board validations.
    class Gameboard
    {
        //Declaring neccessary variables
        public bool crossTurn;
        public bool crossIsHuman = false;
        public bool circleIsHuman = true;
        public static bool extensiveBoard = false;
        private Computer bot = new Computer();
        public int previousMove = -1;
        public int size;
        public int[] board;
        public int lastSquare;

        //This function is to initiaze the gameboard with given parameters
        public Gameboard(int size, bool crossIsHuman, bool circleIsHuman)
        {
            this.size = size;
            board = new int[size * size];
            this.crossIsHuman = crossIsHuman;
            this.circleIsHuman = circleIsHuman;

            //Cross/Black piece always has the first turn
            crossTurn = true;
        }

        //Another constructor to copy the gameboard
        public Gameboard(Gameboard toCopy)
        {
            size = toCopy.size;
            board = (int[])toCopy.board.Clone();
            crossTurn = toCopy.crossTurn;
            lastSquare = toCopy.lastSquare;
        }

        //This function is used to load the moves into the board before starting the game.
        public void LoadMoves() 
        {
            foreach (Move move in LocalStorage.moves) 
            {
                board[move.position] = (move.isCross) ? 1 : 2;
            }

            crossTurn = LocalStorage.moves[LocalStorage.moves.Count - 1].isCross ? false : true;
        }

        //This function proceed each turn in loop till the game ends.
        public void StartIteration()
        {
            lastSquare = (size * size) / 2;
            while (true)
            {
                DrawGameboard(lastSquare);
                //Human turn
                if ((crossTurn && crossIsHuman) || (!crossTurn && circleIsHuman))
                {
                    lastSquare = HumanSelectSquare();
                }
                //Computer turn
                else
                {
                    lastSquare = bot.ComputerSelectSquare(this, 4, 8);
                }
                //End Turn Condition
                if (board[lastSquare] == 0 || board[lastSquare] == 3)
                {
                    SwitchSquare(lastSquare);
                    bot.AssessGameboard(this);
                    int win = EvaluateWin(lastSquare);
                    if (win != 0)
                    {
                        DrawGameboard(lastSquare);
                        DisplayEndingText(win);
                        LocalStorage.moves.Clear();
                        LocalStorage.ClearMoves();
                        TakeInput();
                    }
                    //Saving moves
                    LocalStorage.moves.Add(new Move(crossTurn, previousMove));
                    SwitchTurn();
                }
            }
        }

        //This function is used to display the gameboard with cursor position
        public void DrawGameboard(int cursorPos = 0)
        {
            ClearGameboard();
            string colLabels = "    ";
            for (int i = 0; i < size; i++)
            {
                colLabels += (char)('a' + i);
                if (extensiveBoard)
                {
                    colLabels += " ";
                }
            }
            Console.WriteLine(colLabels);
            for (int i = 0; i < size; i++)
            {
                Console.WriteLine();
                Console.Write((i + 1).ToString("00") + "  ");
                for (int j = 0; j < size; j++)
                {
                    if (board[i * size + j] == 0)
                    {
                        Console.ResetColor();
                        Console.Write("-");
                    }
                    else if (board[i * size + j] == 1)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("X");
                    }
                    else if (board[i * size + j] == 2)
                    {
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.Write("O");
                    }
                    else
                    {
                        Console.Write("T");
                    }
                    if (extensiveBoard)
                    {
                        Console.Write(" ");
                    }
                }
            }
            Console.WriteLine();
            Console.WriteLine();
            if (extensiveBoard)
            {
                Console.SetCursorPosition(4 + ColFromPos(cursorPos) * 2,
                                         2 + RowFromPos(cursorPos));
            }
            else
            {
                Console.SetCursorPosition(4 + ColFromPos(cursorPos),
                                         2 + RowFromPos(cursorPos));
            }
        }

        //This function is used to prompt human/user to select a valid square move.
        public int HumanSelectSquare()
        {
            //Take input
            while (Console.KeyAvailable)
            {
                Console.ReadKey(true);
            }
            ConsoleKey key = Console.ReadKey(true).Key;
            
            //Move the cursor up
            if (key == ConsoleKey.UpArrow && Console.CursorTop > 2)
            {
                Console.CursorTop -= 1;
            }

            //Move the cursor down
            else if (key == ConsoleKey.DownArrow && Console.CursorTop < size + 1)
            {
                Console.CursorTop += 1;
            }

            //Move the cursor left
            else if (key == ConsoleKey.LeftArrow && Console.CursorLeft > 4)
            {
                if (extensiveBoard)
                {
                    Console.CursorLeft -= 2;
                }
                else
                {
                    Console.CursorLeft -= 1;
                }
            }
            //Move the cursor right
            else if ((key == ConsoleKey.RightArrow && Console.CursorLeft < size * 2 + 2 && extensiveBoard)
                      || (key == ConsoleKey.RightArrow && Console.CursorLeft < size + 3))
            {
                if (extensiveBoard)
                {
                    Console.CursorLeft += 2;
                }
                else
                {
                    Console.CursorLeft += 1;
                }
            }
            
            //Exit the game
            else if (key == ConsoleKey.Escape) 
            {
                LocalStorage.WriteMoves();
                Menu.Initialize();
            }

            //Switch to undo/redo mode
            else if (key == ConsoleKey.Z)
            {
                //Switch only if the game has already started with at least one move.
                if (LocalStorage.moves.Count > 0)
                {
                    int i = LocalStorage.moves.Count - 1;
                    crossTurn = !crossTurn;
                    Move lastMove = LocalStorage.moves[i];
                    board[lastMove.position] = 0;
                    DrawGameboard(lastSquare);
                    if (i - 1 >= 0) i--;
                    Console.SetCursorPosition(0, 16);
                    Console.WriteLine("\n\nPress Enter to continue game...");
                    ConsoleKey _key = Console.ReadKey(true).Key;

                    //Start the game again by pressing enter
                    while (_key != ConsoleKey.Enter)
                    {
                        //Undo the move
                        if (_key == ConsoleKey.Z && i > -1)
                        {
                            lastMove = LocalStorage.moves[i];
                            board[lastMove.position] = 0;
                            if (i - 1 >= -1) i--;
                            
                        }

                        //Redo the move
                        else if (_key == ConsoleKey.X)
                        {
                            if (i + 1 < LocalStorage.moves.Count) i++;
                            lastMove = LocalStorage.moves[i];
                            board[lastMove.position] = (lastMove.isCross) ? 1 : 2;                                                    
                        }

                        crossTurn = !crossTurn;
                        DrawGameboard(lastSquare);
                        Console.SetCursorPosition(0, 16);
                        Console.WriteLine("\n\nPress Enter to continue game...");
                        _key = Console.ReadKey(true).Key;
                    }

                    //Enter the game again
                    Console.Clear();
                    if (i + 1 < LocalStorage.moves.Count) 
                    {
                        LocalStorage.moves.RemoveRange(i + 1, LocalStorage.moves.Count - (i + 1));
                    }
                    StartIteration();
                }
            }

            //Setting Cursor Position
            int cursorLeftPos = Console.CursorLeft;
            int cursorTopPos = Console.CursorTop;
            if (extensiveBoard)
            {
                Console.SetCursorPosition(size * 2 + 6, 0);
                Console.Write(((cursorLeftPos - 4) / 2 + (cursorTopPos - 2) * size).ToString("000"));
                Console.SetCursorPosition(size * 2 + 6, 1);
                Console.Write((char)('a' + (cursorLeftPos - 4) / 2) + (cursorTopPos - 1).ToString("00"));
            }
            else
            {
                Console.SetCursorPosition(size + 6, 0);
                Console.Write((cursorLeftPos - 4 + (cursorTopPos - 2) * size).ToString("000"));
                Console.SetCursorPosition(size + 6, 1);
                Console.Write((char)('a' + cursorLeftPos - 4) + (cursorTopPos - 1).ToString("00"));
            }
            Console.SetCursorPosition(cursorLeftPos, cursorTopPos);
            if (key == ConsoleKey.Enter)
            {
                if (extensiveBoard)
                {
                    return (cursorLeftPos - 4) / 2 + (Console.CursorTop - 2) * size;
                }
                else
                {
                    return Console.CursorLeft - 4 + (Console.CursorTop - 2) * size;
                }
            }
            else
            {
                return HumanSelectSquare();
            }
        }

        //Returns 0 if no-one wins, 1 if Cross(X) wins and 2 if Circle(O) wins and -1 for a stalemate.
        public int EvaluateWin(int position)
        {
            int targetCursor = board[position];
            int pos2 = position;
            int inRow = 0;
            for (; ColFromPos(pos2) >= 0; pos2--)
            {
                if (ColFromPos(pos2) - 1 < 0 || board[pos2 - 1] != targetCursor)
                {
                    break;
                }
            }
            for (; ColFromPos(pos2) < size; pos2++)
            {
                inRow++;
                if (ColFromPos(pos2) + 1 >= size || board[pos2 + 1] != targetCursor)
                {
                    if (inRow >= 5)
                    {
                        return targetCursor;
                    }
                    break;
                }
            }
            pos2 = position;
            inRow = 0;
            for (; RowFromPos(pos2) >= 0; pos2 -= size)
            {
                if (RowFromPos(pos2) - 1 < 0 || board[pos2 - size] != targetCursor)
                {
                    break;
                }
            }
            for (; RowFromPos(pos2) < size; pos2 += size)
            {
                inRow++;
                if (RowFromPos(pos2) + 1 >= size || board[pos2 + size] != targetCursor)
                {
                    if (inRow >= 5)
                    {
                        return targetCursor;
                    }
                    break;
                }
            }
            pos2 = position;
            inRow = 0;
            for (; RowFromPos(pos2) >= 0 && ColFromPos(pos2) >= 0; pos2 = pos2 - size - 1)
            {
                if (RowFromPos(pos2) - 1 < 0 || ColFromPos(pos2) - 1 < 0 || board[pos2 - size - 1] != targetCursor)
                {
                    break;
                }
            }
            for (; RowFromPos(pos2) < size && ColFromPos(pos2) < size; pos2 += size + 1)
            {
                inRow++;
                if (RowFromPos(pos2) + 1 >= size || ColFromPos(pos2) + 1 >= size || board[pos2 + size + 1] != targetCursor)
                {
                    if (inRow >= 5)
                    {
                        return targetCursor;
                    }
                    break;
                }
            }
            pos2 = position;
            inRow = 0;
            for (; RowFromPos(pos2) >= 0 && ColFromPos(pos2) < size; pos2 = pos2 - size + 1)
            {
                if (RowFromPos(pos2) - 1 < 0 || ColFromPos(pos2) + 1 >= size || board[pos2 - size + 1] != targetCursor)
                {
                    break;
                }
            }
            for (; RowFromPos(pos2) < size && ColFromPos(pos2) >= 0; pos2 += size - 1)
            {
                inRow++;
                if (RowFromPos(pos2) + 1 >= size || ColFromPos(pos2) - 1 < 0 || board[pos2 + size - 1] != targetCursor)
                {
                    if (inRow >= 5)
                    {
                        return targetCursor;
                    }
                    break;
                }
            }
            if (GetBestMoves(1).Length == 0 && board[0] != 0)
            {
                return -1;
            }
            return 0;
        }

        //This function is used to change the square position.
        public void SwitchSquare(int pos)
        {
            previousMove = pos;
            board[pos] = crossTurn ? 1 : 2;            
        }

        //This function is used to switch the turn to other player
        public void SwitchTurn()
        {
            crossTurn = !crossTurn;
        }

        //This function is used to take input from the user when the game ends.
        public void TakeInput()
        {
            while (Console.KeyAvailable)
            {
                Console.ReadKey(true);
            }
            ConsoleKey key = Console.ReadKey(true).Key;

            //Start the game again
            if (key == ConsoleKey.Enter)
            {
                Console.Clear();
                board = new int[size * size];
                StartIteration();
            }

            //Exit the game to main menu
            else if (key == ConsoleKey.Escape)
            {
                Menu.Initialize();
            }

            //Invalid Input
            else
            {
                TakeInput();
            }
        }

        //Display who won the game
        public void DisplayEndingText(int won)
        {
            Console.Clear();
            DrawGameboard();
            Console.SetCursorPosition(2, 3 + size);
            if (won == 1)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("X");
                Console.ResetColor();
                Console.WriteLine(" won!");
            }
            else if (won == 2)
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.Write("O");
                Console.ResetColor();
                Console.WriteLine(" won!");
            }
            else if (won == -1)
            {
                Console.WriteLine("Stalemate!");
            }
            Console.WriteLine();
            Console.WriteLine("Press enter to play another game or ESC to return to main menu.");
        }

        //Get column index from position
        public int ColFromPos(int position)
        {
            return position % size;
        }

        //Get row index from position
        public int RowFromPos(int position)
        {
            return (position / size);
        }

        //This function is used to clear the gameboard and set the cursor postion to 0.
        public void ClearGameboard()
        {
            Console.SetCursorPosition(0, 0);
            for (int i = 0; i < size + 2; i++)
            {
                Console.WriteLine();
            }
            Console.WriteLine();
            Console.SetCursorPosition(0, 0);
        }

        //This function is used to store the best moves.
        public int[] GetBestMoves(int range = 1)
        {
            List<int> moves = new List<int>();
            for (int i = 0; i < this.board.Length; i++)
            {
                if (this.board[i] != 0)
                {
                    if (this.RowFromPos(i) - 1 >= 0 && this.ColFromPos(i) - 1 >= 0 && !moves.Contains(i - 1 - this.size) && this.board[i - 1 - this.size] == 0)
                    {
                        moves.Add(i - 1 - this.size);
                    }
                    if (this.RowFromPos(i) - 1 >= 0 && this.ColFromPos(i) + 1 < this.size && !moves.Contains(i + 1 - this.size) && this.board[i + 1 - this.size] == 0)
                    {
                        moves.Add(i + 1 - this.size);
                    }
                    if (this.RowFromPos(i) + 1 < this.size && this.ColFromPos(i) - 1 >= 0 && !moves.Contains(i - 1 + this.size) && this.board[i - 1 + this.size] == 0)
                    {
                        moves.Add(i - 1 + this.size);
                    }
                    if (this.RowFromPos(i) + 1 < this.size && this.ColFromPos(i) + 1 < this.size && !moves.Contains(i + 1 + this.size) && this.board[i + 1 + this.size] == 0)
                    {
                        moves.Add(i + 1 + this.size);
                    }
                    if (this.RowFromPos(i) - 1 >= 0 && !moves.Contains(i - this.size) && this.board[i - this.size] == 0)
                    {
                        moves.Add(i - this.size);
                    }
                    if (this.RowFromPos(i) + 1 < this.size && !moves.Contains(i + this.size) && this.board[i + this.size] == 0)
                    {
                        moves.Add(i + this.size);
                    }
                    if (this.ColFromPos(i) - 1 >= 0 && !moves.Contains(i - 1) && this.board[i - 1] == 0)
                    {
                        moves.Add(i - 1);
                    }
                    if (this.ColFromPos(i) + 1 < this.size && !moves.Contains(i + 1) && this.board[i + 1] == 0)
                    {
                        moves.Add(i + 1);
                    }
                }
            }
            return moves.ToArray();
        }
    }
}