using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gomoku
{
    static class Menu
    {
        public static bool IsCrossPlayer = Convert.ToBoolean(Convert.ToInt32(LocalStorage.settings[0].Split('=')[1]));
        public static bool IsCirclePlayer = Convert.ToBoolean(Convert.ToInt32(LocalStorage.settings[1].Split('=')[1]));

        public static void Initialize() 
        {
            Board.wideningSpacesMode = Convert.ToBoolean(Convert.ToInt32(LocalStorage.settings[2].Split('=')[1]));
            Console.Clear();
            DisplayLogo();
            DisplayMainMenuOptions();
            EvaluateMainMenuChoice(PromptPlayer());
        }

        private static void DisplayLogo() 
        {
            Console.WriteLine(@"
  ________                       __          
 /  _____/  ____   _____   ____ |  | ____ __ 
/   \  ___ /  _ \ /     \ /  _ \|  |/ /  |  \
\    \_\  (  <_> )  Y Y  (  <_> )    <|  |  /
 \______  /\____/|__|_|  /\____/|__|_ \____/ 
        \/             \/            \/      
");
        }

        private static void DisplayMainMenuOptions() 
        {          
            Console.WriteLine("a) Continue game");
            Console.WriteLine("b) New game");
            Console.WriteLine("c) Settings");
            Console.WriteLine("d) Help");
            Console.WriteLine("e) Exit");
            Console.Write("\nEnter your choice: ");
        }

        private static char PromptPlayer() 
        {
            return Console.ReadKey().KeyChar;
        }

        private static void EvaluateMainMenuChoice(char choice) 
        {
            switch (choice)
            {
                case 'a':
                    if (LocalStorage.moves.Count > 0)
                    {
                        Program.Initialize();
                        Board conB = new Board(15, IsCrossPlayer, IsCirclePlayer);
                        conB.LoadMoves();
                        conB.StartLoop();
                        Console.Clear();
                    }
                    else 
                    {
                        Console.Clear();
                        DisplayLogo();
                        Console.WriteLine("No save game found! Play new game...\n");
                        DisplayMainMenuOptions();
                        EvaluateMainMenuChoice(PromptPlayer());
                    }
                    break;
                case 'b':
                    LocalStorage.moves.Clear();
                    LocalStorage.ClearMoves();
                    Program.Initialize();
                    Board b = new Board(15, IsCrossPlayer, IsCirclePlayer);
                    b.StartLoop();
                    Console.Clear();
                    break;
                case 'c':
                    InitializeSettings();
                    break;
                case 'd':
                    InitializeHelp();
                    break;
                case 'e':
                    Environment.Exit(0);
                    break;
                default:
                    Console.Clear();
                    DisplayLogo();
                    Console.WriteLine("Invalid Choice! Ensure your caps lock are off...\n");
                    DisplayMainMenuOptions();
                    EvaluateMainMenuChoice(PromptPlayer());
                    break;
            }
        }

        private static void InitializeSettings() 
        {
            Console.Clear();
            DisplayLogo();
            DisplaySettings();
            EvaluateSettingsChoice(PromptPlayer());
        }

        private static void DisplaySettings() 
        {
            if (IsCrossPlayer) Console.WriteLine("a) Cross    = Human");
            else Console.WriteLine("a) Cross    = Computer");
            if (IsCirclePlayer) Console.WriteLine("b) Circle   = Human");
            else Console.WriteLine("b) Circle   = Computer");
            if (Board.wideningSpacesMode) Console.WriteLine("c) Compact  = False");
            else Console.WriteLine("c) Compact  = True");
            Console.WriteLine("\n\t\tp = Back");
            Console.Write("\nEnter your choice: ");        
        }


        private static void EvaluateSettingsChoice(char choice)
        {
            switch (choice)
            {
                case 'a':
                    IsCrossPlayer = !IsCrossPlayer;
                    InitializeSettings();
                    break;
                case 'b':
                    IsCirclePlayer = !IsCirclePlayer;
                    InitializeSettings();
                    break;
                case 'c':
                    Board.wideningSpacesMode = !Board.wideningSpacesMode;
                    InitializeSettings();
                    break;
                case 'p':
                    LocalStorage.SaveSettings();
                    Initialize();
                    break;
                default:
                    Console.Clear();
                    DisplayLogo();
                    Console.WriteLine("Invalid Choice! Ensure your caps lock are off...\n");
                    DisplaySettings();
                    EvaluateSettingsChoice(PromptPlayer());
                    break;
            }
        }

        private static void InitializeHelp() 
        {
            Console.Clear();
            DisplayLogo();
            Console.WriteLine("Welcome to the Gomoku aka Five in a Row Game!");
            Console.WriteLine("\nGomoku is a traditional Japanese board game for 2 players that is similar to but more complex than tic-tac-toe. During the game, players take turns placing black and white pieces (cross and circle in this game) on the board with the goal of creating an unbroken line of 5 pieces in any direction. A traditional Gomoku board has a 15x15 grid of lines, but is sometimes played on a Go board, which has a 19x19 grid.");
            Console.WriteLine("\nThe first player to get five consecutive stones on the board either horizontally, vertically, or diagonally wins. So, in addition to trying to get your 5 consecutive stones on the board, you also have to block the other player’s stones from forming the winning pattern. Once you get the basics of the game down, you’ll have to strategize your moves to win.");
            Console.WriteLine("\nControls:");
            Console.WriteLine("Arrow Keys = To move the cursor");
            Console.WriteLine("Enter = To place stone/piece");
            Console.WriteLine("Z = To undo move");
            Console.WriteLine("X = To redo move");
            Console.WriteLine("ESC = To save the game and return to main menu");
            Console.Write("\nPress any key to proceed to main menu...");
            Console.ReadKey();
            Initialize();
        }
    }  
}
