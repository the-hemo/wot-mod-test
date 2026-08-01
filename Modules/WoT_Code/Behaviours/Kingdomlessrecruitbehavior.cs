// KingdomlessRecruitBehavior.cs
// Namespace: WoT_Code
//
// Feature: Allows the player to recruit kingdomless clan leaders into their
// kingdom via a new conversation option on hero_main_options.
//
// Vanilla blocks this path because LordDefectionCampaignBehavior's persuasion
// flow requires the target clan to be in a kingdom to defect from. Kingdomless
// clans after a kingdom destruction have no such path available to the player.
//
// This behavior adds a direct offer conversation with three possible NPC
// responses based on the clan leader's relation with the player:
//   - Positive relation (>= 0)  : accepts immediately
//   - Neutral relation (-20..0) : hesitates but accepts
//   - Negative relation (< -20) : declines with a hostile response
//
// Registration: In your existing SubModule.OnGameStart(), call:
//   campaignGameStarter.AddBehavior(new KingdomlessRecruitBehavior());

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace WoT_Code
{
    public class KingdomlessRecruitBehavior : CampaignBehaviorBase
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
            AddKingdomlessRecruitDialogs(starter);
        }

        // =====================================================================
        // Dialog flow
        //
        // Flow chart:
        //   hero_main_options
        //     → [player line] "Your kingdom has fallen. Join my cause."
        //         → wot_lordless_offer (branches on relation)
        //             → wot_lordless_accept       (relation >= 0)
        //             → wot_lordless_hesitate      (relation >= -20)
        //             → wot_lordless_decline       (relation < -20)
        //                 → wot_lordless_decline_end
        // =====================================================================

        private void AddKingdomlessRecruitDialogs(CampaignGameStarter starter)
        {
            // ── Player offer line ─────────────────────────────────────────────
            starter.AddPlayerLine(
                "wot_offer_kingdom_to_lordless",
                "hero_main_options",
                "wot_lordless_offer",
                "{=wot_offer_kingdom}Your kingdom has fallen. Join my cause and your clan will have a place in my kingdom.",
                OfferCondition,
                null,
                110); // priority above vanilla hero_main_options lines

            // ── NPC response: accepts (positive relation) ─────────────────────
            starter.AddDialogLine(
                "wot_lordless_accept",
                "wot_lordless_offer",
                "close_window",
                "{=wot_lordless_accept}You have shown yourself a worthy leader. My clan will follow your banner. We are yours.",
                () => GetConversationRelation() >= 0,
                AcceptConsequence);

            // ── NPC response: hesitates but accepts (neutral relation) ─────────
            starter.AddDialogLine(
                "wot_lordless_hesitate",
                "wot_lordless_offer",
                "close_window",
                "{=wot_lordless_hesitate}These are desperate times. My clan needs a home and you offer one. Very well — we will ride under your banner, for now.",
                () => GetConversationRelation() >= -20,
                AcceptConsequence);

            // ── NPC response: declines (negative relation) ────────────────────
            starter.AddDialogLine(
                "wot_lordless_decline",
                "wot_lordless_offer",
                "wot_lordless_decline_end",
                "{=wot_lordless_decline}You think I would bend the knee to you? My clan has lost much, but not our pride. We will find our own way. Leave us.",
                () => GetConversationRelation() < -20,
                null);

            // ── Player exit line after decline ────────────────────────────────
            starter.AddPlayerLine(
                "wot_lordless_decline_exit",
                "wot_lordless_decline_end",
                "close_window",
                "{=wot_lordless_decline_exit}As you wish. The offer stands if you change your mind.",
                null,
                null,
                100);
        }

        // =====================================================================
        // Condition — when to show the offer option
        // =====================================================================

        private bool OfferCondition()
        {
            Hero conversationHero = Hero.OneToOneConversationHero;
            if (conversationHero == null) return false;

            Clan clan = conversationHero.Clan;
            if (clan == null) return false;

            // Target must be kingdomless
            if (clan.Kingdom != null) return false;

            // Target must not be the player's own clan
            if (clan == Clan.PlayerClan) return false;

            // Target clan must not be eliminated
            if (clan.IsEliminated) return false;

            // Target must not be a bandit clan
            if (clan.IsBanditFaction) return false;

            // Conversation hero must be the clan leader
            if (conversationHero != clan.Leader) return false;

            // Player must have a kingdom to offer
            if (Clan.PlayerClan.Kingdom == null) return false;

            // Player must not be a mercenary
            if (Clan.PlayerClan.IsUnderMercenaryService) return false;

            return true;
        }

        // =====================================================================
        // Consequence — apply kingdom join action
        // =====================================================================

        private void AcceptConsequence()
        {
            Clan clan = Hero.OneToOneConversationHero?.Clan;
            if (clan == null) return;

            Kingdom playerKingdom = Clan.PlayerClan.Kingdom;
            if (playerKingdom == null) return;

            ChangeKingdomAction.ApplyByJoinToKingdom(clan, playerKingdom);

            InformationManager.DisplayMessage(new InformationMessage(
                $"{clan.Name} has joined your kingdom.",
                TaleWorlds.Library.Color.FromUint(0x00AA00FF)));
        }

        // =====================================================================
        // Helper — gets relation between player and conversation hero
        // =====================================================================

        private int GetConversationRelation()
        {
            Hero conversationHero = Hero.OneToOneConversationHero;
            if (conversationHero == null) return 0;
            return (int)Hero.MainHero.GetRelation(conversationHero);
        }
    }
}
