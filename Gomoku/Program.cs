using System;

namespace Gomoku
{
    class Program
    {
        static void Main(string[] args)
        {
            LocalStorage.InitializeSettings();
            Menu.Initialize();
            //Initialize();
            //Board b = new Board(15, true, false);
            //b.StartLoop();
            //Console.Clear();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
        public static void Initialize()
        {
            Console.Clear();
            try
            {
                Console.CursorSize = 100;
            }
            catch (System.PlatformNotSupportedException)
            {

            }
            Console.CancelKeyPress += delegate 
            {
                ExitGame();
            };
        }

        public static void ExitGame()
        {
            Console.Clear();
            Environment.Exit(0);
        }
    }
}
