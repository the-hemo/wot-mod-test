using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace WoT_Code
{
    public class WoTLordConversations : CampaignBehaviorBase
    {
        // =====================================================================
        // CampaignBehaviorBase
        // =====================================================================

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(
                this, OnSessionLaunched);
        }

        public override void SyncData(IDataStore dataStore) { }

        private void OnSessionLaunched(CampaignGameStarter starter)
        {
            AddWoTLordConversationsDialogs(starter);
        }

        private void AddWoTLordConversationsDialogs(CampaignGameStarter starter)
        {
            // ── Player offer line ─────────────────────────────────────────────
            starter.AddPlayerLine(
                "wot_want_to_end_mercenary_service",
                "lord_talk_speak_diplomacy_2",
                "lord_ask_exit_service",
                "{=wot_end_mercenary}I would like to end my contract with the {SERVED_FACTION}.",
                new ConversationSentence.OnConditionDelegate(this.conversation_player_want_to_end_service_as_mercenary_on_condition),
                null,
                110); // above vanilla's 100
        }

        // =====================================================================
        // swapping out Culture Formal Name for Kingdom Adjective
        // =====================================================================

        public bool conversation_player_want_to_end_service_as_mercenary_on_condition()
        {
            if (Hero.OneToOneConversationHero.MapFaction == null || Hero.OneToOneConversationHero.Clan == null || MobileParty.MainParty.Army != null)
            {
                return false;
            }
            if (Hero.OneToOneConversationHero.MapFaction.IsKingdomFaction)
            {
                MBTextManager.SetTextVariable("SERVED_FACTION", FactionHelper.GetAdjectiveForFaction(Hero.OneToOneConversationHero.Clan.Kingdom), false);
            }
            return !Hero.OneToOneConversationHero.IsPrisoner && Hero.MainHero.MapFaction == Hero.OneToOneConversationHero.MapFaction && Hero.OneToOneConversationHero.Clan != Hero.MainHero.Clan && Hero.MainHero.Clan.IsUnderMercenaryService;
        }
    }
}
