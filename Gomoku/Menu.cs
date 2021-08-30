using System;

namespace Gomoku
{
    //This class is responsible for handling the full functionality of game menu before the game starts.
    static class Menu
    {
        //Declaring neccessary variables 
        public static bool IsCrossPlayer = Convert.ToBoolean(Convert.ToInt32(LocalStorage.settings[0].Split('=')[1]));
        public static bool IsCirclePlayer = Convert.ToBoolean(Convert.ToInt32(LocalStorage.settings[1].Split('=')[1]));

        //This function is used to initialize and display the main menu.
        public static void Initialize() 
        {
            Gameboard.extensiveBoard = Convert.ToBoolean(Convert.ToInt32(LocalStorage.settings[2].Split('=')[1]));
            Console.Clear();
            DisplayLogo();
            DisplayMainMenuOptions();
            EvaluateMainMenuChoice(PromptPlayer());
        }

        //This function is used to display the logo.
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

        //This function is used to display the main menu options.
        private static void DisplayMainMenuOptions() 
        {          
            Console.WriteLine("a) Continue game");
            Console.WriteLine("b) New game");
            Console.WriteLine("c) Settings");
            Console.WriteLine("d) Help");
            Console.WriteLine("e) Exit");
            Console.Write("\nEnter your choice: ");
        }

        //This function is used to take input from user and return it in the form of character.
        private static char PromptPlayer() 
        {
            return Console.ReadKey().KeyChar;
        }

        //This function is used to validate and evaluate the character the user pressed if it is associated to a particular functionality
        //If the user presses an unknown/invalid character then display the error message and take input again.
        private static void EvaluateMainMenuChoice(char choice) 
        {
            switch (choice)
            {
                //Only continue the game if moves already exists.
                case 'a':
                    if (LocalStorage.moves.Count > 0)
                    {
                        Program.Initialize();
                        Gameboard conB = new Gameboard(15, IsCrossPlayer, IsCirclePlayer);
                        conB.LoadMoves();
                        conB.StartIteration();
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
                //Play a new game
                case 'b':
                    LocalStorage.moves.Clear();
                    LocalStorage.ClearMoves();
                    Program.Initialize();
                    Gameboard b = new Gameboard(15, IsCrossPlayer, IsCirclePlayer);
                    b.StartIteration();
                    Console.Clear();
                    break;
                //Display Settings
                case 'c':
                    InitializeSettings();
                    break;
                //Display Help
                case 'd':
                    InitializeHelp();
                    break;
                //Exit the game
                case 'e':
                    Environment.Exit(0);
                    break;
                //Display the invalid character error message.
                default:
                    Console.Clear();
                    DisplayLogo();
                    Console.WriteLine("Invalid Choice! Ensure your caps lock are off...\n");
                    DisplayMainMenuOptions();
                    EvaluateMainMenuChoice(PromptPlayer());
                    break;
            }
        }

        //This function is used to initialize and display the settings.
        private static void InitializeSettings() 
        {
            Console.Clear();
            DisplayLogo();
            DisplaySettings();
            EvaluateSettingsChoice(PromptPlayer());
        }

        //This function is used to display the settings value based on the loaded settings.
        private static void DisplaySettings() 
        {
            if (IsCrossPlayer) Console.WriteLine("a) Cross    = Human");
            else Console.WriteLine("a) Cross    = Computer");
            if (IsCirclePlayer) Console.WriteLine("b) Circle   = Human");
            else Console.WriteLine("b) Circle   = Computer");
            if (Gameboard.extensiveBoard) Console.WriteLine("c) Compact  = False");
            else Console.WriteLine("c) Compact  = True");
            Console.WriteLine("\n\t\tp = Back");
            Console.Write("\nEnter your choice: ");        
        }

        //This function is used to validate and evaluate the character the user pressed if it is associated to a particular functionality
        //If the user presses an unknown/invalid character then display the error message and take input again.
        private static void EvaluateSettingsChoice(char choice)
        {
            switch (choice)
            {
                //Toggle Cross Human/Computer
                case 'a':
                    IsCrossPlayer = !IsCrossPlayer;
                    InitializeSettings();
                    break;
                //Toggle Circle Human/Computer
                case 'b':
                    IsCirclePlayer = !IsCirclePlayer;
                    InitializeSettings();
                    break;
                //Toggle Compact True/False
                case 'c':
                    Gameboard.extensiveBoard = !Gameboard.extensiveBoard;
                    InitializeSettings();
                    break;
                case 'p':
                //Back to main menu and save the settings
                    LocalStorage.SaveSettings();
                    Initialize();
                    break;
                //Display the invalid character error message.
                default:
                    Console.Clear();
                    DisplayLogo();
                    Console.WriteLine("Invalid Choice! Ensure your caps lock are off...\n");
                    DisplaySettings();
                    EvaluateSettingsChoice(PromptPlayer());
                    break;
            }
        }

        //This function is used to display the offline help document and controls.
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
