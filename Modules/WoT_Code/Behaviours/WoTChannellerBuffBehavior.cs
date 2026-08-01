using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using WoT_Code.Support;

namespace WoT_Code.Behaviours
{
    public class WoTPartyImpairmentModel : DefaultPartyImpairmentModel
    {
        public override ExplainedNumber GetDisorganizedStateDuration(MobileParty party)
        {
            ExplainedNumber result = base.GetDisorganizedStateDuration(party);

            float channellerPercent = GetChannellerPercentage(party);
            if (channellerPercent > 0f)
            {
                float reduction = -channellerPercent * 1f;
                result.AddFactor(reduction, new TextObject("One Power Support"));
            }

            return result;
        }

        private float GetChannellerPercentage(MobileParty party)
        {
            return WoTChannellerUtils.GetChannellerPercentage(party);
        }

    }
}
