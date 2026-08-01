using HarmonyLib;
using TaleWorlds.Core;

namespace WoT_Code.HarmonyPatches
{

    [HarmonyPatch(typeof(DefaultItemValueModel), "CalculateArmorTier")]
    // purpose is to make armour appear more in shops and make cost reflective of high armour stat items
    internal class CalculateArmorTierPatch
    {
        [HarmonyPostfix]
        public static void Postfix(ref float __result)
        {
            if (__result > 0)
            {
                //increased from 0.8 to 0.85 to increase value, tier and rarity in 1.45.
                __result = (__result + 0.4f) * 0.9f - 0.4f;
            }
        }
    }
  
}