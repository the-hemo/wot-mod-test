using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Localization;
using WoT_Code.Support;

namespace WoT_Code.Behaviours
{
    public class WoTPartyHealingModel : DefaultPartyHealingModel
    {
        public override ExplainedNumber GetDailyHealingHpForHeroes(
            PartyBase party, bool isPrisoners, bool includeDescriptions = false)
        {
            ExplainedNumber result = base.GetDailyHealingHpForHeroes(party, isPrisoners, includeDescriptions);

            if (isPrisoners) return result;

            // When party is null, the engine is calculating for the player's main party
            MobileParty mobileParty = party?.MobileParty ?? MobileParty.MainParty;
            if (mobileParty == null) return result;

            float channellerPercent = WoTChannellerUtils.GetChannellerPercentage(mobileParty);
            if (channellerPercent > 0f)
                result.AddFactor(channellerPercent, new TextObject("One Power Healing"));

            return result;
        }

        public override ExplainedNumber GetDailyHealingForRegulars(
            PartyBase party, bool isPrisoners, bool includeDescriptions = false)
        {
            ExplainedNumber result = base.GetDailyHealingForRegulars(party, isPrisoners, includeDescriptions);
            // Only buff the player's main party, not prisoners
            if (isPrisoners) return result;

            // When party is null, the engine is calculating for the player's main party
            MobileParty mobileParty = party?.MobileParty ?? MobileParty.MainParty;
            if (mobileParty == null) return result;

            float channellerPercent = WoTChannellerUtils.GetChannellerPercentage(mobileParty);
            if (channellerPercent > 0f)
                result.AddFactor(channellerPercent, new TextObject("One Power Healing"));
            return result;
        }

        private float GetChannellerPercentage(MobileParty party)
        {
            return WoTChannellerUtils.GetChannellerPercentage(party);
        }
    }
}
