using System;
using System.IO;
using System.Globalization;

namespace Doom_Scroll.Common
{
    public static class GameLogger
    {
        private static string path = "GameLog.txt";  // default file  name at the app root
        public static void InitFileWriter(string dirpath)
        {
            try
            {
                // Check if directory exists.
                if (Directory.Exists(dirpath))
                {
                    DoomScroll._log.LogInfo("Path exists already.");
                    path = Path.Combine(dirpath + "\\GameLog-" + DateTime.Today.Date.ToString("yyyy-MM-dd") + ".txt");
                    return;
                }

                DirectoryInfo dir = Directory.CreateDirectory(dirpath);
                path = Path.Combine(dir.FullName + "\\GameLog-" + DateTime.Today.Date.ToString("yyyy-MM-dd") + ".txt");
                DoomScroll._log.LogInfo("Directory successfully created at: " + Directory.GetCreationTime(path));
            }
            catch (Exception e)
            {
                DoomScroll._log.LogError("Couldn't create directory: " + e.ToString());
            }
        }
        public static void Write(string logEntry)
        {
            using (StreamWriter sw = new StreamWriter(path, true))
            {
                sw.WriteLine(logEntry);
                sw.Close();
            }
        }

        public static string GetTime()
        {
            return DateTime.Now.ToString("t", CultureInfo.CreateSpecificCulture("en-us"));
        }
    }
}
