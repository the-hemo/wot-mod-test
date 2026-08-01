using HarmonyLib;
using TaleWorlds.CampaignSystem.GameComponents;

namespace WoT_Code.HarmonyPatches
{
    // Rebellions arent a common thing in the wheel of time books and we use culture more because the game model is built around this.
    // So this patch reduces the loyalty penalty when a settlement is owned by a different culture to reduce rebellon frequency.
    
    [HarmonyPatch(typeof(DefaultSettlementLoyaltyModel), "SettlementOwnerDifferentCultureLoyaltyEffect", MethodType.Getter)]
    internal class CultureLoyaltyPatch
    {
        [HarmonyPostfix]
        public static void Postfix(ref float __result) => __result = -1f;
    }
}
