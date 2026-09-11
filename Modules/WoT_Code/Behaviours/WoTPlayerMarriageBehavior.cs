using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace WoT_Code.Behaviours
{
    public class WoTPlayerMarriageBehavior : CampaignBehaviorBase
    {
        private MethodInfo _canOfferMarriageForClanMethod;
        private MethodInfo _considerMarriageMethod;
        private MarriageOfferCampaignBehavior _vanillaBehavior;

        public override void RegisterEvents()
        {
            CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this, OnGameLoaded);
            CampaignEvents.DailyTickClanEvent.AddNonSerializedListener(this, OnDailyTickClan);
        }

        public override void SyncData(IDataStore dataStore) { }

        private void OnGameLoaded(CampaignGameStarter starter)
        {
            _vanillaBehavior = Campaign.Current.GetCampaignBehavior<MarriageOfferCampaignBehavior>();
            _canOfferMarriageForClanMethod = typeof(MarriageOfferCampaignBehavior)
                .GetMethod("CanOfferMarriageForClan", BindingFlags.Instance | BindingFlags.NonPublic);
            _considerMarriageMethod = typeof(MarriageOfferCampaignBehavior)
                .GetMethod("ConsiderMarriageForPlayerClanMember", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        private void OnDailyTickClan(Clan consideringClan)
        {
            if (_vanillaBehavior == null || _canOfferMarriageForClanMethod == null || _considerMarriageMethod == null)
            {
                _vanillaBehavior = Campaign.Current.GetCampaignBehavior<MarriageOfferCampaignBehavior>();
                _canOfferMarriageForClanMethod = typeof(MarriageOfferCampaignBehavior)
                    .GetMethod("CanOfferMarriageForClan", BindingFlags.Instance | BindingFlags.NonPublic);
                _considerMarriageMethod = typeof(MarriageOfferCampaignBehavior)
                    .GetMethod("ConsiderMarriageForPlayerClanMember", BindingFlags.Instance | BindingFlags.NonPublic);

                if (_vanillaBehavior == null || _canOfferMarriageForClanMethod == null || _considerMarriageMethod == null)
                    return; // bail safely instead of throwing crash
            }



            if (consideringClan.Culture?.DefaultPartyTemplate == null)
            {
                //InformationManager.DisplayMessage(new InformationMessage(
                //    $"[WoT] Skipping marriage tick for {consideringClan.StringId} — no DefaultPartyTemplate (culture: {consideringClan.Culture?.StringId ?? "null"}).",
                //    Color.FromUint(0xFF0000FF)));
                // Remove log once i playtest as im sure the error was my removal of a default party template
                return;
            }

            if (!Hero.MainHero.CanMarry()) return;
            if (consideringClan == Clan.PlayerClan) return;

            bool canOffer = (bool)_canOfferMarriageForClanMethod.Invoke(_vanillaBehavior, new object[] { consideringClan });
            if (!canOffer) return;

            bool hasNaval = (consideringClan.DefaultPartyTemplate?.ShipHulls?.Count ?? 0) > 0;
            MobileParty.NavigationType navType = hasNaval
                ? MobileParty.NavigationType.All
                : MobileParty.NavigationType.Default;

            float distance = Campaign.Current.Models.MapDistanceModel.GetDistance(
                Clan.PlayerClan.FactionMidSettlement, consideringClan.FactionMidSettlement, false, false, navType);
            float maxDistance = Campaign.Current.Models.MapDistanceModel.GetMaximumDistanceBetweenTwoConnectedSettlements(navType);
            if (MBRandom.RandomFloat < distance / maxDistance - 0.5f) return;

            _considerMarriageMethod.Invoke(_vanillaBehavior, new object[] { Hero.MainHero, consideringClan });
        }
    }
}
