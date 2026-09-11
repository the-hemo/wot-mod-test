// =====================================================================
// Feature: When a clan becomes kingdomless (kingdom destroyed), instead of
// disappearing after the vanilla 28-day timer, clans check their relations
// with existing kingdoms after a 7-day grace period and join accordingly.
//
// This gives the player a 7-day window to recruit the clan first via the
// KingdomlessRecruitBehavior conversation flow before the AI takes over.
//
// Relation thresholds (tweakable via constants below):
//   >= 20  : auto join as vassal (full kingdom membership)
//   >= 0   : join as mercenary
//   <  0   : no auto join — left to vanilla 28-day timer
//
// If multiple kingdoms qualify, the one with the highest relation wins.
// =====================================================================

using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace WoT_Code
{
    public class KingdomlessClanSurvivalBehavior : CampaignBehaviorBase
    {
        // =====================================================================
        // Tuning constants — adjust these to change balance
        // =====================================================================

        // Relation threshold to join as a full vassal
        private const int VassalRelationThreshold = 20;

        // Relation threshold to join as a mercenary (lower bar)
        private const int MercenaryRelationThreshold = 0;

        // Days before the AI relation check fires — gives player time to act
        private const float GracePeriodDays = 7f;

        // =====================================================================
        // State — tracks kingdomless clans and when their grace period expires
        // Persisted via SyncData so it survives save/load
        // =====================================================================

        [SaveableFieldAttribute(1)]
        private Dictionary<Clan, CampaignTime> _pendingClans =
            new Dictionary<Clan, CampaignTime>();

        // =====================================================================
        // CampaignBehaviorBase
        // =====================================================================

        public override void RegisterEvents()
        {
            CampaignEvents.OnClanChangedKingdomEvent.AddNonSerializedListener(
                this, OnClanChangedKingdom);
            CampaignEvents.DailyTickClanEvent.AddNonSerializedListener(
                this, OnDailyTickClan);
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("WoT_KingdomlessClanPending", ref _pendingClans);
        }

        // =====================================================================
        // Event: clan becomes kingdomless — start the grace period timer
        // =====================================================================

        private void OnClanChangedKingdom(
            Clan clan,
            Kingdom oldKingdom,
            Kingdom newKingdom,
            ChangeKingdomAction.ChangeKingdomActionDetail detail,
            bool showNotification)
        {
            // Only care about clans that just LOST their kingdom
            if (newKingdom != null) return;
            if (clan == Clan.PlayerClan) return;
            if (clan.IsEliminated) return;
            if (clan.IsBanditFaction) return;
            if (clan.IsClanTypeMercenary) return;
            if (clan.IsMinorFaction) return;
            if (clan.IsRebelClan) return;
            if (clan.Leader == null) return;

            // Start the 3-day grace period
            CampaignTime graceExpiry = CampaignTime.Now + CampaignTime.Days(GracePeriodDays);
            _pendingClans[clan] = graceExpiry;

            InformationManager.DisplayMessage(new InformationMessage(
                $"{clan.Name} is without a kingdom. They will seek new allegiances in {(int)GracePeriodDays} days.",
                Color.FromUint(0xFFAA00FF)));
        }

        // =====================================================================
        // Event: daily tick — check grace periods and resolve pending clans
        // =====================================================================

        private void OnDailyTickClan(Clan clan)
        {
            if (!_pendingClans.TryGetValue(clan, out CampaignTime graceExpiry))
                return;

            // Not yet — grace period still active, player can still recruit
            if (CampaignTime.Now < graceExpiry)
                return;

            // Grace period expired — remove from pending and resolve
            _pendingClans.Remove(clan);

            // Skip if clan was already recruited by player or eliminated
            if (clan.Kingdom != null) return;
            if (clan.IsEliminated) return;
            if (clan.Leader == null) return;

            ResolveClanPlacement(clan);
        }

        // =====================================================================
        // Core logic — find the best kingdom for this clan based on relations
        // =====================================================================

        private void ResolveClanPlacement(Clan clan)
        {
            Hero clanLeader = clan.Leader;
            Kingdom bestVassalKingdom = null;
            Kingdom bestMercenaryKingdom = null;
            int bestVassalRelation = VassalRelationThreshold - 1;
            int bestMercenaryRelation = MercenaryRelationThreshold - 1;

            foreach (Kingdom kingdom in Kingdom.All)
            {
                // Skip eliminated or player kingdoms
                // (player has had 7 days to recruit — if they haven't, AI decides)
                if (kingdom.IsEliminated) continue;
                if (kingdom.Leader == null) continue;

                int relation = (int)clanLeader.GetRelation(kingdom.Leader);

                // Check for vassal threshold
                if (relation >= VassalRelationThreshold && relation > bestVassalRelation)
                {
                    bestVassalRelation = relation;
                    bestVassalKingdom = kingdom;
                }
                // Check for mercenary threshold (only if no vassal option found)
                else if (relation >= MercenaryRelationThreshold && relation > bestMercenaryRelation)
                {
                    bestMercenaryRelation = relation;
                    bestMercenaryKingdom = kingdom;
                }
            }

            if (bestVassalKingdom != null)
            {
                // Strong relation — join as full vassal
                ChangeKingdomAction.ApplyByJoinToKingdom(clan, bestVassalKingdom);

                InformationManager.DisplayMessage(new InformationMessage(
                    $"{clan.Name} has pledged vassalage to {bestVassalKingdom.Name} " +
                    $"based on their existing relationship.",
                    Color.FromUint(0x00AA00FF)));
            }
            else if (bestMercenaryKingdom != null)
            {
                // Moderate relation — join as mercenary
                ChangeKingdomAction.ApplyByJoinFactionAsMercenary(clan, bestMercenaryKingdom);

                InformationManager.DisplayMessage(new InformationMessage(
                    $"{clan.Name} has taken mercenary service with {bestMercenaryKingdom.Name}.",
                    Color.FromUint(0xAAAA00FF)));
            }
            else
            {
                // No suitable kingdom found — notify player that clan will fade
                InformationManager.DisplayMessage(new InformationMessage(
                    $"{clan.Name} found no allies and will fade from the world.",
                    Color.FromUint(0xFF4444FF)));

                // Vanilla 28-day timer takes over from here
                // No action needed — the engine handles dissolution naturally
            }
        }

        // =====================================================================
        // Saveable type definer — required for Dictionary<Clan, CampaignTime>
        // to persist correctly across saves
        // =====================================================================

        public class KingdomlessClanSurvivalTypeDefiner : SaveableTypeDefiner
        {
            public KingdomlessClanSurvivalTypeDefiner() : base(445832917) { }

            protected override void DefineClassTypes()
            {
                // No class types needed — Clan and CampaignTime are
                // already known to the save system
            }

            protected override void DefineContainerDefinitions()
            {
                // Register the Dictionary type for SyncData
                ConstructContainerDefinition(
                    typeof(Dictionary<Clan, CampaignTime>));
            }
        }
    }
}

