using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using static TaleWorlds.CampaignSystem.CharacterObject;
using static TaleWorlds.CampaignSystem.Hero;

namespace WoT_Code.MagicModel
{
    public class ChannellerComponent : HeroObject 
    {
        // Core flag — mirrors the trait for internal mod use
        public bool IsChanneller { get; set; }

        // =========================================================
        // Rich magic data — populate as the system develops
        // =========================================================

        // Which half of the One Power they use
        // "saidin" (male), "saidar" (female), "true_power" (Forsaken)
        public string SourceOfPower { get; set; }

        // Raw channelling strength 0-100 — drives damage/effect scaling
        public int ChannellingStrength { get; set; }

        // Affinity — primary element this channeller is strongest with
        // "fire", "wind", "earth", "water", "spirit"
        public string ElementAffinity { get; set; }

        // Whether they've been stilled (female) or gentled (male)
        // A stilled/gentled channeller has IsChanneller = true but CanChannel = false
        public bool IsStilled { get; set; }
        public bool IsGentled { get; set; }

        // Convenience property — can they actually channel right now
        public bool CanChannel => IsChanneller && !IsStilled && !IsGentled;

        // Ajah or rank — "red", "blue", "green" etc. for Aes Sedai
        // "soldier", "dedicated", "asha'man" for male channellers
        public string Rank { get; set; }

        // Placeholder for future skill-like progression within the magic system
        public int WeavingExperience { get; set; }
    }
}
