using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.TournamentGames;
using TaleWorlds.Core;

namespace WoT_Code.HarmonyPatches
{
    // Purpose is to allow troops to participate with lower skill requirements as i re did the skill
    //values to be more realistic.
    internal class ArenaParticipantPatch
    {
        [HarmonyPatch(typeof(FightTournamentGame), nameof(FightTournamentGame.CanBeAParticipant))]
        public class TournamentParticipantPatch
        {
            static void Postfix(CharacterObject character, bool considerSkills, ref bool __result)
            {

                if (!character.IsHero)
                {
                    __result = character.Tier >= 3;
                    if (considerSkills)
                    {
                        int oneHanded = character.HeroObject.GetSkillValue(DefaultSkills.OneHanded);
                        int twoHanded = character.HeroObject.GetSkillValue(DefaultSkills.TwoHanded);

                        __result = oneHanded >= 10 || twoHanded >= 10; // instead of 100
                    }
                    
                }
             }
        }
    }
}
