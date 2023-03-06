using System;
using NLog;
using System.Collections.Generic;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace xComfortWingman
{
    public class MyLogger
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public static void DoLog(String text, int level, bool newline, bool addTimestamp = true)
        {
            String date = "";
            String n = "";
            if (addTimestamp) date = ($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff")} - ");
            if (newline) { n = "\n"; }
            if (level > 2 || Program.Settings.GENERAL_DEBUGMODE)
            {
                ConsoleColor fc = (ConsoleColor)Program.Settings.GENERAL_FORECOLOR;
                ConsoleColor bc = (ConsoleColor)Program.Settings.GENERAL_BACKCOLOR;
                
                switch (level)
                {
                    case 5: // Max
                        {
                            Console.BackgroundColor = ConsoleColor.Red;
                            Console.ForegroundColor = ConsoleColor.White;
                            break;
                        }
                    case 4: // High
                        {
                            Console.ForegroundColor = ConsoleColor.White;
                            break;
                        }
                    case 3: // Default
                    default:
                        {
                            Console.ForegroundColor = fc;
                            Console.BackgroundColor = bc;
                            break;
                        }
                    case 2: // Low
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            break;
                        }
                    case 1: // Debug
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            break;
                        }
                    case 0:
                        {
                            Console.ForegroundColor = ConsoleColor.DarkMagenta;
                            break;
                        }
                }
                Console.Write(date + text + n);

                Console.ForegroundColor = fc;
                Console.BackgroundColor = bc;
            }
            if (Program.Settings.GENERAL_OUTPUT_TO_FILE) { System.IO.File.AppendAllText(Program.Settings.GENERAL_OUTPUT_FILE, date + text + n); }
        }

        public static void DoLog(String text)
        {
            DoLog(text, 3, true);
        }

        public static void DoLog(String text, bool newline)
        {
            DoLog(text, 3, newline);
        }

        public static void DoLog(String text, bool newline, bool addTimestamp)
        {
            DoLog(text, 3, newline, addTimestamp);
        }

        public static void DoLog(String text, int level)
        {
            DoLog(text, level, true);
        }

        public static void DoLog(String text, int level, bool newline, int color)
        {
            if (level > 2 || Program.Settings.GENERAL_DEBUGMODE)
            {
                String n = "";
                if (newline) { n = "\n"; }
                Console.Write("[");


                switch (color)
                {
                    case 0: { Console.ForegroundColor = ConsoleColor.Black; break; }
                    case 1: { Console.ForegroundColor = ConsoleColor.DarkBlue; break; }
                    case 2: { Console.ForegroundColor = ConsoleColor.DarkGreen; break; }
                    case 3: { Console.ForegroundColor = ConsoleColor.DarkCyan; break; }
                    case 4: { Console.ForegroundColor = ConsoleColor.DarkRed; break; }
                    case 5: { Console.ForegroundColor = ConsoleColor.DarkMagenta; break; }
                    case 6: { Console.ForegroundColor = ConsoleColor.DarkYellow; break; }
                    case 7: { Console.ForegroundColor = ConsoleColor.Gray; break; }
                    case 8: { Console.ForegroundColor = ConsoleColor.DarkGray; break; }
                    case 9: { Console.ForegroundColor = ConsoleColor.Blue; break; }
                    case 10: { Console.ForegroundColor = ConsoleColor.Green; break; }
                    case 11: { Console.ForegroundColor = ConsoleColor.Cyan; break; }
                    case 12: { Console.ForegroundColor = ConsoleColor.Red; break; }
                    case 13: { Console.ForegroundColor = ConsoleColor.Magenta; break; }
                    case 14: { Console.ForegroundColor = ConsoleColor.Yellow; break; }
                    case 15: { Console.ForegroundColor = ConsoleColor.White; break; }
                    default: { break; }
                }
                Console.Write(text);
                Console.ForegroundColor = (ConsoleColor)Program.Settings.GENERAL_FORECOLOR;
                Console.BackgroundColor = (ConsoleColor)Program.Settings.GENERAL_BACKCOLOR;
                Console.Write("]" + n);
            }
            if(Program.Settings.GENERAL_OUTPUT_TO_FILE) { System.IO.File.AppendAllText(Program.Settings.GENERAL_OUTPUT_FILE, text); }
        }

        public static void LogException(Exception exception)
        {
            DoLog(exception.Message, 5);
            logger.Error(exception);
            if (Program.Settings.GENERAL_OUTPUT_TO_FILE) { System.IO.File.AppendAllText(Program.Settings.GENERAL_OUTPUT_FILE, exception.Message); }
        }
    }
}
