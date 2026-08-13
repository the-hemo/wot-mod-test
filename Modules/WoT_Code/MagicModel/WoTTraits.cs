using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace WoT_Code.MagicModel

// query this with: hero.GetTraitLevel(WoTTraits.CanChannel) > 0
{
    public class WoTTraits
        
    {
        
        private static WoTTraits current;
        private TraitObject canChannel;

        public static TraitObject CanChannel => current.canChannel;

        public WoTTraits()
        {
            current = this;
            canChannel = Game.Current.ObjectManager.RegisterPresumedObject(new TraitObject("can_channel"));
            canChannel.Initialize(
                new TextObject("{=!}Channeller"),
                new TextObject("{=!}This person has the ability to channel."),
                false,  // isHidden — false means it shows in character screen
                -2,      // minValue - maybe males are negative and females are positive?  0 = no ability,
                2);     // maxValue — binary 0/1 flag
        }
    }
}
