using System;
using System.Collections.Generic;
using System.Text;

namespace xComfortWingman
{
    class Logo
    {
        //These logos were made with this tool: http://patorjk.com/software/taag/

        public static void DrawLogo(int LogoType = 0)
        {
            if ((LogoType == 1 || LogoType==3) && Console.WindowWidth < 138) { LogoType = 0; }
            Console.WriteLine();
            switch (LogoType)
            {
                case 0:
                    {
                        string space = "".PadLeft((Console.WindowWidth - 77) / 2, ' ');
                        Console.WriteLine(space + @"        ____                 __            _   ____  __  __  ___ _____ _____ ");
                        Console.WriteLine(space + @" __  __/ ___|___  _ __ ___  / _| ___  _ __| |_|___ \|  \/  |/ _ \_   _|_   _|");
                        Console.WriteLine(space + @" \ \/ / |   / _ \| '_ ` _ \| |_ / _ \| '__| __| __) | |\/| | | | || |   | |  ");
                        Console.WriteLine(space + @"  >  <| |__| (_) | | | | | |  _| (_) | |  | |_ / __/| |  | | |_| || |   | |  ");
                        Console.WriteLine(space + @" /_/\_\\____\___/|_| |_| |_|_|  \___/|_|   \__|_____|_|  |_|\__\_\|_|   |_|  ");
                        break;
                    }
                case 1:
                    {
                        string space = "".PadLeft((Console.WindowWidth - 148) / 2, ' ');
                        Console.WriteLine(space + @":::    :::  ::::::::   ::::::::  ::::    ::::  :::::::::: ::::::::  ::::::::: ::::::::::: ::::::::  ::::    ::::   :::::::: ::::::::::: ::::::::::: ");
                        Console.WriteLine(space + @":+:    :+: :+:    :+: :+:    :+: +:+:+: :+:+:+ :+:       :+:    :+: :+:    :+:    :+:    :+:    :+: +:+:+: :+:+:+ :+:    :+:    :+:         :+:     ");
                        Console.WriteLine(space + @" +:+  +:+  +:+        +:+    +:+ +:+ +:+:+ +:+ +:+       +:+    +:+ +:+    +:+    +:+          +:+  +:+ +:+:+ +:+ +:+    +:+    +:+         +:+     ");
                        Console.WriteLine(space + @"  +#++:+   +#+        +#+    +:+ +#+  +:+  +#+ :#::+::#  +#+    +:+ +#++:++#:     +#+        +#+    +#+  +:+  +#+ +#+    +:+    +#+         +#+     ");
                        Console.WriteLine(space + @" +#+  +#+  +#+        +#+    +#+ +#+       +#+ +#+       +#+    +#+ +#+    +#+    +#+      +#+      +#+       +#+ +#+  # +#+    +#+         +#+     ");
                        Console.WriteLine(space + @"#+#    #+# #+#    #+# #+#    #+# #+#       #+# #+#       #+#    #+# #+#    #+#    #+#     #+#       #+#       #+# #+#   +#+     #+#         #+#     ");
                        Console.WriteLine(space + @"###    ###  ########   ########  ###       ### ###        ########  ###    ###    ###    ########## ###       ###  ###### ###   ###         ###     ");
                        break;
                    }
                case 3:
                    {
                        string space = "".PadLeft((Console.WindowWidth - 97) / 2, ' ');
                        Console.WriteLine(space + @"         #####                                            #####  #     #  #####  ####### ####### ");
                        Console.WriteLine(space + @" #    # #     #  ####  #    # ######  ####  #####  ##### #     # ##   ## #     #    #       #    ");
                        Console.WriteLine(space + @"  #  #  #       #    # ##  ## #      #    # #    #   #         # # # # # #     #    #       #    ");
                        Console.WriteLine(space + @"   ##   #       #    # # ## # #####  #    # #    #   #    #####  #  #  # #     #    #       #    ");
                        Console.WriteLine(space + @"   ##   #       #    # #    # #      #    # #####    #   #       #     # #   # #    #       #    ");
                        Console.WriteLine(space + @"  #  #  #     # #    # #    # #      #    # #   #    #   #       #     # #    #     #       #    ");
                        Console.WriteLine(space + @" #    #  #####   ####  #    # #       ####  #    #   #   ####### #     #  #### #    #       #    ");
                        break;
                    }
                case 4:
                    {
                        string space = "".PadLeft((Console.WindowWidth - 138) / 2, ' ');
                        Console.WriteLine(space + "          .d8888b.                          .d888                 888     .d8888b.  888b     d888  .d88888b. 88888888888 88888888888 ");
                        Console.WriteLine(space + "         d88P  Y88b                        d88P\"                  888    d88P  Y88b 8888b   d8888 d88P\" \"Y88b    888         888     ");
                        Console.WriteLine(space + "         888    888                        888                    888           888 88888b.d88888 888     888    888         888     ");
                        Console.WriteLine(space + "888  888 888         .d88b.  88888b.d88b.  888888 .d88b.  888d888 888888      .d88P 888Y88888P888 888     888    888         888     ");
                        Console.WriteLine(space + "`Y8bd8P' 888        d88\"\"88b 888 \"888 \"88b 888   d88\"\"88b 888P\"   888     .od888P\"  888 Y888P 888 888     888    888         888     ");
                        Console.WriteLine(space + "  X88K   888    888 888  888 888  888  888 888   888  888 888     888    d88P\"      888  Y8P  888 888 Y8b 888    888         888     ");
                        Console.WriteLine(space + ".d8\"\"8b. Y88b  d88P Y88..88P 888  888  888 888   Y88..88P 888     Y88b.  888\"       888   \"   888 Y88b.Y8b88P    888         888     ");
                        Console.WriteLine(space + "888  888  \"Y8888P\"   \"Y88P\"  888  888  888 888    \"Y88P\"  888      \"Y888 888888888  888       888  \"Y888888\"     888         888     ");
                        Console.WriteLine(space + "                                                                                                         Y8b                         ");
                        break;
                    }
                case 5:
                    {
                        string space = "".PadLeft((Console.WindowWidth - 90) / 2, ' ');
                        Console.WriteLine(space + @"       _______              ___                  ______  _______ _______ _______ _______ ");
                        Console.WriteLine(space + @"      (_______)            / __)             _  (_____ \(_______|_______|_______|_______)");
                        Console.WriteLine(space + @" _   _ _       ___  ____ _| |__ ___   ____ _| |_  ____) )_  _  _ _    _     _       _    ");
                        Console.WriteLine(space + @"( \ / ) |     / _ \|    (_   __) _ \ / ___|_   _)/ ____/| ||_|| | |  | |   | |     | |   ");
                        Console.WriteLine(space + @" ) X (| |____| |_| | | | || | | |_| | |     | |_| (_____| |   | | |__| |   | |     | |   ");
                        Console.WriteLine(space + @"(_/ \_)\______)___/|_|_|_||_|  \___/|_|      \__)_______)_|   |_|\______)  |_|     |_|   ");
                        Console.WriteLine(space + @"                                                                                         ");
                        break;
                    }
                default:
                    { break; }
            }
            Console.WriteLine();
        }
    }
}
