using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Core;
using TaleWorlds.Localization;

// purpose is to give a speed boost to parties with Aiel Scouts.
// Idea from Discord community member: Seawave

namespace WoT_Code.Behaviours
{
    
    public class WoTAielScoutSpeedModel : DefaultPartySpeedCalculatingModel
    {
        // Speed bonus per Aiel scout troop in the party
        private const float BonusPerAielTroop = 0.02f;

        // Maximum total speed bonus regardless of troop count
        private const float MaxAielSpeedBonus = 0.3f;

        // Minimum number of Aiel troops needed before any bonus applies
        // Prevents a single scout giving a meaningful bonus to a 500-man army
        private const int MinimumAielTroopsForBonus = 3;

        // Aiel scout troop StringIds that contribute to the bonus
        private static readonly string[] AielScoutTroopIds =
        {
            "AielArcher1",
            "AielArcher2",
            "AielArcher3"
        };

        // TextObject for the speed tooltip — shown on campaign map hover
        private static readonly TextObject AielSpeedBonusText =
            new TextObject("Aiel Scouts");

        // =====================================================================
        // Override
        // =====================================================================

        public override ExplainedNumber CalculateFinalSpeed(
            MobileParty mobileParty, ExplainedNumber finalSpeed)
        {
            // Let vanilla calculate base speed first
            ExplainedNumber result = base.CalculateFinalSpeed(mobileParty, finalSpeed);

            // Count Aiel scout troops in this party
            int aielCount = CountAielScoutTroops(mobileParty);

            if (aielCount >= MinimumAielTroopsForBonus)
            {
                // Calculate bonus — 0.1f per troop, capped at MaxAielSpeedBonus
                float bonus = Math.Min(
                    aielCount * BonusPerAielTroop,
                    MaxAielSpeedBonus);

                // AddFactor with TextObject — this is what shows in the
                // campaign map speed tooltip automatically
                result.AddFactor(bonus, AielSpeedBonusText);
            }

            return result;
        }

        // =====================================================================
        // Helper — counts all Aiel scout troops across the party roster
        // =====================================================================

        private int CountAielScoutTroops(MobileParty party)
        {
            if (party?.MemberRoster == null) return 0;

            int count = 0;
            foreach (TroopRosterElement element in party.MemberRoster.GetTroopRoster())
            {
                if (element.Character?.StringId == null) continue;

                foreach (string aielId in AielScoutTroopIds)
                {
                    if (element.Character.StringId.Equals(
                        aielId, StringComparison.OrdinalIgnoreCase))
                    {
                        count += element.Number;
                        break;
                    }
                }
            }

            return count;
        }
    }
}
