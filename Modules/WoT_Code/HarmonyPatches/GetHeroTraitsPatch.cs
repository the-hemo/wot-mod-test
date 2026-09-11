using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Library;
using WoT_Code.MagicModel;

namespace WoT_Code.HarmonyPatches
{
    /*
    [HarmonyPatch(typeof(CampaignUIHelper), nameof(CampaignUIHelper.GetHeroTraits))]
    public class GetHeroTraitsPatch
    {

        static void Postfix(ref IEnumerable<TraitObject> __result)
        {
            //InformationManager.DisplayMessage(new InformationMessage("GetHeroTraits postfix fired"));
            __result = __result.AddItem(WoTTraits.CanChannel);
        }
    }
    */
}
