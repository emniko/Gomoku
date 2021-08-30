using System;

namespace Gomoku
{
    //This is the main class for starting the game.
    class Program
    {
        //This function is responsible for starting the program.
        static void Main(string[] args)
        {
            LocalStorage.InitializeSettings();
            LocalStorage.InitializeMoves();
            Menu.Initialize();
        }

        //This function is used to set the cursor size on the console.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
        public static void Initialize()
        {
            Console.Clear();
            try
            {
                Console.CursorSize = 100;
            }
            catch (PlatformNotSupportedException)
            {

            }
            Console.CancelKeyPress += delegate 
            {
                ExitGame();
            };
        }

        //This function is used to exit the game.
        public static void ExitGame()
        {
            Console.Clear();
            Environment.Exit(0);
        }
    }
}
