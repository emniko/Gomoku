using System;
using System.IO;

namespace Gomoku
{
    public class LocalStorage
    {
        public static string[] settings;
        private static string fileName = AppDomain.CurrentDomain.BaseDirectory + "\\settings.dat";
        public static void InitializeSettings() 
        {
            if (File.Exists(fileName))
            {
                settings = File.ReadAllLines(fileName);
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

        private static void WriteSettings()
        {
            using (StreamWriter sw = new StreamWriter(fileName, false))
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
    }
}
