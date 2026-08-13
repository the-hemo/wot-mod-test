using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Incidents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;
using WoT_Code.Support;

namespace WoT_Code.Behaviours
{
    public class WoTLoreIncidents : CampaignBehaviorBase
    {
        // =====================================================================
        // CampaignBehaviorBase
        // =====================================================================

        public override void RegisterEvents()
        {
            // Both hooks needed — presumed objects registered on a new game
            // won't exist after loading a save, so re-register on load too.
            CampaignEvents.OnNewGameCreatedEvent.AddNonSerializedListener(
                this, new Action<CampaignGameStarter>(OnNewGameCreated));
            CampaignEvents.OnGameLoadFinishedEvent.AddNonSerializedListener(
                this, new Action(InitializeIncidents));
        }

        public override void SyncData(IDataStore dataStore) { }

        private void OnNewGameCreated(CampaignGameStarter starter)
        {
            InitializeIncidents();
        }

        // =====================================================================
        // Registration helper — mirrors vanilla's private RegisterIncident,
        // just callable from our own assembly instead of TaleWorlds'.
        // =====================================================================

        private Incident RegisterIncident(
            string id,
            string title,
            string description,
            IncidentsCampaignBehaviour.IncidentTrigger trigger,
            IncidentsCampaignBehaviour.IncidentType type,
            CampaignTime cooldown,
            Func<TextObject, bool> condition)
        {
            Incident incident = Game.Current.ObjectManager
                .RegisterPresumedObject<Incident>(new Incident(id));
            incident.Initialize(title, description, trigger, type, cooldown, condition);
            return incident;
        }

        // =====================================================================
        // Incident definitions
        // =====================================================================

        private void InitializeIncidents()
        {
            // ---------------------------------------------------------------
            // "A Troubled Dream" — fires leaving a village. Only surfaces if
            // the party contains at least one troop carrying can_channel at
            // a meaningful level, using the same trait lookup pattern as
            // WoTChannellerUtils.GetChannellerPercentage.
            // ---------------------------------------------------------------
            Incident dreamIncident = RegisterIncident(
                "wot_incident_troubled_dream",
                "{=wot001}A Troubled Dream",
                "{=wot002}One of your channellers wakes the camp before dawn, shaking and pale. " +
                "{PRONOUN} speaks of a dream thick with the scent of lightning — wheels within " +
                "wheels, and a shadow that knew {PRONOUN2} name. The others are unsettled; some " +
                "call it an ill omen, others a sign that the Pattern has marked this one for " +
                "something greater.",
                IncidentsCampaignBehaviour.IncidentTrigger.LeavingVillage,
                IncidentsCampaignBehaviour.IncidentType.DreamsSongsAndSigns,
                CampaignTime.Days(45f),
                delegate (TextObject description)
                {
                    // Condition delegate — gates whether the incident can fire
                    // AND lets us populate text variables for the description
                    // before it's shown, same as vanilla's RANK/PRONOUN usage.
                    bool hasChanneller = PartyHasChanneller(MobileParty.MainParty);
                    if (hasChanneller)
                    {
                        description.SetTextVariable("PRONOUN", "she");
                        description.SetTextVariable("PRONOUN2", "her");
                    }
                    return hasChanneller;
                });

            dreamIncident.AddOption(
                "{=wot003}Comfort the channeller and treat the dream as a sign to be heeded",
                new List<IncidentEffect>
                {
                    IncidentEffect.TraitChange(DefaultTraits.Mercy, 100),
                    IncidentEffect.MoraleChange(5f)
                },
                null, null);

            dreamIncident.AddOption(
                "{=wot004}Dismiss it as nerves and order the camp back to sleep",
                new List<IncidentEffect>
                {
                    IncidentEffect.TraitChange(DefaultTraits.Calculating, 50),
                    IncidentEffect.MoraleChange(-5f).WithChance(0.5f)
                },
                null, null);

            dreamIncident.AddOption(
                "{=wot005}Pay the channeller privately to keep quiet about what she saw",
                new List<IncidentEffect>
                {
                    IncidentEffect.GoldChange(() => -50),
                    IncidentEffect.TraitChange(DefaultTraits.Honor, -50)
                },
                null,null); 
            // Consequence delegate — for anything IncidentEffect's vocabulary
            // doesn't cover. Runs after effects are applied. This is where
            // you'd hook something WoT-specific that has no vanilla equivalent
            // (e.g. nudging a custom reputation tracker, if you build one).
            //delegate (TextObject resultText)
            {
                    // Example placeholder — replace with real WoT-specific logic
                    // once you have something to hook here.
                 //   return true;
                }
        }

        // =====================================================================
        // Helper — reuses the same ChannellerIds registry you already have
        // in WoTChannellerUtils rather than duplicating trait lookup logic.
        // =====================================================================

        private bool PartyHasChanneller(MobileParty party)
        {
            return WoTChannellerUtils.GetChannellerPercentage(party) > 0f;
        }
    }
}
