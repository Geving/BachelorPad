using System;
using NLog;
using System.Collections.Generic;
using System.Text;

namespace xComfortWingman
{
    public class MyLogger
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public static void DoLog(String text, int level, bool newline)
        {
            if (level > 2 || Program.Settings.DEBUGMODE)
            {
                //ConsoleColor fc = Console.ForegroundColor;
                //ConsoleColor bc = Console.BackgroundColor;
                String date = ($"{DateTime.Now.Year}-{DateTime.Now.Month.ToString("00")}-{DateTime.Now.Day.ToString("00")} {DateTime.Now.Hour.ToString("00")}:{DateTime.Now.Minute.ToString("00")}:{DateTime.Now.Second.ToString("00")}.{DateTime.Now.Millisecond.ToString("000")} - ");
                String n = "";
                if (newline) { n = "\n"; }
                //text = date + text + n;
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
                            Console.ResetColor();
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
                            //Console.BackgroundColor = ConsoleColor.DarkMagenta;
                            //Console.ForegroundColor = ConsoleColor.White;
                            break;
                        }
                }
                Console.Write(date + text + n);

                //Console.ForegroundColor = fc;
                //Console.BackgroundColor = bc;
                Console.ResetColor();
            }
        }

        public static void DoLog(String text)
        {
            DoLog(text, 3, true);
        }

        public static void DoLog(String text, bool newline)
        {
            DoLog(text, 3, newline);
        }

        public static void DoLog(String text, int level)
        {
            DoLog(text, level, true);
        }

        public static void DoLog(String text, int level, bool newline, int color)
        {
            if (level > 2 || Program.Settings.DEBUGMODE)
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
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write("]" + n);
            }
        }

        public static void LogException(Exception exception)
        {
            DoLog(exception.Message, 5);
            logger.Error(exception);
        }
    }
}
