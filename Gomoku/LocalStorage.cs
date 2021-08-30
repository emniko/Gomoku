using System;
using System.Collections.Generic;
using System.IO;

namespace Gomoku
{
    //This class is used to store local data through which game can be saved at any state even after exiting it.
    public class LocalStorage
    {
        //Declaring neccessary variables
        public static string[] settings;
        public static List<Move> moves;
        private static string settingsFileName = AppDomain.CurrentDomain.BaseDirectory + "\\settings.dat"; //Reading settings file
        private static string movesFileName = AppDomain.CurrentDomain.BaseDirectory + "\\moves.dat"; //Reading moves file

        //This function is used to load settings file into game.
        public static void InitializeSettings() 
        {
            //If file exists then load otherwise create a new file in the directory with new settings.
            if (File.Exists(settingsFileName))
            {
                settings = File.ReadAllLines(settingsFileName);
            }
            else
            {
                settings = new string[3];
                settings[0] = "Cross=1";
                settings[1] = "Circle=0";
                settings[2] = "Compact=0";
                WriteSettings();
            }
        }

        //This function is used to load settings file into game.
        public static void InitializeMoves()
        {
            moves = new List<Move>();

            //If file exists then load moves otherwise create a blank file in the directory.
            if (File.Exists(movesFileName))
            {
                string[] moveLines = File.ReadAllLines(movesFileName);
                foreach (string move in moveLines) 
                {
                    string[] tokens = move.Split(',');
                    moves.Add(new Move(Convert.ToBoolean(Convert.ToInt32(tokens[0])), Convert.ToInt32(tokens[1])));
                }
            }
            else
            {
                //Blank file
                using (File.Create(movesFileName)) { } 
            }
        }

        //This function is used to write new settings into the settings file.
        private static void WriteSettings()
        {
            using (StreamWriter sw = new StreamWriter(settingsFileName, false))
            {
                foreach (string setting in settings)
                {
                    sw.WriteLine(setting);
                }
            }
        }

        //This function is used to save new settings into the game.
        public static void SaveSettings() 
        {
            settings[0] = "Cross=" + Convert.ToInt32(Menu.IsCrossPlayer);
            settings[1] = "Circle=" + Convert.ToInt32(Menu.IsCirclePlayer);
            settings[2] = "Compact=" + Convert.ToInt32(Gameboard.extensiveBoard);
            WriteSettings();
        }

        //This function is used to write new moves into the moves file.
        public static void WriteMoves() 
        {
            using (StreamWriter sw = new StreamWriter(movesFileName, false))
            {
                foreach (Move move in moves)
                {
                    sw.WriteLine($"{Convert.ToInt32(move.isCross)},{move.position}");
                }
            }
        }

        //This function is used to clear all existing moves from the moves file.
        public static void ClearMoves()
        {
            using (StreamWriter sw = new StreamWriter(movesFileName, false))
            {
            }
        }
    }
}
