using HarmonyLib;
using System.Reflection;
using TaleWorlds.CampaignSystem.CharacterDevelopment;

namespace WoT_Code.HarmonyPatches
{
    [HarmonyPatch(typeof(DefaultCulturalFeats), "InitializeAll")]
    public static class InitializeAllPatch
    {
        static void Postfix(DefaultCulturalFeats __instance)
        {
            // Use reflection to access the private field _battaniaMilitiaFeat
            FieldInfo battaniaMilitiaFeatField = typeof(DefaultCulturalFeats).GetField("_battaniaMilitiaFeat", BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo khuzaitAnimalProductionFeatField = typeof(DefaultCulturalFeats).GetField("_khuzaitAnimalProductionFeat", BindingFlags.NonPublic | BindingFlags.Instance);

            if (battaniaMilitiaFeatField != null)
            {
                // Get the current value of _battaniaMilitiaFeat
                var battaniaMilitiaFeat = battaniaMilitiaFeatField.GetValue(__instance) as FeatObject;

                if (battaniaMilitiaFeat != null)
                {
                    // Re-initialize _battaniaMilitiaFeat with the new string
                    battaniaMilitiaFeat.Initialize("{=!}battanian_militia_production", "{=lt_1qUFMK28}Towns owned by this culture's rulers have +1 militia production.", 1f, true, FeatObject.AdditionType.Add);
                }
            }
            if (khuzaitAnimalProductionFeatField != null)
            {
                // Get the current value of _khuzaitAnimalProductionFeat
                var khuzaitAnimalProductionFeat = khuzaitAnimalProductionFeatField.GetValue(__instance) as FeatObject;

                if (khuzaitAnimalProductionFeat != null)
                {
                    // Re-initialize _khuzaitAnimalProductionFeat with the new string
                    khuzaitAnimalProductionFeat.Initialize("{=!}khuzait_increased_animal_production", "{=lt_Xaw2CoCG}25% production bonus to horse, mule, cow and sheep in villages owned by this culture's rulers.", 0.25f, true, FeatObject.AdditionType.AddFactor);
                }
            }
        }
    }
}

