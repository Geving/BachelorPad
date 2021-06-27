using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Linq;

namespace xComfortWingman
{
    class Menu
    {
        private static Settings def = new Settings();

        public static void InfoMenu()
        {
            Console.WriteLine("These are the available arguments:");
            Console.WriteLine("\t -m\tEnables the menu");
            Console.WriteLine("\t -? or -h\tShow this information");
            Console.WriteLine("\t -def \tUses the default settings for this one time");
            Console.WriteLine("\t -debug \tActivates debug mode");
            Console.WriteLine("\t -nope \tDoes nothing...");
        }

        public static void MainMenu()
        {
            def = Settings.DefaultSettings();
            Console.Clear();
            Console.WriteLine($"Welcome!");
            Console.WriteLine($"");
            Console.WriteLine($"Please select an option from the menu:");
            Console.WriteLine($"");
            Console.WriteLine($"\t1. Start (default)");
            Console.WriteLine($"\t2. Settings");
            Console.WriteLine($"\t3. About");
            Console.WriteLine($"\t0. Exit");
            Console.WriteLine($"");
            string selection = Console.ReadKey().KeyChar.ToString();
            switch (selection)
            {
                case "\r":
                case "1":
                    {
                        return;
                    }
                case "2":
                    {
                        SettingsMenu();
                        break;
                    }
                case "3":
                    {
                       
                        break;
                    }
                case "0":
                    {
                        return; // Exit!
                    }
                default:
                    {
                        Console.WriteLine($" is NOT a valid selection!");
                        Thread.Sleep(1000);
                        break;
                    }
            }
            MainMenu();

        }

        public static void SettingsMenu()
        {
            Console.Clear();
            Console.WriteLine($"\t\tSETTINGS\n");
            Console.WriteLine($"\t1. View Settings");
            Console.WriteLine($"\t2. Edit Settings");
            Console.WriteLine($"\t3. Write Settings to file");
            Console.WriteLine($"\t4. Load Settings from file");
            Console.WriteLine($"\t5. Reset to defaults");
            Console.WriteLine($"");
            Console.WriteLine($"\t0. Back");
            Console.WriteLine($"");
            string subselection = Console.ReadKey().KeyChar.ToString();
            switch (subselection)
            {
                case "1":
                    {
                        ProcessSettingsMenu(true);
                        break; }
                case "2":
                    {
                        ProcessSettingsMenu(false);
                        break; }
                case "3":
                    {
                        Console.Clear();
                        Console.Write("Writing settings to file...");
                        Console.WriteLine(Settings.WriteSettingsToFile(Program.Settings, Settings.SettingsFilePath()) ? "[OK]" : "[FAIL]");
                        Console.WriteLine("Press any key to return to the menu...");
                        Console.ReadKey();
                        break;
                    }
                case "4":
                    {
                        Console.Write("Reading settings from file...");
                        Program.Settings = Settings.ReadSettingsFromFile(Settings.SettingsFilePath());
                        Console.WriteLine("[Done]");
                        Console.WriteLine("Press anys key to return to the menu...");
                        Console.ReadKey();
                        break;
                    }
                case "5":
                    {
                        Console.WriteLine("Please type 'default' to confirm that you want to reset all settings to their default values:");
                        if (Console.ReadLine().Replace("'","") == "default" ) { Settings.ResetToDefault(); }
                        break;
                    }
                case "0":
                    {
                        return; // Exit!
                    }
                default:
                    {
                        Console.WriteLine($" is NOT a valid selection!");
                        Thread.Sleep(1000);
                        break;
                    }
            }
            SettingsMenu();
        }

        public static void ProcessSettingsMenu(bool ViewOnly)
        {
            Console.Clear();
            Console.WriteLine($"\t\t{(ViewOnly?"VIEW":"EDIT")} SETTINGS\n");
            Console.WriteLine($"\tPlease select the topic you wish to {(ViewOnly?"view":"edit")}:");
            Console.WriteLine($"");
            Console.WriteLine($"\t1. General - File locations, timeouts, etc");
            Console.WriteLine($"\t2. MQTT - Server address, ports, username, etc");
            Console.WriteLine($"\t3. Homie - Settings related to the Homie specs");
            Console.WriteLine($"\t4. CI (Communication Interface)- USB/RS232, parameters, etc");
            Console.WriteLine($"\t5. All settings!");
            Console.WriteLine($"");
            Console.WriteLine($"\t0. Back");
            string subselection = Console.ReadKey().KeyChar.ToString();
            Console.Clear();
            Thread.Sleep(150);
            switch (subselection)
            {
                case "1": //General
                    {
                        ProcessGroup("GENERAL", ViewOnly);
                        break;
                    }
                case "2": //MQTT
                    {
                        ProcessGroup("MQTT", ViewOnly);
                        break;
                    }                    
                case "3": //Homie
                    {
                        ProcessGroup("HOMIE", ViewOnly);
                        break;
                    }
                case "4": //CI
                    {
                        ProcessGroup("CI", ViewOnly);
                        ProcessGroup("RS232", ViewOnly);

                        break;
                    }
                case "5": //All
                    {
                        ProcessGroup("all", ViewOnly);
                        break;
                    }
                case "0":
                    {
                        //SettingsMenu(); // Exit!
                        return;
                    }
                default:
                    {
                        Console.WriteLine($" is NOT a valid selection!");
                        Thread.Sleep(1000);
                        break;
                    }
            }
            if (ViewOnly)
            {
                Console.WriteLine("Press any key to return to the menu...");
                Console.ReadKey();
            }
            ProcessSettingsMenu(ViewOnly);
        }

        public static void ProcessGroup(string FilterText, bool ViewOnly)
        {
            foreach (System.Reflection.PropertyInfo info in Program.Settings.GetType().GetProperties())
            {
                if (FilterText == "all" || info.Name.StartsWith(FilterText))  // Basic filtering
                {
                    if (ViewOnly)
                    {
                        Console.WriteLine(info.Name.PadRight(40, ' ') + info.GetValue(Program.Settings, null).ToString()); // Print to console
                    }
                    else
                    {
                        ProcessSetting(Program.Settings, info, info.Name);  // Present options to user, store results
                    }
                }
            }
        }

        private static void ProcessSetting(object SettingOwner , System.Reflection.PropertyInfo SettingToProcess, string Title)
        {
            
            object currentValue= SettingOwner.GetType().GetProperty(SettingToProcess.Name).GetValue(def, null);
            object defaultValue = def.GetType().GetProperty(SettingToProcess.Name).GetValue(def, null);

            if (SettingToProcess.PropertyType.IsEnum) //Enums aren't catched by the switch, so we single them out first.
            {
                SettingOwner.GetType().GetProperty(SettingToProcess.Name).SetValue(SettingOwner, SetNewEnumValue(Title, "Please select an option:", currentValue, defaultValue, SettingToProcess.PropertyType));
            }
            else
            {
                switch (SettingToProcess.PropertyType.ToString()) // There are a few tiny differences between the way the types are handled
                {
                    case "System.String":
                        {
                            SettingOwner.GetType().GetProperty(SettingToProcess.Name).SetValue(SettingOwner, SetNewValue(Title, currentValue.ToString(), defaultValue.ToString()));
                            break;
                        }
                    case "System.Int32":
                        {
                            SettingOwner.GetType().GetProperty(SettingToProcess.Name).SetValue(SettingOwner, Convert.ToInt32(SetNewValue(Title, currentValue.ToString(), defaultValue.ToString())));
                            break;
                        }
                    case "System.Boolean":
                        {
                            SettingOwner.GetType().GetProperty(SettingToProcess.Name).SetValue(SettingOwner, SetNewBoolValue(Title, Title + "?", Convert.ToBoolean(currentValue), Convert.ToBoolean(defaultValue)));
                            break;
                        }
                    default:
                        {
                            Console.WriteLine("Not sure how to handle '" + SettingToProcess.Name + "'  of type: " + SettingToProcess.PropertyType.ToString());
                            break;
                        }
                }
            }
        }

        private static string SetNewValue(string SettingName, string SettingValue,string DefaultValue, string[] Selection)
        {
            Console.WriteLine($"\t{SettingName}");
            Console.WriteLine($"Please select a new value for {SettingName}: ");
            Console.WriteLine("");
            int itmCnt = 1;
            foreach (string item in Selection)
            {
                Console.WriteLine($"\t{itmCnt++}) {item} {(item==SettingValue?"(current)":"")}{(item == DefaultValue ? "(default)" : "")}");
            }
            Console.WriteLine("\t0. Cancel");
            //string newVal = Console.ReadLine();
            string newVal = Console.ReadKey().KeyChar.ToString(); //This works as long as one remembers to keep the number of alternatives to a maximum of 9, or switch to letters.
            switch (newVal)
            {
                case "": //Keep current setting
                    { return SettingValue; }
                case "default":
                    {
                        return DefaultValue;
                    }
                default:
                    {
                        try
                        {
                            string selected = Selection[Convert.ToInt32(newVal)-1];
                            Console.WriteLine(". " + Selection[Convert.ToInt32(newVal)]);
                            return selected;
                        } catch //(Exception exception)
                        {
                            Console.WriteLine($"\n({newVal} is NOT a valid seletion!)");
                            return SetNewValue(SettingName, SettingValue, DefaultValue, Selection);
                        }
                    }
                }
            }
      
        private static string SetNewValue(string SettingName, string SettingValue, string DefaultValue)
        {
            Console.WriteLine($"\t{SettingName}");
            Console.WriteLine($"To keep the current value of '{SettingValue}', just press 'Enter'.");
            Console.WriteLine($"To revert to the default value of '{SettingValue}', type 'default'.");
            Console.WriteLine($"Please enter a new value for {SettingName}: ");
            string newVal = Console.ReadLine();
            switch (newVal)
            {
                case "": //Keep current setting
                    { return SettingValue; }
                case "default":
                    { return DefaultValue; }
                default:
                    { return newVal; }
            }
        }

        private static bool SetNewBoolValue(string Topic, string Question, bool SettingValue, bool DefaultValue)
        {
            Console.WriteLine($"\t{Topic}");
            Console.WriteLine($"{Question}");
            Console.WriteLine("");

            Console.WriteLine($"\t1) Yes{(true == SettingValue ? "(current)" : "")}{(true == DefaultValue ? "(default)" : "")}");
            Console.WriteLine($"\t2) No {(false == SettingValue ? "(current)" : "")}{(false == DefaultValue ? "(default)" : "")}");
            Console.WriteLine("\t0. Cancel");

            string newVal = Console.ReadKey().KeyChar.ToString();
            switch (newVal)
            {
                case "":  { return SettingValue; }
                case "y":
                case "1": { return true; }
                case "n":
                case "2": { return false; }
                default:
                    {
                        Console.WriteLine($"\n({newVal} is NOT a valid seletion!)");
                        return SetNewBoolValue(Topic, Question, SettingValue, DefaultValue);
                    }
            }
        }

        private static object SetNewEnumValue(string Topic, string Question, object SettingValue, object DefaultValue, Type Selection)
        {
            Console.WriteLine($"\t{Topic}");
            Console.WriteLine($"{Question}");
            Console.WriteLine("");

            var enums = Enum.GetValues(Selection);

            foreach (object item in enums)
            {
                Console.WriteLine($"\t{(int)item+1}) {item.ToString()} {(item == SettingValue ? "(current)" : "")}{(item == DefaultValue ? "(default)" : "")}");
            }

            Console.WriteLine("\t0. Cancel");

            string newVal = Console.ReadKey().KeyChar.ToString(); //This works as long as one remembers to keep the number of alternatives to a maximum of 9, or switch to letters.
            switch (newVal)
            {
                case "0":
                case "": //Keep current setting
                    { return SettingValue; }
                case "default":
                    {
                        return DefaultValue;
                    }
                default:
                    {
                        try
                        {
                            //Console.WriteLine(". " + Selection[Convert.ToInt32(newVal)]);
                            return enums.GetValue(Convert.ToInt32(newVal)-1);
                        }
                        catch //(Exception exception)
                        {
                            Console.WriteLine($"\n({newVal} is NOT a valid seletion!)");
                            return SetNewEnumValue(Topic, Question, SettingValue, DefaultValue, Selection);
                        }
                    }
            }
        }

    }
}
