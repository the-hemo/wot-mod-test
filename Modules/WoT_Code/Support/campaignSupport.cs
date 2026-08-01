using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Library;

namespace WoT_Code.Support
{
    internal class campaignSupport
    {
        public static void displayMessageInChat(String s)
        {
            InformationManager.DisplayMessage(new InformationMessage(s));
        }
        public static void displayMessageInChat(String s, Color consoleColor)
        {
            InformationManager.DisplayMessage(new InformationMessage(s, consoleColor));
        }
    }
}
