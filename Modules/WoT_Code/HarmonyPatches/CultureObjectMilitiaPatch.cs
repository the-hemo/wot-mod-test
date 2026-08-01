using System;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace WoT_Code.HarmonyPatches
{
    /*
     * This patch is not working with 1.3.nn.  The idea was to minimise cultures, but had to reinstate white and black towers to avoid errors.  The patch is left here for reference and potential future use if the militia system is reworked.
    [HarmonyPatch(typeof(Settlement), "AddMilitiasToParty", new Type[] { typeof(MobileParty), typeof(int) })]
    internal class MilitiaSpawnPatch
    {
        // Token: 0x060000A7 RID: 167 RVA: 0x00007450 File Offset: 0x00005650
        private static bool Prefix(Settlement __instance, MobileParty militiaParty, int militiaNumberToAdd)
        {
            bool flag6 = __instance == null;
            string text;
            if (flag6)
            {
                text = null;
            }
            else
            {
                TextObject name = __instance.Name;
                text = ((name != null) ? name.ToString() : null);
            }
            string text4 = text ?? "Unknown Settlement";
            bool flag = __instance == null || __instance.OwnerClan == null;
            bool flag7 = flag;
            bool result;
            if (flag7)
            {
                result = true;
            }
            else
            {
                bool flag2 = __instance.Culture.StringId != "empire";
                bool flag8 = flag2;
                if (flag8)
                {
                    result = true;
                }
                else
                {
                    Kingdom kingdom = __instance.OwnerClan.Kingdom;
                    bool flag3 = kingdom == null;
                    bool flag9 = flag3;
                    if (flag9)
                    {
                        result = true;
                    }
                    else
                    {
                        string kingdomId = kingdom.StringId;
                        string text2 = kingdomId;
                        string a = text2;
                        bool flag10 = a == "TheWhiteTower";
                        CharacterObject BaseMilitiaTroopTemplate;
                        CharacterObject EliteBaseMilitiaTroopTemplate;
                        CharacterObject RangeMilitiaTroopTemplate;
                        CharacterObject EliteRangeMilitiaTroopTemplate;
                        if (flag10)
                        {
                            BaseMilitiaTroopTemplate = Game.Current.ObjectManager.GetObject<CharacterObject>("WhiteTower_militia1");
                            EliteBaseMilitiaTroopTemplate = Game.Current.ObjectManager.GetObject<CharacterObject>("WhiteTower_militia3");
                            RangeMilitiaTroopTemplate = Game.Current.ObjectManager.GetObject<CharacterObject>("AesSedai3");
                            EliteRangeMilitiaTroopTemplate = Game.Current.ObjectManager.GetObject<CharacterObject>("AesSedai5");
                        }
                        else
                        {
                            bool flag11 = a == "TheBlackTower";
                            if (flag11)
                            {
                                BaseMilitiaTroopTemplate = Game.Current.ObjectManager.GetObject<CharacterObject>("Ashaman1");
                                EliteBaseMilitiaTroopTemplate = Game.Current.ObjectManager.GetObject<CharacterObject>("Ashaman3");
                                RangeMilitiaTroopTemplate = Game.Current.ObjectManager.GetObject<CharacterObject>("Ashaman2");
                                EliteRangeMilitiaTroopTemplate = Game.Current.ObjectManager.GetObject<CharacterObject>("Ashaman4");
                            }
                            else
                            {
                                BaseMilitiaTroopTemplate = Game.Current.ObjectManager.GetObject<CharacterObject>("Militia1");
                                EliteBaseMilitiaTroopTemplate = Game.Current.ObjectManager.GetObject<CharacterObject>("Militia3");
                                RangeMilitiaTroopTemplate = Game.Current.ObjectManager.GetObject<CharacterObject>("MilitiaBow1");
                                EliteRangeMilitiaTroopTemplate = Game.Current.ObjectManager.GetObject<CharacterObject>("MilitiaBow2");
                            }
                        }
                        bool flag4 = BaseMilitiaTroopTemplate == null;
                        bool flag12 = flag4;
                        if (flag12)
                        {
                            string text3 = kingdomId;
                            string a2 = text3;
                            bool flag13 = a2 == "TheWhiteTower";
                            if (flag13)
                            {
                                BaseMilitiaTroopTemplate = Game.Current.ObjectManager.GetObject<CharacterObject>("WhiteTower_militia1");
                                EliteBaseMilitiaTroopTemplate = Game.Current.ObjectManager.GetObject<CharacterObject>("WhiteTower_militia3");
                                RangeMilitiaTroopTemplate = Game.Current.ObjectManager.GetObject<CharacterObject>("AesSedai3");
                                EliteRangeMilitiaTroopTemplate = Game.Current.ObjectManager.GetObject<CharacterObject>("AesSedai5");
                            }
                            else
                            {
                                bool flag14 = a2 == "TheBlackTower";
                                if (flag14)
                                {
                                    BaseMilitiaTroopTemplate = Game.Current.ObjectManager.GetObject<CharacterObject>("Ashaman1");
                                    EliteBaseMilitiaTroopTemplate = Game.Current.ObjectManager.GetObject<CharacterObject>("Ashaman3");
                                    RangeMilitiaTroopTemplate = Game.Current.ObjectManager.GetObject<CharacterObject>("Ashaman2");
                                    EliteRangeMilitiaTroopTemplate = Game.Current.ObjectManager.GetObject<CharacterObject>("Ashaman4");
                                }
                                else
                                {
                                    BaseMilitiaTroopTemplate = Game.Current.ObjectManager.GetObject<CharacterObject>("Militia1");
                                    EliteBaseMilitiaTroopTemplate = Game.Current.ObjectManager.GetObject<CharacterObject>("Militia3");
                                    RangeMilitiaTroopTemplate = Game.Current.ObjectManager.GetObject<CharacterObject>("MilitiaBow1");
                                    EliteRangeMilitiaTroopTemplate = Game.Current.ObjectManager.GetObject<CharacterObject>("MilitiaBow2");
                                }
                            }
                            bool flag5 = BaseMilitiaTroopTemplate != null;
                            bool flag15 = !flag5;
                            if (flag15)
                            {
                                return true;
                            }
                        }
                        float troopRatio;
                        float num;
                        Campaign.Current.Models.SettlementMilitiaModel.CalculateMilitiaSpawnRate(__instance, out troopRatio, out num);

                        // Call AddTroopToMilitiaParty with a ref for the remaining count (matches original logic)
                        MilitiaSpawnPatch.AddTroopToMilitiaParty(__instance, militiaParty, BaseMilitiaTroopTemplate, EliteBaseMilitiaTroopTemplate, troopRatio, ref militiaNumberToAdd);
                        MilitiaSpawnPatch.AddTroopToMilitiaParty(__instance, militiaParty, RangeMilitiaTroopTemplate, EliteRangeMilitiaTroopTemplate, 1f, ref militiaNumberToAdd);
                        result = false;
                    }
                }
            }
            return result;
        }

        // Token: 0x060000A8 RID: 168 RVA: 0x0000781C File Offset: 0x00005A1C
        private static void AddTroopToMilitiaParty(Settlement settlement, MobileParty militiaParty, CharacterObject BaseMilitiaTroopTemplate, CharacterObject eliteMilitiaTroop, float troopRatio, ref int numberToAddRemaining)
        {
            bool flag = numberToAddRemaining > 0;
            if (flag)
            {
                // Calculate number to add as before
                int num = MBRandom.RoundRandomized(troopRatio * (float)numberToAddRemaining);

                // Calculate veteran chance — handle both float and ExplainedNumber return types safely.
                var veteranChanceValue = Campaign.Current.Models.SettlementMilitiaModel.CalculateVeteranMilitiaSpawnChance(settlement);
                float veteranChance;
                // If the API returns an ExplainedNumber (newer versions), use ResultNumber; if it returns float (older), use value directly.
                if (veteranChanceValue is ExplainedNumber en)
                {
                    veteranChance = en.ResultNumber;
                }
                else
                {
                    // Fallback: attempt a safe conversion, clamp to [0,1]
                    try
                    {
                        veteranChance = Convert.ToSingle(veteranChanceValue);
                    }
                    catch
                    {
                        veteranChance = 0f;
                    }
                }

                for (int i = 0; i < num; i++)
                {
                    bool flag2 = MBRandom.RandomFloat < veteranChance;
                    if (flag2)
                    {
                        militiaParty.MemberRoster.AddToCounts(eliteMilitiaTroop, 1, false, 0, 0, true, -1);
                    }
                    else
                    {
                        militiaParty.MemberRoster.AddToCounts(BaseMilitiaTroopTemplate, 1, false, 0, 0, true, -1);
                    }
                }
                numberToAddRemaining -= num;
            }
        }
    }
    */
}
