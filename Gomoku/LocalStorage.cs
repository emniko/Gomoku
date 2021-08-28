using System;
using System.Collections.Generic;
using System.IO;

namespace Gomoku
{
    public class LocalStorage
    {
        public static string[] settings;
        public static List<Move> moves;
        private static string settingsFileName = AppDomain.CurrentDomain.BaseDirectory + "\\settings.dat";
        private static string movesFileName = AppDomain.CurrentDomain.BaseDirectory + "\\moves.dat";
        public static void InitializeSettings() 
        {
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
        public static void InitializeMoves()
        {
            moves = new List<Move>();

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

        public static void SaveSettings() 
        {
            settings[0] = "Cross=" + Convert.ToInt32(Menu.IsCrossPlayer);
            settings[1] = "Circle=" + Convert.ToInt32(Menu.IsCirclePlayer);
            settings[2] = "Compact=" + Convert.ToInt32(Board.wideningSpacesMode);
            WriteSettings();
        }

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

        public static void ClearMoves()
        {
            using (StreamWriter sw = new StreamWriter(movesFileName, false))
            {
            }
        }
    }
}
