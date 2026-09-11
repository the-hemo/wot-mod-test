using HarmonyLib;
using System;
using System.IO;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Election;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;

namespace WoT_Code.HarmonyPatches
{
    [HarmonyPatch(typeof(SettlementClaimantDecision), nameof(SettlementClaimantDecision.CalculateMeritOfOutcome))]
    internal class SettlementClaimantMeritPatch
    {
        private const float StrengthDivisor = 10f;
        private const float RulerBonus = 30f;
        private const float FiefCountPenaltyPerFief = 0.1f; // multiplicative on denominator

        private static float GetLowFiefBonus(int existingFortificationCount)
        {
            switch (existingFortificationCount)
            {
                case 0: return 120f;
                case 1: return 60f;
                case 2: return 30f;
                default: return 0f;
            }
        }

        [HarmonyPrefix]
        private static bool Prefix(SettlementClaimantDecision __instance, DecisionOutcome candidateOutcome, ref float __result)
        {
            SettlementClaimantDecision.ClanAsDecisionOutcome clanOutcome =
                (SettlementClaimantDecision.ClanAsDecisionOutcome)candidateOutcome;
            Clan clan = clanOutcome.Clan;
            Settlement targetSettlement = __instance.Settlement;

            float existingFortificationValue = 0f;
            int existingFortificationCount = 0;
            float closestDistance = Campaign.MapDiagonal + 1f;
            float secondClosestDistance = Campaign.MapDiagonal + 1f;

            foreach (Settlement settlement in Settlement.All)
            {
                if (settlement.OwnerClan == clan && settlement.IsFortification && targetSettlement != settlement)
                {
                    existingFortificationValue += settlement.GetSettlementValueForFaction(clan.Kingdom);

                    float distance = Campaign.Current.Models.MapDistanceModel.GetDistance(
                        settlement, targetSettlement, false, false, MobileParty.NavigationType.All);

                    if (distance < secondClosestDistance)
                    {
                        if (distance < closestDistance)
                        {
                            secondClosestDistance = closestDistance;
                            closestDistance = distance;
                        }
                        else if (distance < secondClosestDistance)
                        {
                            secondClosestDistance = distance;
                        }
                    }
                    existingFortificationCount++;
                }
            }

            // --- travel-time factor: unchanged from vanilla, kept intact per design decision ---
            float baseline = 1f;
            float floor = baseline * 0.25f;
            float travelDistance = baseline;
            if (secondClosestDistance < Campaign.MapDiagonal)
                travelDistance = (secondClosestDistance + closestDistance) / 2f;
            else if (closestDistance < Campaign.MapDiagonal)
                travelDistance = closestDistance;

            float travelDays = travelDistance / (Campaign.Current.EstimatedAverageLordPartySpeed * (float)CampaignTime.HoursInDay);
            float proximityFactor = MathF.Pow(
                baseline / MathF.Max(floor, MathF.Min(2.5f, travelDays)),
                Campaign.Current.GetAverageDistanceBetweenClosestTwoTownsWithNavigationType(MobileParty.NavigationType.All) * 0.0076f);

            // --- strength term, reduced divisor ---
            float strengthScore = clan.CurrentTotalStrength;
            if (targetSettlement.OwnerClan == clan && targetSettlement.Town != null && targetSettlement.Town.GarrisonParty != null)
            {
                strengthScore -= targetSettlement.Town.GarrisonParty.Party.CalculateCurrentStrength();
                if (strengthScore < 0f) strengthScore = 0f;
            }

            float targetSettlementValue = targetSettlement.GetSettlementValueForFaction(clan.Kingdom);
            bool isRuler = clan.Leader == clan.Kingdom.Leader;

            float lowFiefBonus = GetLowFiefBonus(existingFortificationCount);
            float rulerBonus = isRuler ? RulerBonus : 0f;
            float lastCapturedBonus = (targetSettlement.Town != null && targetSettlement.Town.LastCapturedBy == clan) ? 30f : 0f;
            float playerClanBonus = (clan.Leader == Hero.MainHero) ? 30f : 0f;
            float povertyBonus = (clan.Leader.Gold < 30000) ? MathF.Min(30f, 30f - (float)clan.Leader.Gold / 1000f) : 0f;
            float tierScore = (float)clan.Tier * 30f;

            float numerator = tierScore + strengthScore / StrengthDivisor + lowFiefBonus + lastCapturedBonus + rulerBonus + povertyBonus + playerClanBonus;
            float denominator = (existingFortificationValue + targetSettlementValue) * (1f + FiefCountPenaltyPerFief * existingFortificationCount);

            float merit = numerator / denominator * proximityFactor * 200000f;
            /*
            try
            {
                string line = string.Join(",",
                    CampaignTime.Now.ToString(), targetSettlement.Name.ToString(), clan.Name.ToString(),
                    clan.Tier, strengthScore.ToString("F0"), existingFortificationCount,
                    lowFiefBonus, rulerBonus, povertyBonus, playerClanBonus,
                    proximityFactor.ToString("F3"), denominator.ToString("F0"), merit.ToString("F1"));
                File.AppendAllText(BasePath.Name + "wot_merit_log.csv", line + Environment.NewLine);
            }
            catch { logging must never break the decision }
            */
            __result = merit;
            return false;
        }
    }
}
