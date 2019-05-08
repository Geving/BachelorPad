using System;
using System.Collections.Generic;
using System.Text;

namespace xComfortWingman
{
    public class Logger
    {

        public static void DoLog(String text, int level, bool newline)
        {
            ConsoleColor fc = Console.ForegroundColor;
            ConsoleColor bc = Console.BackgroundColor;
            String n = "";
            if (newline) { n = "\n"; }
            switch (level)
            {
                case 5: // Max
                    {
                        Console.BackgroundColor = ConsoleColor.Red;
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write(text + n);
                        break;
                    }
                case 4: // High
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write(text + n);
                        break;
                    }
                case 3: // Default
                default:
                    {
                        Console.Write(text + n);
                        break;
                    }
                case 2: // Low
                    {
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        Console.Write(text + n);
                        break;
                    }
                case 1: // Debug
                    {
                        if (Program.Settings.DEBUGMODE)
                        {
                            Console.ForegroundColor = ConsoleColor.DarkYellow;
                            Console.Write(text + n);
                        }
                        break;
                    }
                case 0:
                    {
                        if (Program.Settings.DEBUGMODE)
                        {
                            Console.BackgroundColor = ConsoleColor.DarkYellow;
                            Console.ForegroundColor = ConsoleColor.Black;
                            Console.Write(text + n);
                        }
                        break;
                    }
            }
            Console.ForegroundColor = fc;
            Console.BackgroundColor = bc;
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
            DoLog(text, 3, true);
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

    }
}
