//   - Recruitment costs influence, not gold
//   - Cost is adjusted by player's relation with the settlement's owning clan leader
//   - Player's current kingdom must match the settlement's faction (or player is independent)
//   - Independent players (no kingdom) can recruit from any settlement with enough influence
//   - Per-settlement cooldown, persisted across saves via SyncData
//   - town_tarvalon recruits equal numbers of AesSedai3 AND Whitetower_warder2
//   - All other settlements recruit a single troop type

using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace WoT_Code.Behaviours
{
    public class OnePowerRecruitBehavior : CampaignBehaviorBase
    {
        // =====================================================================
        // Settlement registry
        //
        // Maps settlement StringId to recruitment configuration.
        // TarValon is special — it recruits two troop types simultaneously.
        // All other settlements recruit a single troop type.
        //
        // KingdomCultureId is the vanilla culture StringId that the owning
        // kingdom must match for kingdom-restricted recruitment.
        // =====================================================================

        private class SettlementConfig
        {
            public string PrimaryTroopId { get; set; }
            public string SecondaryTroopId { get; set; } // null for single-troop settlements
            public int BaseCostPerTroop { get; set; }
            public List<string> KingdomCultureId { get; set; } // vanilla culture StringId
            public List<string> KingdomDisplayName { get; set; } // for the UI message
            public string DisplayName { get; set; } // shown in menu text
        }

        private static readonly Dictionary<string, SettlementConfig> Settlements =
            new Dictionary<string, SettlementConfig>
        {
            {
                "town_tarvalon", new SettlementConfig
                {
                    PrimaryTroopId   = "AesSedai3",
                    SecondaryTroopId = "Whitetower_warder2",
                    BaseCostPerTroop = 5,
                    KingdomCultureId = new List<string> { "empire", "battania", "khuzait", "white", "black" },
                    KingdomDisplayName = new List<string> { "Westlander", "Coastlander", "Borderlander" },
                    DisplayName      = "Aes Sedai and their Warders"
                }
            },
            {
                "town_ashaman", new SettlementConfig
                {
                    PrimaryTroopId   = "Ashaman3",
                    BaseCostPerTroop = 5,
                    KingdomCultureId = new List<string> { "empire", "battania", "khuzait", "white", "black" },
                    KingdomDisplayName = new List<string> { "Westlander", "Coastlander", "Borderlander" },
                    DisplayName      = "Asha'man"
                }
            },
            {
                "town_thetown", new SettlementConfig
                {
                    PrimaryTroopId   = "Dreadlord1",
                    BaseCostPerTroop = 5,
                    KingdomCultureId = new List<string> { "sturgia" },
                    KingdomDisplayName = new List<string> { "Darkfriend" },
                    DisplayName      = "Dreadlords"
                }
            },
            {
                "town_fortress", new SettlementConfig
                {
                    PrimaryTroopId   = "DreadlordAiel1",
                    BaseCostPerTroop = 5,
                    KingdomCultureId = new List<string> { "sturgia" },      
                    KingdomDisplayName = new List<string> { "Darkfriend" },
                    DisplayName      = "Aiel Dreadlords"
                }
            },
            {
                "town_aiel_1", new SettlementConfig
                {
                    PrimaryTroopId   = "AielWiseone1",
                    BaseCostPerTroop = 4,
                    KingdomCultureId = new List<string> { "aserai" },       
                    KingdomDisplayName = new List<string> { "Aiel" },
                    DisplayName      = "Wise Ones"
                }
            },
                        {
                "town_aiel_4", new SettlementConfig
                {
                    PrimaryTroopId   = "AielWiseone1",
                    BaseCostPerTroop = 4,
                    KingdomCultureId = new List<string> { "aserai" },
                    KingdomDisplayName = new List<string> { "Aiel" },
                    DisplayName      = "Wise Ones"
                }
            },
                                    {
                "town_aiel_5", new SettlementConfig
                {
                    PrimaryTroopId   = "AielWiseone1",
                    BaseCostPerTroop = 4,
                    KingdomCultureId = new List<string> { "aserai" },
                    KingdomDisplayName = new List<string> { "Aiel" },
                    DisplayName      = "Wise Ones"
                }
            },
            {
                "town_falme", new SettlementConfig
                {
                    PrimaryTroopId   = "Damane3",
                    BaseCostPerTroop = 5,
                    KingdomCultureId = new List<string> { "vlandia" },      
                    KingdomDisplayName = new List<string> { "Seanchan" },
                    DisplayName      = "Damane"
                }
            },
            {
                "town_tanchico", new SettlementConfig
                {
                    PrimaryTroopId   = "Illumtroop1",
                    BaseCostPerTroop = 2,
                    KingdomCultureId = new List<string> { "empire", "battania", "khuzait", "vlandia" },     
                    KingdomDisplayName = new List<string> { "Westlander", "Coastlander", "Borderlander", "Seanchan" },
                    DisplayName      = "Illuminators"
                }
            },
            {
                "town_cairhien", new SettlementConfig
                {
                    PrimaryTroopId   = "Illumtroop1",
                    BaseCostPerTroop = 2,
                    KingdomCultureId = new List<string> { "empire", "battania", "khuzait", "vlandia" },
                    KingdomDisplayName = new List<string> { "Westlander", "Coastlander", "Borderlander", "Seanchan" },
                    DisplayName      = "Illuminators"
                }
            },
            {
                "town_eboudar", new SettlementConfig
                {
                    PrimaryTroopId   = "KIN2",
                    BaseCostPerTroop = 2,
                    KingdomCultureId = new List<string> { "empire", "battania", "khuzait" },     
                    KingdomDisplayName = new List<string> { "Westlander", "Coastlander", "Borderlander" },
                    DisplayName      = "Kinswomen"
                }
            },
            {
                "town_emonds", new SettlementConfig
                {
                    PrimaryTroopId   = "Ashaman3",
                    BaseCostPerTroop = 5,
                    KingdomCultureId = new List<string> { "empire", "battania", "khuzait" },
                    KingdomDisplayName = new List<string> { "Westlander", "Coastlander", "Borderlander" },  
                    DisplayName      = "Asha'man"
                }
            },
            {
                "town_chachin", new SettlementConfig
                {
                    PrimaryTroopId   = "AesSedai_Red1",
                    BaseCostPerTroop = 5,
                    KingdomCultureId = new List<string> { "empire", "battania", "khuzait" },
                    KingdomDisplayName = new List<string> { "Westlander", "Coastlander", "Borderlander" },
                    DisplayName      = "Elaida's Red Ajah"
                }
            },
                        {
                "town_tear", new SettlementConfig
                {
                    PrimaryTroopId   = "seafolkwind2",
                    BaseCostPerTroop = 3,
                    KingdomCultureId = new List<string> { "empire", "battania", "khuzait" },
                    KingdomDisplayName = new List<string> { "Westlander", "Coastlander", "Borderlander" },
                    DisplayName      = "Sea Folk Windfinders"
                }
            },
        };

        // Quantity tiers offered in the sub-menu - note if you change to 1 you need to change the menu text to "set(s)" instead of "sets"
        private static readonly int[] RecruitQuantities = { 5, 10, 15, 20 };

        // Cooldown duration per settlement
        private const float CooldownDays = 7f;

        // Per-settlement cooldown tracking — persisted via SyncData
        private Dictionary<string, CampaignTime> _cooldowns =
            new Dictionary<string, CampaignTime>();

        // =====================================================================
        // CampaignBehaviorBase
        // =====================================================================

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(
                this, OnSessionLaunched);
        }

        public override void SyncData(IDataStore dataStore)
        {
            // Persist cooldowns across saves so they survive reload
            dataStore.SyncData("WoT_OnePowerRecruitCooldowns", ref _cooldowns);
        }

        private void OnSessionLaunched(CampaignGameStarter starter)
        {
            AddOnePowerMenus(starter);
        }

        // =====================================================================
        // Menu construction
        // =====================================================================

        private void AddOnePowerMenus(CampaignGameStarter starter)
        {
            // Entry option — appears in the town centre menu
            starter.AddGameMenuOption(
                "town",
                "wot_onepower_recruit_enter",
                "{WOT_RECRUIT_ENTRY_TEXT}",
                OnePowerEntryCondition,
                args => GameMenu.SwitchToMenu("wot_onepower_recruit_menu"),
                false, 4, false);

            // Quantity selection sub-menu
            starter.AddGameMenu(
                "wot_onepower_recruit_menu",
                "{WOT_RECRUIT_MENU_TEXT}",
                OnePowerMenuInit,
                GameMenu.MenuOverlayType.SettlementWithBoth);

            // One option per quantity tier
            foreach (int qty in RecruitQuantities)
            {
                int capturedQty = qty; // capture for lambda
                starter.AddGameMenuOption(
                    "wot_onepower_recruit_menu",
                    $"wot_recruit_{qty}",
                    $"{{WOT_RECRUIT_OPTION_{qty}_TEXT}}",
                    args => RecruitOptionCondition(args, capturedQty),
                    args => ExecuteRecruitment(capturedQty),
                    false, -1, false);
            }

            // Leave option
            starter.AddGameMenuOption(
                "wot_onepower_recruit_menu",
                "wot_onepower_leave",
                "This is not the right time.",
                args =>
                {
                    args.optionLeaveType = GameMenuOption.LeaveType.Leave;
                    return true;
                },
                args => GameMenu.SwitchToMenu("town"),
                true);
        }

        // =====================================================================
        // Condition and init delegates
        // =====================================================================

        private bool OnePowerEntryCondition(MenuCallbackArgs args)
        {
            Settlement settlement = Settlement.CurrentSettlement;
            if (settlement == null) return false;
            if (!Settlements.TryGetValue(settlement.StringId, out SettlementConfig config))
                return false;

            // Set the entry option text dynamically
            if (config.DisplayName == "Illuminators")
            {
                MBTextManager.SetTextVariable("WOT_RECRUIT_ENTRY_TEXT",
                    $"Seek those belonging to the Illuminators Guild ({config.DisplayName}). ");
            }
            else
            {
                MBTextManager.SetTextVariable("WOT_RECRUIT_ENTRY_TEXT",
                    $"Seek those who can channel the One Power ({config.DisplayName}).");
            }

            // Kingdom faction check — if player has a kingdom, its culture must
            // match this settlement's culture. Independent players can always access.
            //  "Only vassals or rulers of kingdoms of acceptable cultural allegiances may recruit here. "
            if (!IsFactionAllowed(settlement, config))
            {
                args.Tooltip = new TextObject(
                    "Your kingdom's cultural allegiance prevents you from recruiting here. " +
                    $"Allowed factions: {string.Join(", ", config.KingdomDisplayName)}");
                args.IsEnabled = false;
                return true; // show but disabled
            }

            // Cooldown check
            if (IsOnCooldown(settlement.StringId, out int daysRemaining))
            {
                args.Tooltip = new TextObject(
                    $"The channellers need time to recover. Return in {daysRemaining} days.");
                args.IsEnabled = false;
                return true; // show but disabled with tooltip
            }

            // Influence check — must afford at least 5
            int minCost = CalculateCost(config.BaseCostPerTroop, 5, settlement);
            if (Clan.PlayerClan.Influence < minCost)
            {
                args.Tooltip = new TextObject(
                    $"You need at least {minCost} influence to recruit here. " +
                    $"You have {(int)Clan.PlayerClan.Influence}.");
                args.IsEnabled = false;
                return true; // show but disabled
            }

            args.optionLeaveType = GameMenuOption.LeaveType.Submenu;
            return true;
        }

        private void OnePowerMenuInit(MenuCallbackArgs args)
        {
            Settlement settlement = Settlement.CurrentSettlement;
            if (settlement == null) return;
            if (!Settlements.TryGetValue(settlement.StringId, out SettlementConfig config))
                return;

            int relation = GetRelation(settlement);
            string relationText = relation >= 0
                ? $"+{relation} (discount applied)"
                : $"{relation} (premium applied)";

            // Set main menu description text
            MBTextManager.SetTextVariable("WOT_RECRUIT_MENU_TEXT",
                $"The keeper of {settlement.Name} regards you carefully. " +
                $"Your relation with this faction: {relationText}. " +
                (config.SecondaryTroopId != null
                    ? $"Each set includes one Aes Sedai and one Warder. "
                    : string.Empty) +
                "How many do you require?");

            // Pre-calculate and set cost text variables for each quantity option
            foreach (int qty in RecruitQuantities)
            {
                int cost = CalculateCost(config.BaseCostPerTroop, qty, settlement);
                string label = config.SecondaryTroopId != null
                    ? $"{qty} sets of {config.DisplayName} — {cost} influence"
                    : $"{qty} {config.DisplayName} — {cost} influence";
                MBTextManager.SetTextVariable($"WOT_RECRUIT_OPTION_{qty}_TEXT", label);
            }
        }

        private bool RecruitOptionCondition(MenuCallbackArgs args, int quantity)
        {
            Settlement settlement = Settlement.CurrentSettlement;
            if (settlement == null) return false;
            if (!Settlements.TryGetValue(settlement.StringId, out SettlementConfig config))
                return false;

            int cost = CalculateCost(config.BaseCostPerTroop, quantity, settlement);

            if (Clan.PlayerClan.Influence < cost)
            {
                args.Tooltip = new TextObject(
                    $"Insufficient influence. Need {cost}, " +
                    $"have {(int)Clan.PlayerClan.Influence}.");
                args.IsEnabled = false;
            }

            return true;
        }

        // =====================================================================
        // Recruitment execution
        // =====================================================================

        private void ExecuteRecruitment(int quantity)
        {
            Settlement settlement = Settlement.CurrentSettlement;
            if (settlement == null) return;
            if (!Settlements.TryGetValue(settlement.StringId, out SettlementConfig config))
                return;

            int totalCost = CalculateCost(config.BaseCostPerTroop, quantity, settlement);

            // Resolve primary troop
            CharacterObject primaryTroop = CharacterObject.Find(config.PrimaryTroopId);
            if (primaryTroop == null)
            {
                InformationManager.DisplayMessage(new InformationMessage(
                    $"[WoT] Could not find troop '{config.PrimaryTroopId}' — check XML StringId.",
                    Color.FromUint(0xFF0000FF)));
                return;
            }

            // Resolve secondary troop (TarValon only)
            CharacterObject secondaryTroop = null;
            if (config.SecondaryTroopId != null)
            {
                secondaryTroop = CharacterObject.Find(config.SecondaryTroopId);
                if (secondaryTroop == null)
                {
                    InformationManager.DisplayMessage(new InformationMessage(
                        $"[WoT] Could not find troop '{config.SecondaryTroopId}' — check XML StringId.",
                        Color.FromUint(0xFF0000FF)));
                    return;
                }
            }

            // Deduct influence
            Clan.PlayerClan.Influence -= totalCost;

            // Add troops to party
            PartyBase.MainParty.MemberRoster.AddToCounts(primaryTroop, quantity);
            if (secondaryTroop != null)
                PartyBase.MainParty.MemberRoster.AddToCounts(secondaryTroop, quantity);

            // Record cooldown
            _cooldowns[settlement.StringId] = CampaignTime.Now;

            // Confirmation message
            string recruitedText = secondaryTroop != null
                ? $"{quantity} {config.DisplayName}"
                : $"{quantity} {config.DisplayName}";

            InformationManager.DisplayMessage(new InformationMessage(
                $"{recruitedText} have joined your party. " +
                $"({totalCost} influence spent, " +
                $"{(int)Clan.PlayerClan.Influence} remaining)",
                Color.FromUint(0x00AA00FF)));

            GameMenu.SwitchToMenu("town");
        }

        // =====================================================================
        // Helper methods
        // =====================================================================

        /// <summary>
        /// Returns true if the player is allowed to recruit from this settlement.
        /// Independent players (no kingdom) are always allowed.
        /// Kingdom members can only recruit from settlements matching their kingdom's culture.
        /// </summary>
        private bool IsFactionAllowed(Settlement settlement, SettlementConfig config)
        {
            Kingdom playerKingdom = Clan.PlayerClan.Kingdom;

            // Independent players can recruit anywhere with enough influence
            if (playerKingdom == null)
                return true;

            // Check if player kingdom's culture is in the allowed list
            return config.KingdomCultureId.Contains(
                playerKingdom.Culture?.StringId ?? string.Empty);
        }

        /// <summary>
        /// Calculates the total influence cost adjusted by relation.
        /// Relation -100 → 1.5× cost. Relation 0 → 1.0× cost. Relation +100 → 0.5× cost.
        /// </summary>
        private int CalculateCost(int baseCostPerTroop, int quantity, Settlement settlement)
        {
            int relation = GetRelation(settlement);
            float modifier = 1.0f - (relation / 200.0f);
            modifier = MBMath.ClampFloat(modifier, 0.5f, 1.5f);
            return Math.Max(1, (int)(baseCostPerTroop * quantity * modifier));
        }

        /// <summary>
        /// Gets the player's relation with the settlement's owning clan leader.
        /// Returns 0 safely if the settlement has no owner.
        /// </summary>
        private int GetRelation(Settlement settlement)
        {
            Hero owner = settlement.OwnerClan?.Leader;
            if (owner == null) return 0;
            return (int)Hero.MainHero.GetRelation(owner);
        }

        /// <summary>
        /// Checks whether the settlement is currently on cooldown.
        /// Returns true and populates daysRemaining if cooldown is active.
        /// </summary>
        private bool IsOnCooldown(string settlementId, out int daysRemaining)
        {
            daysRemaining = 0;
            if (!_cooldowns.TryGetValue(settlementId, out CampaignTime lastRecruitTime))
                return false;

            CampaignTime cooldownEnd = lastRecruitTime + CampaignTime.Days(CooldownDays);
            if (CampaignTime.Now >= cooldownEnd)
                return false;

            daysRemaining = Math.Max(1,
                (int)(cooldownEnd - CampaignTime.Now).ToDays);
            return true;
        }
    }
}

