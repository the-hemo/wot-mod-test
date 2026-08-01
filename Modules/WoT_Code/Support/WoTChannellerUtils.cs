using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;

namespace WoT_Code.Support
{
    public static class WoTChannellerUtils
    {
        public static readonly HashSet<string> ChannellerIds = new HashSet<string>
    {
        "AesSedai2", "AesSedai3", "AesSedai4", "AesSedai5",  "KIN2", "KIN3",
            "Ashaman2", "Ashaman3", "Ashaman4", "Ashaman5a", "Ashaman5b", "Ashaman5c",
            "Dreadlord1", "Dreadlord2", "Dreadlord3", "DreadlordAiel1", "DreadlordAiel2", "DreadlordAiel3", 
            "AielWiseone1", "AielWiseone2",  "seafolkwind2",  "seafolkwind3",  "seafolkwind4",
            "Damane2", "Damane3", "Damane4", "Damane5", "AesSedai_Red1", "AesSedai_Red2", "AesSedai_Red3"
    };

        public static float GetChannellerPercentage(MobileParty party)
        {
            if (party?.MemberRoster == null || party.MemberRoster.TotalManCount == 0)
                return 0f;

            int channellerCount = 0;
            foreach (TroopRosterElement element in party.MemberRoster.GetTroopRoster())
            {
                if (ChannellerIds.Contains(element.Character?.StringId))
                    channellerCount += element.Number;
            }

            return (float)channellerCount / party.MemberRoster.TotalManCount;
        }
    }
}
