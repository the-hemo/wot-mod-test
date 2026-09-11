using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using WoT_Code.MagicModel;

namespace WoT_Code.HarmonyPatches
    
{
    /*
        [HarmonyPatch(typeof(CampaignUIHelper), nameof(CampaignUIHelper.GetTraitTooltipText))]
        internal class CustomTraitTooltipPatch
        {
            // register any custom (non-vanilla) traits that need tooltips here —
            // saves re-patching this every time WoT adds another trait later
            private static readonly HashSet<TraitObject> CustomTraits = new HashSet<TraitObject>
    {
        WoTTraits.CanChannel
    };

            [HarmonyPrefix]
            static bool Prefix(TraitObject traitObject, int traitValue, ref string __result)
            {
                if (!CustomTraits.Contains(traitObject))
                    return true; // not ours — let vanilla run its own whitelist as normal

                GameTexts.SetVariable("NEWLINE", "\n");

                if (traitValue != 0)
                {
                    TextObject content = GameTexts.FindText("str_trait_name_" + traitObject.StringId.ToLower(),
                        (traitValue + MathF.Abs(traitObject.MinValue)).ToString());
                    GameTexts.SetVariable("TRAIT_VALUE", traitValue);
                    GameTexts.SetVariable("TRAIT_NAME", content);
                    TextObject content2 = GameTexts.FindText("str_trait", traitObject.StringId.ToLower());
                    GameTexts.SetVariable("TRAIT", content2);
                    GameTexts.SetVariable("TRAIT_DESCRIPTION", traitObject.Description);
                    __result = GameTexts.FindText("str_trait_tooltip", null).ToString();
                }
                else
                {
                    TextObject content3 = GameTexts.FindText("str_trait", traitObject.StringId.ToLower());
                    GameTexts.SetVariable("TRAIT", content3);
                    GameTexts.SetVariable("TRAIT_DESCRIPTION", traitObject.Description);
                    __result = GameTexts.FindText("str_trait_description_tooltip", null).ToString();
                }

                return false; // skip original — it would hit the FailedAssert for us
            }
        }
    */
}
