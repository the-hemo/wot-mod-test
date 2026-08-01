using HarmonyLib;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.Settlements;

namespace WoT_Code.HarmonyPatches
{
    [HarmonyPatch(typeof(CompanionsCampaignBehavior), "_desiredTotalCompanionCount", MethodType.Getter)]
    class WandererLimitPatch
    {
        [HarmonyPostfix]
        public static void Postfix(ref float __result) => __result = (float)Town.AllTowns.Count * 1f;
    }
}
