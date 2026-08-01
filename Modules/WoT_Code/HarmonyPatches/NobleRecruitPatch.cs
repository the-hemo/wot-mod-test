using System;
using System.Collections.Generic;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;

namespace WoT_Code.HarmonyPatches
{
    
    [HarmonyPatch(typeof(RecruitmentCampaignBehavior), "UpdateVolunteersOfNotablesInSettlement")]
    internal class NobleRecruitPatch
    {
        private static void Postfix(Settlement settlement)
        {
            bool flag = settlement == null || settlement.OwnerClan == null;
            if (!flag)
            {
                string flag2 = settlement.Culture.StringId;
                if (flag2 == "khuzait")
                {
                    Kingdom kingdom = settlement.OwnerClan.Kingdom;
                    bool flag3 = kingdom == null;
                    if (!flag3)
                    {
                        string stringId = kingdom.StringId;
                        string a = stringId;
                        CharacterObject rootTroopTemplate;
                        if (!(a == "TheSaldea"))
                        {
                            if (!(a == "TheArafel"))
                            {
                                if (!(a == "TheKandor"))
                                {
                                    if (!(a == "TheSheinar"))
                                    {
                                        return;
                                    }
                                    else
                                    {
                                        rootTroopTemplate = Game.Current.ObjectManager.GetObject<CharacterObject>("Sheinar_noble1");
                                    }
                                }
                                else
                                {
                                    rootTroopTemplate = Game.Current.ObjectManager.GetObject<CharacterObject>("Kandor_noble1");
                                }
                            }
                            else
                            {
                                rootTroopTemplate = Game.Current.ObjectManager.GetObject<CharacterObject>("Arafel_noble1");
                            }
                        }
                        else
                        {
                            rootTroopTemplate = Game.Current.ObjectManager.GetObject<CharacterObject>("Saldea_noble1");
                        }
                        bool flag4 = rootTroopTemplate == null;
                        if (!flag4)
                        {
                            foreach (Hero notable in settlement.Notables)
                            {
                                bool flag5 = !notable.CanHaveRecruits;
                                if (!flag5)
                                {
                                    for (int i = 0; i < 6; i++)
                                    {
                                        bool flag6 = notable.VolunteerTypes[i] != null;
                                        if (flag6)
                                        {
                                            bool isEliteTroop = NobleRecruitPatch.IsEliteTroop(notable.VolunteerTypes[i]);
                                            bool flag7 = isEliteTroop;
                                            if (flag7)
                                            {
                                                CharacterObject replacement = NobleRecruitPatch.GetTroopOfTier(rootTroopTemplate, notable.VolunteerTypes[i].Tier);
                                                bool flag8 = replacement != null;
                                                if (flag8)
                                                {
                                                    notable.VolunteerTypes[i] = replacement;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                if (flag2 == "battania")
                {
                    Kingdom kingdom = settlement.OwnerClan.Kingdom;
                    bool flag3 = kingdom == null;
                    if (!flag3)
                    {
                        string stringId = kingdom.StringId;
                        string a = stringId;
                        CharacterObject rootTroopTemplate;
                        if (!(a == "TheAradDoman"))
                        {
                            if (!(a == "TheIllian"))
                            {
                                if (!(a == "TheAltara"))
                                {
                                    if (!(a == "TheMayene"))
                                    {
                                        if (!(a == "TheTarabon"))
                                        {
                                            if (!(a == "TheTear"))
                                            {
                                                if (!(a == "TheFarMadding"))
                                                {
                                                    return;
                                                }
                                                else
                                                {
                                                    rootTroopTemplate = Game.Current.ObjectManager.GetObject<CharacterObject>("FarMadding_noble1");
                                                }
                                            }
                                            else
                                            {
                                                rootTroopTemplate = Game.Current.ObjectManager.GetObject<CharacterObject>("Tear_noble1");
                                            }
                                        }
                                        else
                                        {
                                            rootTroopTemplate = Game.Current.ObjectManager.GetObject<CharacterObject>("Tarabon_noble1");
                                        }
                                    }
                                    else
                                    {
                                        rootTroopTemplate = Game.Current.ObjectManager.GetObject<CharacterObject>("Mayene_noble1");
                                    }
                                }
                                else
                                {
                                rootTroopTemplate = Game.Current.ObjectManager.GetObject<CharacterObject>("Altara_noble1");
                                }
                            }
                            else
                            {
                                rootTroopTemplate = Game.Current.ObjectManager.GetObject<CharacterObject>("Illian_noble1");
                            }
                        }
                        else
                        {
                            rootTroopTemplate = Game.Current.ObjectManager.GetObject<CharacterObject>("AradDoman_noble1");
                        }
                        bool flag4 = rootTroopTemplate == null;
                        if (!flag4)
                        {
                            foreach (Hero notable in settlement.Notables)
                            {
                                bool flag5 = !notable.CanHaveRecruits;
                                if (!flag5)
                                {
                                    for (int i = 0; i < 6; i++)
                                    {
                                        bool flag6 = notable.VolunteerTypes[i] != null;
                                        if (flag6)
                                        {
                                            bool isEliteTroop = NobleRecruitPatch.IsEliteTroop(notable.VolunteerTypes[i]);
                                            bool flag7 = isEliteTroop;
                                            if (flag7)
                                            {
                                                CharacterObject replacement = NobleRecruitPatch.GetTroopOfTier(rootTroopTemplate, notable.VolunteerTypes[i].Tier);
                                                bool flag8 = replacement != null;
                                                if (flag8)
                                                {
                                                    notable.VolunteerTypes[i] = replacement;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                if (flag2 == "empire")
                {
                    Kingdom kingdom = settlement.OwnerClan.Kingdom;
                    bool flag3 = kingdom == null;
                    if (!flag3)
                    {
                        string stringId = kingdom.StringId;
                        string a = stringId;
                        CharacterObject rootTroopTemplate;
                        if (!(a == "empire"))
                        {
                            if (!(a == "empire_s"))
                            {
                                if (!(a == "TheMurandy"))
                                {
                                    if (!(a == "TheGhealdan"))
                                    {
                                        if (!(a == "TheAmadicia"))
                                        {
                                            if (!(a == "TheBlackTower"))
                                            {
                                                if (!(a == "TheWhiteTower"))
                                                {
                                                    if (!(a == "empire_w"))
                                                    {
                                                        return;
                                                    }
                                                    else
                                                    { 
                                                        rootTroopTemplate = Game.Current.ObjectManager.GetObject<CharacterObject>("Dragonsworn_noble1"); 
                                                    }
                                                }
                                                else
                                                {
                                                    rootTroopTemplate = Game.Current.ObjectManager.GetObject<CharacterObject>("AesSedai1");
                                                }
                                            }
                                            else
                                            {
                                                rootTroopTemplate = Game.Current.ObjectManager.GetObject<CharacterObject>("Ashaman1");
                                            }
                                        }
                                        else
                                        {
                                            rootTroopTemplate = Game.Current.ObjectManager.GetObject<CharacterObject>("Amadicia_noble1");
                                        }
                                    }
                                    else
                                    {
                                        rootTroopTemplate = Game.Current.ObjectManager.GetObject<CharacterObject>("Ghealdan_noble1");
                                    }
                                }
                                else
                                {
                                    rootTroopTemplate = Game.Current.ObjectManager.GetObject<CharacterObject>("Murandy_noble1");
                                }
                            }
                            else
                            {
                                rootTroopTemplate = Game.Current.ObjectManager.GetObject<CharacterObject>("Cairhien_noble1");
                            }
                        }
                        else
                        {
                            rootTroopTemplate = Game.Current.ObjectManager.GetObject<CharacterObject>("Andor_noble1");
                        }
                        bool flag4 = rootTroopTemplate == null;
                        if (!flag4)
                        {
                            foreach (Hero notable in settlement.Notables)
                            {
                                bool flag5 = !notable.CanHaveRecruits;
                                if (!flag5)
                                {
                                    for (int i = 0; i < 6; i++)
                                    {
                                        bool flag6 = notable.VolunteerTypes[i] != null;
                                        if (flag6)
                                        {
                                            bool isEliteTroop = NobleRecruitPatch.IsEliteTroop(notable.VolunteerTypes[i]);
                                            bool flag7 = isEliteTroop;
                                            if (flag7)
                                            {
                                                CharacterObject replacement = NobleRecruitPatch.GetTroopOfTier(rootTroopTemplate, notable.VolunteerTypes[i].Tier);
                                                bool flag8 = replacement != null;
                                                if (flag8)
                                                {
                                                    notable.VolunteerTypes[i] = replacement;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public static bool IsEliteTroop(CharacterObject troop)
        {
            bool flag = troop == null || troop.Culture == null || troop.Culture.EliteBasicTroop == null;
            bool result;
            if (flag)
            {
                result = false;
            }
            else
            {
                List<CharacterObject> eliteTroops = new List<CharacterObject>();
                Stack<CharacterObject> stack = new Stack<CharacterObject>();
                stack.Push(troop.Culture.EliteBasicTroop);
                eliteTroops.Add(troop.Culture.EliteBasicTroop);
                while (stack.Count > 0)
                {
                    CharacterObject current = stack.Pop();
                    bool flag2 = current.UpgradeTargets != null && current.UpgradeTargets.Length != 0;
                    if (flag2)
                    {
                        foreach (CharacterObject upgrade in current.UpgradeTargets)
                        {
                            bool flag3 = !eliteTroops.Contains(upgrade);
                            if (flag3)
                            {
                                eliteTroops.Add(upgrade);
                                stack.Push(upgrade);
                            }
                        }
                    }
                }
                result = eliteTroops.Contains(troop);
            }
            return result;
        }

        public static CharacterObject GetTroopOfTier(CharacterObject rootTroop, int targetTier)
        {
            CharacterObject result = rootTroop;
            while (result.Tier < targetTier && result.UpgradeTargets != null && result.UpgradeTargets.Length != 0)
            {
                result = result.UpgradeTargets[MBRandom.RandomInt(result.UpgradeTargets.Length)];
            }
            return result;
        }
    }
   
}
