using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using TaleWorlds.CampaignSystem.CampaignBehaviors;

namespace WoT_Code.HarmonyPatches
{
    [HarmonyPatch(typeof(LordConversationsCampaignBehavior), "conversation_player_want_to_end_service_as_mercenary_on_condition")]
    internal class SuppressVanillaEndMercenaryLinePatch
    {
        [HarmonyPostfix]
        public static void Postfix(ref bool __result)
        {
            __result = false;
        }
    }
}
