// WoTCharacterCreationBehavior.cs
// Namespace: WoT_Code
//
// Architecture: Instance class inheriting CharacterCreationCampaignBehavior
// ---------------------------------------------------------------------------------
// Key insight from working mod (CharacterCreationRedoneSandbox.cs):
//   The patch class MUST inherit CharacterCreationCampaignBehavior so that:
//   - The Harmony prefix receives __instance as the concrete subclass
//   - All NarrativeMenu character delegates are bound to THIS instance
//   - Virtual methods (GetMotherEquipmentId, GetFatherEquipmentId,
//     GetParentMenuNarrativeMenuCharacterArgs etc.) resolve against our instance
//   - FaceGenUpdated and ApplyMainHeroEquipment resolve against our instance
//
// A static class prefix cannot satisfy these requirements — ModifyMenuCharacters
// calls back into the registered behavior instance to resolve character args.
// If that instance is vanilla's, it crashes on our replacement menus.

using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.CharacterCreationContent;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace WoT_Code
{
    // =========================================================================
    // WoTCharacterCreationBehavior
    //
    // Inherits CharacterCreationCampaignBehavior — the engine calls back into
    // this instance for character args, equipment resolution, FaceGenUpdated.
    // The Harmony prefix intercepts InitializeData and builds our WoT menus,
    // with all delegates bound to __instance (this class) via instance methods.
    // =========================================================================

    [HarmonyPatch(typeof(CharacterCreationCampaignBehavior), "InitializeData")]
    public class WoTCharacterCreationBehavior : CharacterCreationCampaignBehavior, ICharacterCreationContentHandler
    {
        // =====================================================================
        // Harmony Prefix — replaces vanilla InitializeData entirely.
        // __instance is our WoTCharacterCreationBehavior instance.
        // =====================================================================

        [HarmonyPrefix]
        private static bool Prefix(WoTCharacterCreationBehavior __instance, CharacterCreationManager characterCreationManager)
        {
            CultureObject culture = CharacterObject.PlayerCharacter?.Culture;
            if (culture != null)
            {
                TextObject generatedName = NameGenerator.Current.GenerateClanName(culture, null);
                Clan.PlayerClan.ChangeClanName(generatedName, generatedName);
            }

            int focusToAdd = characterCreationManager.CharacterCreationContent.FocusToAdd;
            int skillLevelToAdd = characterCreationManager.CharacterCreationContent.SkillLevelToAdd;
            int attributeLevelToAdd = characterCreationManager.CharacterCreationContent.AttributeLevelToAdd;

            characterCreationManager.CharacterCreationContent.ChangeReviewPageDescription(
                new TextObject("You prepare to set off for a grand adventure in the world of the Wheel of Time! Here is your character. Continue if you are ready, or go back to make changes."));

            //characterCreationManager.CharacterCreationContent.AddEquipmentToUseGetter(
            //    (string occupationId, out string equipmentId) =>
            //        __instance.TryGetEquipmentId(occupationId, out equipmentId));

            __instance.AddParentsMenu(characterCreationManager, focusToAdd, skillLevelToAdd, attributeLevelToAdd);
            __instance.AddYoungChildMenu(characterCreationManager, focusToAdd, skillLevelToAdd, attributeLevelToAdd);
            __instance.AddChildhoodMenu(characterCreationManager, focusToAdd, skillLevelToAdd, attributeLevelToAdd);
            __instance.AddTeenagerMenu(characterCreationManager, focusToAdd, skillLevelToAdd, attributeLevelToAdd);
            __instance.AddFocusMenu(characterCreationManager, focusToAdd, skillLevelToAdd, attributeLevelToAdd);
            __instance.AddAgeSelectionMenu(characterCreationManager);

            return false;
        }



        // =====================================================================
        // FaceGenUpdated — called by engine after each character creation stage to
        // update the character's FaceGen appearance. Override vanilla virtual.
        // =====================================================================

        public new void FaceGenUpdated()
        {
            CharacterCreationManager manager =
                (GameStateManager.Current.ActiveState as CharacterCreationState)?.CharacterCreationManager;
            if (manager == null) return;

            BodyProperties motherProps = CharacterObject.PlayerCharacter.GetBodyProperties(CharacterObject.PlayerCharacter.Equipment, -1);
            BodyProperties fatherProps = CharacterObject.PlayerCharacter.GetBodyProperties(CharacterObject.PlayerCharacter.Equipment, -1);
            BodyProperties baseProps = CharacterObject.PlayerCharacter.GetBodyProperties(CharacterObject.PlayerCharacter.Equipment, -1);

            FaceGen.GenerateParentKey(baseProps, CharacterObject.PlayerCharacter.Race,
                ref motherProps, ref fatherProps);

            motherProps = new BodyProperties(new DynamicBodyProperties(33f, 0.3f, 0.2f), motherProps.StaticProperties);
            fatherProps = new BodyProperties(new DynamicBodyProperties(33f, 0.5f, 0.5f), fatherProps.StaticProperties);

            foreach (NarrativeMenu menu in manager.NarrativeMenus)
            {
                foreach (NarrativeMenuCharacter character in menu.Characters)
                {
                    if (character.StringId.Equals("mother_character"))
                        character.UpdateBodyProperties(motherProps, CharacterObject.PlayerCharacter.Race, true);

                    else if (character.StringId.Equals("father_character"))
                        character.UpdateBodyProperties(fatherProps, CharacterObject.PlayerCharacter.Race, false);

                    // For player stage characters — apply age-adjusted props
                    // so FaceGenUpdated doesn't reset what ModifyMenuCharacters set
                    else if (character.StringId.Equals("player_childhood_character"))
                    {
                        BodyProperties aged = FaceGen.GetBodyPropertiesWithAge(ref baseProps, 8f);
                        character.UpdateBodyProperties(aged, CharacterObject.PlayerCharacter.Race, false);
                    }
                    else if (character.StringId.Equals("player_education_character"))
                    {
                        BodyProperties aged = FaceGen.GetBodyPropertiesWithAge(ref baseProps, 13f);
                        character.UpdateBodyProperties(aged, CharacterObject.PlayerCharacter.Race, false);
                    }
                    else if (character.StringId.Equals("player_youth_character"))
                    {
                        BodyProperties aged = FaceGen.GetBodyPropertiesWithAge(ref baseProps, 17f);
                        character.UpdateBodyProperties(aged, CharacterObject.PlayerCharacter.Race, false);
                    }
                    else if (character.StringId.Equals("player_adulthood_character"))
                    {
                        BodyProperties aged = FaceGen.GetBodyPropertiesWithAge(ref baseProps, 20f);
                        character.UpdateBodyProperties(aged, CharacterObject.PlayerCharacter.Race, false);
                    }
                }
            }
        }

        // =====================================================================
        // GetMotherEquipmentId / GetFatherEquipmentId / GetPlayerEquipmentId
        // Override vanilla virtuals — called during onSelect delegates to dress
        // NPC characters. Uses vanilla naming convention:
        //   mother_char_creation_{occupation}_{culture}
        //   father_char_creation_{occupation}_{culture}
        //   player_char_creation_{culture}_{occupation}_{m|f}
        // =====================================================================

        public new string GetMotherEquipmentId(CharacterCreationManager mgr, string occupation, string culture)
        {
            return "mother_char_creation_" + occupation + "_" + culture;
        }

        public new string GetFatherEquipmentId(CharacterCreationManager mgr, string occupation, string culture)
        {
            return "father_char_creation_" + occupation + "_" + culture;
        }

        public new string GetPlayerEquipmentId(CharacterCreationManager mgr, string occupation, string culture, bool isFemale)
        {
            return "player_char_creation_" + culture + "_" + occupation + "_" + (isFemale ? "f" : "m");
        }

        // =====================================================================
        // ApplyMainHeroEquipment — vanilla equivalent is private so we declare
        // our own. Called from age selection onConsequence delegates.
        // Added null guard on playerChar that vanilla's private version lacks.
        // =====================================================================

        private void ApplyMainHeroEquipment(CharacterCreationManager characterCreationManager)
        {
            NarrativeMenu narrativeMenuWithId = characterCreationManager.GetNarrativeMenuWithId("narrative_age_selection_menu");
            NarrativeMenuCharacter narrativeMenuCharacter = null;
            foreach (NarrativeMenuCharacter c in narrativeMenuWithId.Characters)
            {
                if (c.StringId.Equals("player_age_selection_character"))
                {
                    narrativeMenuCharacter = c;
                    break;
                }
            }
            if (narrativeMenuCharacter == null) return;
            CharacterObject.PlayerCharacter.Equipment.FillFrom(narrativeMenuCharacter.Equipment.DefaultEquipment, true);
            CharacterObject.PlayerCharacter.FirstCivilianEquipment.FillFrom(narrativeMenuCharacter.Equipment.GetRandomCivilianEquipment(), true);
        }

        // =====================================================================
        // Helper to set parent occupation and equipment in one place,
        // called from onSelect delegates.
        // =====================================================================

        private void SetParentOccupationAndEquipment(CharacterCreationManager mgr,
        string occupation, string motherAnimation, string fatherAnimation)
        {
            mgr.CharacterCreationContent.SetParentOccupation(occupation);
            MBEquipmentRoster motherRoster = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(
                GetMotherEquipmentId(mgr, occupation, mgr.CharacterCreationContent.SelectedCulture.StringId));
            MBEquipmentRoster fatherRoster = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>(
                GetFatherEquipmentId(mgr, occupation, mgr.CharacterCreationContent.SelectedCulture.StringId));
            UpdateParentEquipment(mgr, motherRoster, fatherRoster, motherAnimation, fatherAnimation);
        }

        // =====================================================================
        // Helper to set player occupation and equipment in one place,
        // called from onSelect delegates.
        // =====================================================================

        private void UpdatePlayerCharacterEquipment(CharacterCreationManager mgr,
        string characterStringId, string animationId)
        {
            foreach (var c in mgr.CurrentMenu.Characters)
            {
                if (c.StringId == characterStringId)
                {
                    string equipId = GetPlayerEquipmentId(mgr,
                        mgr.CharacterCreationContent.SelectedTitleType,
                        mgr.CharacterCreationContent.SelectedCulture.StringId,
                        Hero.MainHero.IsFemale);
                    MBEquipmentRoster roster = Game.Current.ObjectManager
                        .GetObject<MBEquipmentRoster>(equipId);
                    if (roster != null) c.SetEquipment(roster);
                    c.SetAnimationId(animationId);
                    break;
                }
            }
        }

        // =====================================================================
        // Age Selection Menu — instance method, mirrors vanilla 1.4.5 pattern.
        // Bound to __instance so ApplyMainHeroEquipment resolves correctly.
        // inputMenuId uses vanilla sentinel "narrative_adulthood_menu" to chain
        // correctly from the last narrative stage. outputMenuId "" = final menu.
        // =====================================================================

        public new void AddAgeSelectionMenu(CharacterCreationManager manager)
        {
            BodyProperties bodyProps = CharacterObject.PlayerCharacter.GetBodyProperties(CharacterObject.PlayerCharacter.Equipment, -1);
            bodyProps = FaceGen.GetBodyPropertiesWithAge(ref bodyProps, (float)manager.CharacterCreationContent.StartingAge);

            NarrativeMenuCharacter playerChar = new NarrativeMenuCharacter(
                "player_age_selection_character",
                bodyProps, CharacterObject.PlayerCharacter.Race,
                CharacterObject.PlayerCharacter.IsFemale);
            //NarrativeMenuCharacter horseChar = new NarrativeMenuCharacter("narrative_character_horse");

            NarrativeMenu ageMenu = new NarrativeMenu(
                "narrative_age_selection_menu",
                "narrative_focus_menu", "",
                new TextObject("Starting Age"),
                new TextObject("Your character started off on the adventuring path at the age of..."),
                new List<NarrativeMenuCharacter> { playerChar },
                new NarrativeMenu.GetNarrativeMenuCharacterArgsDelegate(
                    this.GetAgeSelectionMenuCharacterArgs));

            ageMenu.AddNarrativeMenuOption(new NarrativeMenuOption(
                "age_selection_young_adult_option",
                new TextObject("20"),
                new TextObject("While lacking experience a bit, you are full of youthful energy and fully eager for the long years of adventuring ahead."),
                args => { args.SetUnspentFocusToAdd(2); args.SetUnspentAttributeToAdd(1); },
                mgr => true,
                mgr => {
                    foreach (var c in mgr.CurrentMenu.Characters)
                    {
                        if (c.StringId == "player_age_selection_character")
                        {
                            c.SetAnimationId("act_childhood_focus"); c.ChangeAge(20f);
                            BodyProperties baseProps = CharacterObject.PlayerCharacter.GetBodyProperties(null, -1);
                            BodyProperties agedProps = FaceGen.GetBodyPropertiesWithAge(ref baseProps, 20f);
                            c.UpdateBodyProperties(agedProps, CharacterObject.PlayerCharacter.Race, Hero.MainHero.IsFemale);
                            break;
                        }
                    }
                    mgr.CharacterCreationContent.StartingAge = 20;
                    Hero.MainHero.SetBirthDay(CampaignTime.YearsFromNow(-20f));
                },
                mgr => { mgr.CharacterCreationContent.StartingAge = 20; this.ApplyMainHeroEquipment(mgr); }));

            ageMenu.AddNarrativeMenuOption(new NarrativeMenuOption(
                "age_selection_adult_option",
                new TextObject("30"),
                new TextObject("You are at your prime. You still have some youthful energy but also have a substantial amount of experience under your belt."),
                args => { args.SetUnspentFocusToAdd(4); args.SetUnspentAttributeToAdd(2); },
                mgr => true,
                mgr => {
                    foreach (var c in mgr.CurrentMenu.Characters)
                    {
                        if (c.StringId == "player_age_selection_character")
                        {
                            c.SetAnimationId("act_childhood_athlete"); c.ChangeAge(30f);
                            BodyProperties baseProps = CharacterObject.PlayerCharacter.GetBodyProperties(null, -1);
                            BodyProperties agedProps = FaceGen.GetBodyPropertiesWithAge(ref baseProps, 30f);
                            c.UpdateBodyProperties(agedProps, CharacterObject.PlayerCharacter.Race, Hero.MainHero.IsFemale);
                            break;
                        }
                    }
                    mgr.CharacterCreationContent.StartingAge = 30;
                    Hero.MainHero.SetBirthDay(CampaignTime.YearsFromNow(-30f));
                },
                mgr => { mgr.CharacterCreationContent.StartingAge = 30; this.ApplyMainHeroEquipment(mgr); }));

            ageMenu.AddNarrativeMenuOption(new NarrativeMenuOption(
                "age_selection_middle_age_option",
                new TextObject("40"),
                new TextObject("This is the right age for starting off. You have years of experience, and you are old enough for people to respect you and gather under your banner."),
                args => { args.SetUnspentFocusToAdd(6); args.SetUnspentAttributeToAdd(3); },
                mgr => true,
                mgr => {
                    foreach (var c in mgr.CurrentMenu.Characters)
                    {
                        if (c.StringId == "player_age_selection_character")
                        {
                            c.SetAnimationId("act_childhood_sharp"); c.ChangeAge(40f);
                            BodyProperties baseProps = CharacterObject.PlayerCharacter.GetBodyProperties(null, -1);
                            BodyProperties agedProps = FaceGen.GetBodyPropertiesWithAge(ref baseProps, 40f);
                            c.UpdateBodyProperties(agedProps, CharacterObject.PlayerCharacter.Race, Hero.MainHero.IsFemale);
                            break;
                        }
                    }
                    mgr.CharacterCreationContent.StartingAge = 40;
                    Hero.MainHero.SetBirthDay(CampaignTime.YearsFromNow(-40f));
                },
                mgr => { mgr.CharacterCreationContent.StartingAge = 40; this.ApplyMainHeroEquipment(mgr); }));

            ageMenu.AddNarrativeMenuOption(new NarrativeMenuOption(
                "age_selection_elder_option",
                new TextObject("50"),
                new TextObject("While you are past your prime, there is still enough time to go on that last big adventure. And you have all the experience you need to overcome anything!"),
                args => { args.SetUnspentFocusToAdd(8); args.SetUnspentAttributeToAdd(4); },
                mgr => true,
                mgr => {
                    foreach (var c in mgr.CurrentMenu.Characters)
                    {
                        if (c.StringId == "player_age_selection_character")
                        {
                            c.SetAnimationId("act_childhood_tough"); c.ChangeAge(50f); 
                            BodyProperties baseProps = CharacterObject.PlayerCharacter.GetBodyProperties(null, -1);
                            BodyProperties agedProps = FaceGen.GetBodyPropertiesWithAge(ref baseProps, 50f);
                            c.UpdateBodyProperties(agedProps, CharacterObject.PlayerCharacter.Race, Hero.MainHero.IsFemale);
                            break;
                        }
                    }
                    mgr.CharacterCreationContent.StartingAge = 50;
                    Hero.MainHero.SetBirthDay(CampaignTime.YearsFromNow(-50f));
                },
                mgr => { mgr.CharacterCreationContent.StartingAge = 50; this.ApplyMainHeroEquipment(mgr); }));

            manager.AddNewMenu(ageMenu);
        }

        // =====================================================================
        // Equipment resolution — instance method, delegate bound to __instance.
        // =====================================================================

        //public new bool TryGetEquipmentId(string occupationId, out string equipmentId)
        //{

        //    switch (occupationId)
        //    {
        //        // ------------------------------------------------------------------
        //        // EQUIPMENT PLACEHOLDER TAGS
        //        // Replace each string value with the id attribute of the corresponding
        //        // <EquipmentRoster id="..."> entry in your module's XML files.
        //        // ------------------------------------------------------------------

        //        // Aiel
        //        case "aiel_warriors": equipmentId = "EQUIPMENT_AIEL_WARRIORS"; return true;
        //        case "aiel_trackers": equipmentId = "EQUIPMENT_AIEL_TRACKERS"; return true;
        //        case "aiel_healers": equipmentId = "EQUIPMENT_AIEL_HEALERS"; return true;
        //        case "aiel_farmers": equipmentId = "EQUIPMENT_AIEL_FARMERS"; return true;
        //        case "aiel_hunters": equipmentId = "EQUIPMENT_AIEL_HUNTERS"; return true;
        //        case "aiel_smiths": equipmentId = "EQUIPMENT_AIEL_SMITHS"; return true;

        //        // Shadow
        //        case "shadow_noble_soldiers": equipmentId = "EQUIPMENT_SHADOW_NOBLE_SOLDIERS"; return true;
        //        case "shadow_thieves": equipmentId = "EQUIPMENT_SHADOW_THIEVES"; return true;
        //        case "shadow_healers": equipmentId = "EQUIPMENT_SHADOW_HEALERS"; return true;
        //        case "shadow_nobility": equipmentId = "EQUIPMENT_SHADOW_NOBILITY"; return true;
        //        case "shadow_farmers": equipmentId = "EQUIPMENT_SHADOW_FARMERS"; return true;
        //        case "shadow_peddlers": equipmentId = "EQUIPMENT_SHADOW_PEDDLERS"; return true;

        //        // Borderlands
        //        case "borderlands_noble_soldiers": equipmentId = "EQUIPMENT_BORDERLANDS_NOBLE_SOLDIERS"; return true;
        //        case "borderlands_thieves": equipmentId = "EQUIPMENT_BORDERLANDS_THIEVES"; return true;
        //        case "borderlands_healers": equipmentId = "EQUIPMENT_BORDERLANDS_HEALERS"; return true;
        //        case "borderlands_nobility": equipmentId = "EQUIPMENT_BORDERLANDS_NOBILITY"; return true;
        //        case "borderlands_farmers": equipmentId = "EQUIPMENT_BORDERLANDS_FARMERS"; return true;
        //        case "borderlands_peddlers": equipmentId = "EQUIPMENT_BORDERLANDS_PEDDLERS"; return true;

        //        // Westlands
        //        case "westlands_noble_soldiers": equipmentId = "EQUIPMENT_WESTLANDS_NOBLE_SOLDIERS"; return true;
        //        case "westlands_thieves": equipmentId = "EQUIPMENT_WESTLANDS_THIEVES"; return true;
        //        case "westlands_healers": equipmentId = "EQUIPMENT_WESTLANDS_HEALERS"; return true;
        //        case "westlands_nobility": equipmentId = "EQUIPMENT_WESTLANDS_NOBILITY"; return true;
        //        case "westlands_farmers": equipmentId = "EQUIPMENT_WESTLANDS_FARMERS"; return true;
        //        case "westlands_peddlers": equipmentId = "EQUIPMENT_WESTLANDS_PEDDLERS"; return true;

        //        // Coastlands
        //        case "coastlands_noble_soldiers": equipmentId = "EQUIPMENT_COASTLANDS_NOBLE_SOLDIERS"; return true;
        //        case "coastlands_thieves": equipmentId = "EQUIPMENT_COASTLANDS_THIEVES"; return true;
        //        case "coastlands_healers": equipmentId = "EQUIPMENT_COASTLANDS_HEALERS"; return true;
        //        case "coastlands_nobility": equipmentId = "EQUIPMENT_COASTLANDS_NOBILITY"; return true;
        //        case "coastlands_hunters": equipmentId = "EQUIPMENT_COASTLANDS_HUNTERS"; return true;
        //        case "coastlands_guildsmen": equipmentId = "EQUIPMENT_COASTLANDS_GUILDSMEN"; return true;

        //        // Seanchan
        //        case "seanchan_deathguard": equipmentId = "EQUIPMENT_SEANCHAN_DEATHGUARD"; return true;
        //        case "seanchan_thieves": equipmentId = "EQUIPMENT_SEANCHAN_THIEVES"; return true;
        //        case "seanchan_healers": equipmentId = "EQUIPMENT_SEANCHAN_HEALERS"; return true;
        //        case "seanchan_nobility": equipmentId = "EQUIPMENT_SEANCHAN_NOBILITY"; return true;
        //        case "seanchan_soldiers": equipmentId = "EQUIPMENT_SEANCHAN_SOLDIERS"; return true;
        //        case "seanchan_engineers": equipmentId = "EQUIPMENT_SEANCHAN_ENGINEERS"; return true;

        //        default:
        //            equipmentId = null;
        //            return false;
        //    }
        //}

        // =====================================================================
        // Skill shorthands — static properties reading through to DefaultSkills
        // singleton at call time, not at class-load time. This avoids null
        // references if the class is loaded before DefaultSkills.RegisterAll()
        // has run during game initialization.
        // TwoHanded is declared but not currently used by any option; retained
        // for future use.
        // =====================================================================

        private static SkillObject OneHanded => DefaultSkills.OneHanded;
        private static SkillObject TwoHanded => DefaultSkills.TwoHanded;
        private static SkillObject Polearm => DefaultSkills.Polearm;
        private static SkillObject Bow => DefaultSkills.Bow;
        private static SkillObject Crossbow => DefaultSkills.Crossbow;
        private static SkillObject Throwing => DefaultSkills.Throwing;
        private static SkillObject Riding => DefaultSkills.Riding;
        private static SkillObject Athletics => DefaultSkills.Athletics;
        private static SkillObject Smithing => DefaultSkills.Crafting;
        private static SkillObject Scouting => DefaultSkills.Scouting;
        private static SkillObject Tactics => DefaultSkills.Tactics;
        private static SkillObject Roguery => DefaultSkills.Roguery;
        private static SkillObject Charm => DefaultSkills.Charm;
        private static SkillObject Leadership => DefaultSkills.Leadership;
        private static SkillObject Trade => DefaultSkills.Trade;
        private static SkillObject Steward => DefaultSkills.Steward;
        private static SkillObject Medicine => DefaultSkills.Medicine;
        private static SkillObject Engineering => DefaultSkills.Engineering;

        // Attribute shorthands — same pattern, read-through at call time
        private static CharacterAttribute Vigor => DefaultCharacterAttributes.Vigor;
        private static CharacterAttribute Control => DefaultCharacterAttributes.Control;
        private static CharacterAttribute Endurance => DefaultCharacterAttributes.Endurance;
        private static CharacterAttribute Cunning => DefaultCharacterAttributes.Cunning;
        private static CharacterAttribute Social => DefaultCharacterAttributes.Social;
        private static CharacterAttribute Intelligence => DefaultCharacterAttributes.Intelligence;

        // Trait shorthand — vanilla trait objects
        private static TraitObject Valor => DefaultTraits.Valor;
        private static TraitObject Mercy => DefaultTraits.Mercy;
        private static TraitObject Generosity => DefaultTraits.Generosity;
        private static TraitObject Honor => DefaultTraits.Honor;
        private static TraitObject Calculating => DefaultTraits.Calculating;

        // =====================================================================
        // Per-stage NarrativeMenuCharacterArgs delegates
        // Each stage needs its own delegate returning the correct character
        // string ID and age. ModifyMenuCharacters calls this when transitioning
        // into each menu — wrong character type or null = crash.
        //
        // Stage ages mirror vanilla:
        //   parents    — mother(33) + father(33) with none equipment
        //   youngchild — player aged 8
        //   childhood  — player aged 10
        //   teenager   — player aged 14
        //   focus      — player aged 18
        // =====================================================================

        public new List<NarrativeMenuCharacterArgs> GetParentMenuNarrativeMenuCharacterArgs(
            CultureObject culture, string occupationType, CharacterCreationManager mgr)
        {
            string cultureId = mgr.CharacterCreationContent.SelectedCulture?.StringId ?? culture.StringId;
            return new List<NarrativeMenuCharacterArgs>
            {
                new NarrativeMenuCharacterArgs("mother_character", 33,
                    "mother_char_creation_none_" + cultureId,
                    "act_character_creation_female_default_standing",
                    "spawnpoint_player_1", "", "", null, true, true),
                new NarrativeMenuCharacterArgs("father_character", 33,
                    "father_char_creation_none_" + cultureId,
                    "act_character_creation_male_default_standing",
                    "spawnpoint_player_1", "", "", null, true, false)
            };
        }

        private List<NarrativeMenuCharacterArgs> GetYoungChildCharacterArgs(
            CultureObject culture, string occupationType, CharacterCreationManager mgr)
        {
            string cultureId = mgr.CharacterCreationContent.SelectedCulture?.StringId ?? culture.StringId;
            string occupation = mgr.CharacterCreationContent.SelectedParentOccupation ?? "merchant";
            string gender = Hero.MainHero.IsFemale ? "f" : "m";
            return new List<NarrativeMenuCharacterArgs>
            {
                new NarrativeMenuCharacterArgs("player_childhood_character", 8,
                    "player_char_creation_childhood_age_" + cultureId + "_" + occupation + "_" + gender,
                    "act_childhood_schooled", "spawnpoint_player_1", "", "", null, true,
                    Hero.MainHero.IsFemale)
            };
        }

        private List<NarrativeMenuCharacterArgs> GetChildhoodCharacterArgs(
            CultureObject culture, string occupationType, CharacterCreationManager mgr)
        {
            string cultureId = mgr.CharacterCreationContent.SelectedCulture?.StringId ?? culture.StringId;
            string occupation = mgr.CharacterCreationContent.SelectedParentOccupation ?? "merchant";
            string gender = Hero.MainHero.IsFemale ? "f" : "m";
            return new List<NarrativeMenuCharacterArgs>
            {
                new NarrativeMenuCharacterArgs("player_childhood_character", 13,
                    "player_char_creation_childhood_age_" + cultureId + "_" + occupation + "_" + gender,
                    "act_childhood_schooled", "spawnpoint_player_1", "", "", null, true,
                    Hero.MainHero.IsFemale)
            };
        }

        private List<NarrativeMenuCharacterArgs> GetTeenagerCharacterArgs(
            CultureObject culture, string occupationType, CharacterCreationManager mgr)
        {
            string cultureId = mgr.CharacterCreationContent.SelectedCulture?.StringId ?? culture.StringId;
            string occupation = mgr.CharacterCreationContent.SelectedTitleType ?? "default";
            string gender = Hero.MainHero.IsFemale ? "f" : "m";
            return new List<NarrativeMenuCharacterArgs>
            {
                new NarrativeMenuCharacterArgs("player_youth_character", 17,
                    "player_char_creation_" + cultureId + "_" + occupation + "_" + gender,
                    "act_childhood_schooled", "spawnpoint_player_1", "", "", null, true,
                    Hero.MainHero.IsFemale)
            };
        }

        private List<NarrativeMenuCharacterArgs> GetFocusCharacterArgs(
            CultureObject culture, string occupationType, CharacterCreationManager mgr)
        {
            string cultureId = mgr.CharacterCreationContent.SelectedCulture?.StringId ?? culture.StringId;
            string occupation = mgr.CharacterCreationContent.SelectedTitleType ?? "default";
            string gender = Hero.MainHero.IsFemale ? "f" : "m";
            return new List<NarrativeMenuCharacterArgs>
            {
                new NarrativeMenuCharacterArgs("player_adulthood_character", 20,
                    "player_char_creation_" + cultureId + "_" + occupation + "_" + gender,
                    "act_childhood_schooled", "spawnpoint_player_1", "", "", null, true,
                    Hero.MainHero.IsFemale)
            };
        }

        public List<NarrativeMenuCharacterArgs> GetAgeSelectionMenuCharacterArgs(
            CultureObject culture, string occupationType, CharacterCreationManager mgr)
        {
            string cultureId = mgr.CharacterCreationContent.SelectedCulture?.StringId ?? culture.StringId;
            string equipId = GetPlayerEquipmentId(mgr,
                mgr.CharacterCreationContent.SelectedTitleType,
                cultureId,
                Hero.MainHero.IsFemale);

            return new List<NarrativeMenuCharacterArgs>
            {
                new NarrativeMenuCharacterArgs(
                    "player_age_selection_character",
                    mgr.CharacterCreationContent.StartingAge,
                    equipId,
                    "act_childhood_schooled",
                    "spawnpoint_player_1", "", "", null, true,
                    Hero.MainHero.IsFemale),
            };
        }

        // =====================================================================
        // Helper: build a NarrativeMenu — delegate passed per stage type
        // so ModifyMenuCharacters gets the correct character on each transition.
        // =====================================================================

        private NarrativeMenu MakeMenu(
            string stringId,
            string inputMenuId,
            string outputMenuId,
            string title,
            string description,
            NarrativeMenu.GetNarrativeMenuCharacterArgsDelegate characterArgsDelegate,
            List<NarrativeMenuCharacter> characters = null)
        {
            return new NarrativeMenu(
                stringId,
                inputMenuId,
                outputMenuId,
                new TextObject(title),
                new TextObject(description),
                characters ?? new List<NarrativeMenuCharacter>(),
                characterArgsDelegate
            );
        }

        // =====================================================================
        // Helper: build a NarrativeMenuOption cleanly
        //
        // 1.4.5 architecture notes (verified against decompiled source):
        //   - NarrativeMenuOption takes 7 parameters; 7th (onConsequence) is null
        //     for all background options — engine applies effects via args.
        //   - focusToAdd/skillLevelToAdd/attributeLevelToAdd passed explicitly
        //     from the prefix body where they are read from CharacterCreationContent.
        //   - gold has no setter on NarrativeMenuOptionArgs — not applied in 1.4.5.
        // =====================================================================

        private NarrativeMenuOption MakeOption(
            string stringId,
            string text,
            string description,
            string occupationId,
            List<SkillObject> skills,
            CharacterAttribute attribute,
            int focusToAdd,
            int skillLevelToAdd,
            int attributeLevelToAdd,
            NarrativeMenuOptionOnConditionDelegate condition = null,
            NarrativeMenuOptionOnSelectDelegate onSelect = null,
            List<TraitObject> traits = null,
            int traitLevel = 0,
            int renown = 0,
            int gold = 0)
        {
            return new NarrativeMenuOption(
                stringId,
                new TextObject(text),
                new TextObject(description),
                args =>
                {
                    args.SetAffectedSkills(skills.ToArray());
                    args.SetFocusToSkills(focusToAdd);
                    args.SetLevelToSkills(skillLevelToAdd);
                    args.SetLevelToAttribute(attribute, attributeLevelToAdd);
                    if (traits != null)
                    {
                        args.SetAffectedTraits(traits.ToArray());
                        args.SetLevelToTraits(traitLevel);
                    }
                    args.SetRenownToAdd(renown);
                },
                condition ?? (mgr => true),
                onSelect ?? (mgr => { }),
                null
            );
        }

        // =====================================================================
        // CULTURE: AIEL (aserai)
        // =====================================================================

        // =====================================================================
        // AddParentsMenu -- all 6 cultures, culture-filtered via onCondition
        // =====================================================================
        public void AddParentsMenu(CharacterCreationManager manager, int focusToAdd, int skillLevelToAdd, int attributeLevelToAdd)
        {
            // Build parent characters from player body properties
            BodyProperties motherProps;
            BodyProperties fatherProps;
            FaceGen.GenerateParentKey(
                motherProps = fatherProps = CharacterObject.PlayerCharacter.GetBodyProperties(
                    CharacterObject.PlayerCharacter.Equipment, -1),
                CharacterObject.PlayerCharacter.Race,
                ref motherProps, ref fatherProps);

            motherProps = new BodyProperties(new DynamicBodyProperties(33f, 0.3f, 0.2f), motherProps.StaticProperties);
            fatherProps = new BodyProperties(new DynamicBodyProperties(33f, 0.5f, 0.5f), fatherProps.StaticProperties);

            var characters = new List<NarrativeMenuCharacter>
            {
                new NarrativeMenuCharacter("mother_character", motherProps, CharacterObject.PlayerCharacter.Race, true),
                new NarrativeMenuCharacter("father_character", fatherProps, CharacterObject.PlayerCharacter.Race, false)
            };

            var menu = MakeMenu(
                "narrative_parent_menu", "start", "narrative_youngchild_menu",
                "Your Heritage",
                "You were born into a family of...",
                new NarrativeMenu.GetNarrativeMenuCharacterArgsDelegate(this.GetParentMenuNarrativeMenuCharacterArgs), characters);

            // AIEL -- visible when SelectedCulture == "aserai"
            menu.AddNarrativeMenuOption(MakeOption("aiel_parents_warriors", "Warriors",
                "You were born into a family of warriors. Your parents were both skilled fighters, and they trained you in the art of combat from a young age. You learned about the importance of honor and loyalty, and you grew up with a strong sense of duty to your family and your people.",
                "aiel_warriors",
                new List<SkillObject> { OneHanded, Polearm }, Vigor, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "aserai",
                mgr => SetParentOccupationAndEquipment(mgr, "aiel_warriors", "act_character_creation_female_default_side_to_side_1", "act_character_creation_male_default_side_to_side_1")));
            menu.AddNarrativeMenuOption(MakeOption("aiel_parents_trackers", "Trackers",
                "You were born into a family of trackers. Your parents were skilled scouts, who taught you how to read the land and follow trails. You learned about the importance of patience and observation, and you grew up with a deep connection to nature.",
                "aiel_trackers",
                new List<SkillObject> { Scouting, Bow }, Cunning, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "aserai",
                mgr => SetParentOccupationAndEquipment(mgr, "aiel_trackers", "act_character_creation_female_default_side_to_side_1", "act_character_creation_male_default_side_to_side_1")));
            menu.AddNarrativeMenuOption(MakeOption("aiel_parents_healers", "Healers",
                "You were born into a family of healers. Your parents taught you about the human body and how to treat injuries and illnesses. You learned about the importance of compassion and care, and you grew up with a desire to help others.",
                "aiel_healers",
                new List<SkillObject> { Medicine, Charm }, Intelligence, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "aserai",
                mgr => SetParentOccupationAndEquipment(mgr, "aiel_healers", "act_character_creation_female_default_side_to_side_1", "act_character_creation_male_default_side_to_side_1")));
            menu.AddNarrativeMenuOption(MakeOption("aiel_parents_farmers", "Oasis farmers",
                "You were born into a family of oasis farmers. Your parents taught you how to cultivate crops and manage water resources in the harsh desert environment. You learned about the importance of hard work and resourcefulness, and you grew up with a strong connection to the land.",
                    "aiel_farmers",
                new List<SkillObject> { Engineering, Trade }, Social, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "aserai",
                mgr => SetParentOccupationAndEquipment(mgr, "aiel_farmers", "act_character_creation_female_default_side_to_side_1", "act_character_creation_male_default_side_to_side_1")));
            menu.AddNarrativeMenuOption(MakeOption("aiel_parents_hunters", "Hunters",
                "You were born into a family of hunters. Your parents taught you how to track and hunt game in the waste. You learned about the importance of patience and skill, and you grew up with a deep respect for nature.",
                "aiel_hunters",
                new List<SkillObject> { Athletics, Throwing }, Control, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "aserai",
                mgr => SetParentOccupationAndEquipment(mgr, "aiel_hunters", "act_character_creation_female_default_side_to_side_1", "act_character_creation_male_default_side_to_side_1")));
            menu.AddNarrativeMenuOption(MakeOption("aiel_parents_smiths", "Smiths",
                "You were born into a family of smiths. Your parents taught you how to forge weapons and tools, and you learned about the importance of craftsmanship and attention to detail. You grew up with a strong work ethic and a desire to create things of beauty and function.",
                "aiel_smiths",
                new List<SkillObject> { Smithing, Engineering }, Endurance, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "aserai",
                mgr => SetParentOccupationAndEquipment(mgr, "aiel_smiths", "act_character_creation_female_default_side_to_side_1", "act_character_creation_male_default_side_to_side_1")));

            // SHADOW -- visible when SelectedCulture == "sturgia"
            menu.AddNarrativeMenuOption(MakeOption("shadow_parents_nobles", "Noble soldiers",
                "Your father served with a Lord's personal guard. Growing up in the castle barracks you were trained in the art of combat from a young age. You learned about the importance of honor and loyalty.",
                "noble_soldiers",
                new List<SkillObject> { OneHanded, Riding }, Vigor, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "sturgia",
                mgr => SetParentOccupationAndEquipment(mgr, "noble_soldiers", "act_character_creation_female_default_side_to_side_1", "act_character_creation_male_default_side_to_side_1")));
            menu.AddNarrativeMenuOption(MakeOption("shadow_parents_thieves", "Back alley thieves",
                "You were born into a family of back alley thieves. Your parents taught you the skills necessary for a life of crime, and more importantly, how to survive in the underbelly of large cities.",
                "thieves",
                new List<SkillObject> { Roguery, Throwing }, Cunning, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "sturgia",
                mgr => SetParentOccupationAndEquipment(mgr, "thieves", "act_character_creation_female_default_side_to_side_1", "act_character_creation_male_default_side_to_side_1")));
            menu.AddNarrativeMenuOption(MakeOption("shadow_parents_healers", "Village healers",
                "You were born into a family of healers. Your parents taught you how to treat injuries and illnesses with local herbs. You grew up with a desire to help others.",
                "healers",
                new List<SkillObject> { Medicine, Scouting }, Intelligence, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "sturgia",
                mgr => SetParentOccupationAndEquipment(mgr, "healers", "act_character_creation_female_default_side_to_side_1", "act_character_creation_male_default_side_to_side_1")));
            menu.AddNarrativeMenuOption(MakeOption("shadow_parents_nobility", "A minor noble house",
                "You were born into a family of minor nobility. Your parents taught you about governance, diplomacy, and the responsibilities of leadership.",
                "nobility",
                new List<SkillObject> { Charm, Steward }, Social, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "sturgia",
                mgr => SetParentOccupationAndEquipment(mgr, "nobility", "act_character_creation_female_default_side_to_side_1", "act_character_creation_male_default_side_to_side_1")));
            menu.AddNarrativeMenuOption(MakeOption("shadow_parents_farmers", "Farmers",
                "You were born into a family of farmers. Your parents taught you the value of hard work and the importance of tending to the land, and of course, how to shoot a longbow. You grew up with a strong connection to nature and a deep understanding of agricultural practices.",
                "farmers",  
                new List<SkillObject> { Athletics, Bow }, Control, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "sturgia",
                mgr => SetParentOccupationAndEquipment(mgr, "farmers", "act_character_creation_female_default_side_to_side_1", "act_character_creation_male_default_side_to_side_1")));
            menu.AddNarrativeMenuOption(MakeOption("shadow_parents_peddlers", "Peddlers",
                "You were born into a family of peddlers. Your parents taught you how to use a forge and understand how to sell the products they made. You grew up with a strong work ethic and a desire to profit from your hard work.",
                "peddlers",
                new List<SkillObject> { Smithing, Trade }, Endurance, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "sturgia",
                mgr => SetParentOccupationAndEquipment(mgr, "peddlers", "act_character_creation_female_default_side_to_side_1", "act_character_creation_male_default_side_to_side_1")));

            // BORDERLANDS -- visible when SelectedCulture == "khuzait"
            menu.AddNarrativeMenuOption(MakeOption("borderlands_parents_nobles", "Noble soldiers",
                "Your father served with a Lord's personal guard. Growing up in the castle barracks you were trained in the art of combat from a young age. You learned about the importance of honor and loyalty.",
                "noble_soldiers",
                new List<SkillObject> { OneHanded, Riding }, Vigor, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "khuzait",
                mgr => SetParentOccupationAndEquipment(mgr, "noble_soldiers", "act_character_creation_female_default_side_to_side_1", "act_character_creation_male_default_side_to_side_1")));
            menu.AddNarrativeMenuOption(MakeOption("borderlands_parents_thieves", "Bandits",
                "You were born into a family of bandits. Your parents taught you the skills necessary for a life of crime, and more importantly, how to survive in the underbelly of society.",
                "bandits",
                new List<SkillObject> { Roguery, Throwing }, Cunning, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "khuzait",
                mgr => SetParentOccupationAndEquipment(mgr, "bandits", "act_character_creation_female_default_side_to_side_1", "act_character_creation_male_default_side_to_side_1")));
            menu.AddNarrativeMenuOption(MakeOption("borderlands_parents_healers", "Village healers",
                "You were born into a family of healers. Your parents taught you how to treat injuries and illnesses with local herbs. You grew up with a desire to help others.",
                "healers",
                new List<SkillObject> { Medicine, Scouting }, Intelligence, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "khuzait",
                mgr => SetParentOccupationAndEquipment(mgr, "healers", "act_character_creation_female_default_side_to_side_1", "act_character_creation_male_default_side_to_side_1")));
            menu.AddNarrativeMenuOption(MakeOption("borderlands_parents_nobility", "A minor noble house",
                "You were born into a family of minor nobility. Your parents taught you about governance, diplomacy, and the responsibilities of leadership.",
                "nobility",
                new List<SkillObject> { Charm, Steward }, Social, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "khuzait",
                mgr => SetParentOccupationAndEquipment(mgr, "nobility", "act_character_creation_female_default_side_to_side_1", "act_character_creation_male_default_side_to_side_1")));
            menu.AddNarrativeMenuOption(MakeOption("borderlands_parents_farmers", "Farmers",
                "You were born into a family of farmers. Your parents taught you the value of hard work and the importance of tending to the land, and of course, how to shoot a longbow. You grew up with a strong connection to nature and a deep understanding of agricultural practices.",
                "farmers",
                new List<SkillObject> { Athletics, Bow }, Control, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "khuzait",
                mgr => SetParentOccupationAndEquipment(mgr, "farmers", "act_character_creation_female_default_side_to_side_1", "act_character_creation_male_default_side_to_side_1")));
            menu.AddNarrativeMenuOption(MakeOption("borderlands_parents_peddlers", "Peddlers",
                "You were born into a family of peddlers. Your parents taught you how to use a forge and understand how to sell the products they made. You grew up with a strong work ethic and a desire to profit from your hard work.",
                "peddlers",
                new List<SkillObject> { Smithing, Trade }, Endurance, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "khuzait",
                mgr => SetParentOccupationAndEquipment(mgr, "peddlers", "act_character_creation_female_default_side_to_side_1", "act_character_creation_male_default_side_to_side_1")));

            // WESTLANDS -- visible when SelectedCulture == "empire"
            menu.AddNarrativeMenuOption(MakeOption("westlands_parents_nobles", "Noble soldiers",
                "Your father served with a Lord's personal guard. Growing up in the castle barracks you were trained in the art of combat from a young age. You learned about the importance of honor and loyalty.",
                "noble_soldiers",
                new List<SkillObject> { OneHanded, Riding }, Vigor, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "empire",
                mgr => SetParentOccupationAndEquipment(mgr, "noble_soldiers", "act_character_creation_female_default_side_to_side_1", "act_character_creation_male_default_side_to_side_1")));
            menu.AddNarrativeMenuOption(MakeOption("westlands_parents_thieves", "Vagabonds",
                "You were born into a family of back alley thieves. Your parents taught you the skills necessary for a life of crime, and more importantly, how to survive in the underbelly of large cities.",
                "vagabonds",
                new List<SkillObject> { Roguery, Throwing }, Cunning, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "empire",
                mgr => SetParentOccupationAndEquipment(mgr, "vagabonds", "act_character_creation_female_default_side_to_side_1", "act_character_creation_male_default_side_to_side_1")));
            menu.AddNarrativeMenuOption(MakeOption("westlands_parents_healers", "Village wisdom",
                "You were born into a family of healers. Your parents taught you how to treat injuries and illnesses with local herbs. You grew up with a desire to help others.",
                "healers",
                new List<SkillObject> { Medicine, Scouting }, Intelligence, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "empire",
                mgr => SetParentOccupationAndEquipment(mgr, "healers", "act_character_creation_female_default_side_to_side_1", "act_character_creation_male_default_side_to_side_1")));
            menu.AddNarrativeMenuOption(MakeOption("westlands_parents_nobility", "A minor noble house",
                "You were born into a family of minor nobility. Your parents taught you about governance, diplomacy, and the responsibilities of leadership.",
                "nobility",
                new List<SkillObject> { Charm, Steward }, Social, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "empire",
                mgr => SetParentOccupationAndEquipment(mgr, "nobility", "act_character_creation_female_default_side_to_side_1", "act_character_creation_male_default_side_to_side_1")));
            menu.AddNarrativeMenuOption(MakeOption("westlands_parents_farmers", "Sheep herders",
                "You were born into a family of farmers. Your parents taught you the value of hard work and the importance of tending to the land, and of course, how to shoot a longbow. You grew up with a strong connection to nature and a deep understanding of agricultural practices.",
                "farmers",
                new List<SkillObject> { Athletics, Bow }, Control, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "empire",
                mgr => SetParentOccupationAndEquipment(mgr, "farmers", "act_character_creation_female_default_side_to_side_1", "act_character_creation_male_default_side_to_side_1")));
            menu.AddNarrativeMenuOption(MakeOption("westlands_parents_peddlers", "Tinkers",
                "You were born into a family of tinkers. Your parents taught you how to use a forge and understand how to sell the products they made. You grew up with a strong work ethic and a desire to profit from your hard work.",
                "tinkers",
                new List<SkillObject> { Smithing, Trade }, Endurance, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "empire",
                mgr => SetParentOccupationAndEquipment(mgr, "tinkers", "act_character_creation_female_default_side_to_side_1", "act_character_creation_male_default_side_to_side_1")));

            // COASTLANDS -- visible when SelectedCulture == "battania"
            menu.AddNarrativeMenuOption(MakeOption("coastlands_parents_nobles", "noble soldiers",
                "Your father served with a Lord's personal guard. Growing up in the castle barracks you were trained in the art of combat from a young age. You learned about the importance of honor and loyalty.",
                "noble_soldiers",
                new List<SkillObject> { OneHanded, Riding }, Vigor, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "battania",
                mgr => SetParentOccupationAndEquipment(mgr, "noble_soldiers", "act_character_creation_female_default_side_to_side_1", "act_character_creation_male_default_side_to_side_1")));
            menu.AddNarrativeMenuOption(MakeOption("coastlands_parents_thieves", "back alley thieves",
                "You were born into a family of back alley thieves. Your parents taught you the skills necessary for a life of crime, and more importantly, how to survive in the underbelly of large cities.",
                "thieves",
                new List<SkillObject> { Roguery, Throwing }, Cunning, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "battania",
                mgr => SetParentOccupationAndEquipment(mgr, "thieves", "act_character_creation_female_default_side_to_side_1", "act_character_creation_male_default_side_to_side_1")));
            menu.AddNarrativeMenuOption(MakeOption("coastlands_parents_healers", "village healers",
                "You were born into a family of healers. Your parents taught you how to treat injuries and illnesses with local herbs. You grew up with a desire to help others.",
                "healers",
                new List<SkillObject> { Medicine, Athletics }, Intelligence, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "battania",
                mgr => SetParentOccupationAndEquipment(mgr, "healers", "act_character_creation_female_default_side_to_side_1", "act_character_creation_male_default_side_to_side_1")));
            menu.AddNarrativeMenuOption(MakeOption("coastlands_parents_nobility", "a minor noble house",
                "You were born into a family of minor nobility. Your parents taught you about governance, diplomacy, and the responsibilities of leadership.",
                "nobility",
                new List<SkillObject> { Charm, Steward }, Social, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "battania",
                mgr => SetParentOccupationAndEquipment(mgr, "nobility", "act_character_creation_female_default_side_to_side_1", "act_character_creation_male_default_side_to_side_1")));
            menu.AddNarrativeMenuOption(MakeOption("coastlands_parents_hunters", "hunters",
                "You were born into a family of hunters. Your parents taught you how to use a bow to track prey, not only to provide an income, but more importantly, to survive.",
                "hunters",
                new List<SkillObject> { Scouting, Bow }, Control, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "battania",
                mgr => SetParentOccupationAndEquipment(mgr, "hunters", "act_character_creation_female_default_side_to_side_1", "act_character_creation_male_default_side_to_side_1")));
            menu.AddNarrativeMenuOption(MakeOption("coastlands_parents_guildsmen", "guildsmen",
                "You were born into a family of guildsmen. Your parents taught you how to use a forge and understand how to sell the products they made. You grew up with a strong work ethic and a desire to profit from your hard work.",
                "guildsmen",
                new List<SkillObject> { Smithing, Trade }, Endurance, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "battania",
                mgr => SetParentOccupationAndEquipment(mgr, "guildsmen", "act_character_creation_female_default_side_to_side_1", "act_character_creation_male_default_side_to_side_1")));

            // SEANCHAN -- visible when SelectedCulture == "vlandia"
            menu.AddNarrativeMenuOption(MakeOption("seanchan_parents_deathguard", "the Deathwatch Guard",
                "Your father served with the Deathwatch Guard. Growing up in the barracks you were trained in the art of combat from a young age. You learned about the importance of honor and the importance of serving the Blood.",
                "deathwatch_guard",
                new List<SkillObject> { OneHanded, Riding }, Vigor, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "vlandia",
                mgr => SetParentOccupationAndEquipment(mgr, "deathwatch_guard", "act_character_creation_female_default_side_to_side_1", "act_character_creation_male_default_side_to_side_1")));
            menu.AddNarrativeMenuOption(MakeOption("seanchan_parents_thieves", "back alley thieves",
                "You were born into a family of thieves. Your parents taught you the skills necessary for a life of crime, and more importantly, how to survive in the underbelly of large Seanchan cities.",
                "thieves",
                new List<SkillObject> { Roguery, Throwing }, Cunning, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "vlandia",
                mgr => SetParentOccupationAndEquipment(mgr, "thieves", "act_character_creation_female_default_side_to_side_1", "act_character_creation_male_default_side_to_side_1")));
            menu.AddNarrativeMenuOption(MakeOption("seanchan_parents_healers", "village healers",
                "You were born into a family of healers. Your parents taught you how to treat injuries and illnesses with local herbs. You grew up with a desire to help others.",
                "healers",
                new List<SkillObject> { Medicine, Scouting }, Intelligence, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "vlandia",
                mgr => SetParentOccupationAndEquipment(mgr, "healers", "act_character_creation_female_default_side_to_side_1", "act_character_creation_male_default_side_to_side_1")));
            menu.AddNarrativeMenuOption(MakeOption("seanchan_parents_nobility", "a minor noble house",
                "You were born into a family of minor nobility. Your parents taught you about governance, diplomacy, and the responsibilities of leadership.",
                "nobility",
                new List<SkillObject> { Charm, Steward }, Social, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "vlandia",
                mgr => SetParentOccupationAndEquipment(mgr, "nobility", "act_character_creation_female_default_side_to_side_1", "act_character_creation_male_default_side_to_side_1")));
            menu.AddNarrativeMenuOption(MakeOption("seanchan_parents_soldiers", "career soldiers",
                "You were born into a family of military parents. Your parents taught you how to use a bow to keep you occupied on the long marches, moving from place to place depending on the orders from the nobility above.",
                "soldiers",
                new List<SkillObject> { Athletics, Bow }, Control, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "vlandia",
                mgr => SetParentOccupationAndEquipment(mgr, "soldiers", "act_character_creation_female_default_side_to_side_1", "act_character_creation_male_default_side_to_side_1")));
            menu.AddNarrativeMenuOption(MakeOption("seanchan_parents_engineers", "engineers",
                "You were born into a family of engineers. Your parents taught you how to use a forge and understand design drawings. You grew up with a strong work ethic and a desire to create things of beauty and function.",
                "engineers",
                new List<SkillObject> { Smithing, Engineering }, Endurance, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "vlandia",
                mgr => SetParentOccupationAndEquipment(mgr, "engineers", "act_character_creation_female_default_side_to_side_1", "act_character_creation_male_default_side_to_side_1")));

            manager.AddNewMenu(menu);
        }

        // =====================================================================
        // AddYoungChildMenu -- all 6 cultures, culture-filtered via onCondition
        // =====================================================================
        public void AddYoungChildMenu(CharacterCreationManager manager, int focusToAdd, int skillLevelToAdd, int attributeLevelToAdd)
        {
            BodyProperties playerProps = CharacterObject.PlayerCharacter.GetBodyProperties(
            CharacterObject.PlayerCharacter.Equipment, -1);
                        
            var characters = new List<NarrativeMenuCharacter>
            {
                new NarrativeMenuCharacter("player_childhood_character", playerProps,
                    CharacterObject.PlayerCharacter.Race,
                    CharacterObject.PlayerCharacter.IsFemale)
            };

            var menu = MakeMenu(
                "narrative_youngchild_menu", "narrative_parent_menu", "narrative_childhood_menu",
                "Early Childhood",
                "As a child you were noted for...",
                new NarrativeMenu.GetNarrativeMenuCharacterArgsDelegate(this.GetYoungChildCharacterArgs), characters);

            // AIEL -- visible when SelectedCulture == "aserai"
            menu.AddNarrativeMenuOption(MakeOption("aiel_yc_leadership", "your leadership skills.",
                "As a child, you were always the one who took charge. Whether it was organizing games with other children or helping your parents with tasks around the home, you had a natural ability to lead and inspire others.",
                "", new List<SkillObject> { Leadership, Tactics }, Cunning, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "aserai"));
            menu.AddNarrativeMenuOption(MakeOption("aiel_yc_brawn", "your brawn.",
                "As a child, you were always the strongest and most athletic among your peers. You loved to run, climb, and play rough games, and you had a natural talent for physical activities.",
                    "", new List<SkillObject> { OneHanded, Throwing }, Vigor, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "aserai"));
            menu.AddNarrativeMenuOption(MakeOption("aiel_yc_detail", "your attention to detail.",
                "As a child, you were always the one who noticed the little things. You had a keen eye for detail and a natural talent for observation, which made you excellent at tasks that required precision and focus.",
                "", new List<SkillObject> { Athletics, Bow }, Control, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "aserai"));
            menu.AddNarrativeMenuOption(MakeOption("aiel_yc_numbers", "your aptitude for numbers.",
                "As a child, you were always the one who excelled at math and problem-solving. You had a natural talent for numbers and a logical mind, which made you excellent at tasks that required analysis and strategy.",
                "", new List<SkillObject> { Engineering, Trade }, Intelligence, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "aserai"));
            menu.AddNarrativeMenuOption(MakeOption("aiel_yc_people", "your way with people.",
                "As a child, you were always the one who got along with everyone. You had a natural charm and charisma that made you popular among your peers, and you had a talent for making friends and influencing others.",
                "", new List<SkillObject> { Charm, Leadership }, Social, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "aserai"));
            menu.AddNarrativeMenuOption(MakeOption("aiel_yc_waste", "your ability to read the waste.",
                "As a child, you were always the one who could navigate and understand the harsh desert environment. You had a natural talent for survival and a deep connection to the land.",
                "", new List<SkillObject> { Scouting, Medicine }, Endurance, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "aserai"));

            // SHADOW -- visible when SelectedCulture == "sturgia"
            menu.AddNarrativeMenuOption(MakeOption("shadow_yc_leadership", "your leadership skills.",
                "As a child, you were always the one who took charge. Whether it was organizing games with other children or helping your parents with tasks around the home, you had a natural ability to lead and inspire others.",
                "", new List<SkillObject> { Leadership, Tactics }, Cunning, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "sturgia"));
            menu.AddNarrativeMenuOption(MakeOption("shadow_yc_brawn", "your brawn.",
                "As a child, you were always the strongest and most athletic among your peers. You loved to run, climb, and play rough games, and you had a natural talent for physical activities.",
                    "", new List<SkillObject> { OneHanded, Throwing }, Vigor, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "sturgia"));
            menu.AddNarrativeMenuOption(MakeOption("shadow_yc_detail", "your attention to detail.",
                "As a child, you were always the one who noticed the little things. You had a keen eye for detail and a natural talent for observation, which made you excellent at tasks that required precision and focus.",
                    "", new List<SkillObject> { Athletics, Bow }, Control, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "sturgia"));
            menu.AddNarrativeMenuOption(MakeOption("shadow_yc_numbers", "your aptitude for numbers.",
                "As a child, you were always the one who excelled at math and problem-solving. You had a natural talent for numbers and a logical mind, which made you excellent at tasks that required analysis and strategy.",
                    "", new List<SkillObject> { Engineering, Trade }, Intelligence, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "sturgia"));
            menu.AddNarrativeMenuOption(MakeOption("shadow_yc_people", "your way with people.",
                "As a child, you were always the one who got along with everyone. You had a natural charm and charisma that made you popular among your peers, and you had a talent for making friends and influencing others.",
                    "", new List<SkillObject> { Charm, Leadership }, Social, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "sturgia"));
            menu.AddNarrativeMenuOption(MakeOption("shadow_yc_land", "your ability to read the land.",
                "As a child, you were always the one who could navigate and understand the dangerous environment. You had a natural talent for survival and a deep connection to the land.",
                    "", new List<SkillObject> { Scouting, Medicine }, Endurance, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "sturgia"));

            // BORDERLANDS -- visible when SelectedCulture == "khuzait"
            menu.AddNarrativeMenuOption(MakeOption("borderlands_yc_leadership", "your leadership skills.",
                "As a child, you were always the one who took charge. Whether it was organizing games with other children or helping your parents with tasks around the home, you had a natural ability to lead and inspire others.",
                    "", new List<SkillObject> { Leadership, Tactics }, Cunning, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "khuzait"));
            menu.AddNarrativeMenuOption(MakeOption("borderlands_yc_brawn", "your brawn.",
                "As a child, you were always the strongest and most athletic among your peers. You loved to run, climb, and play rough games, and you had a natural talent for physical activities.",
                        "", new List<SkillObject> { OneHanded, Throwing }, Vigor, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "khuzait"));
            menu.AddNarrativeMenuOption(MakeOption("borderlands_yc_detail", "your attention to detail.",
                "As a child, you were always the one who noticed the little things. You had a keen eye for detail and a natural talent for observation, which made you excellent at tasks that required precision and focus.",
                    "", new List<SkillObject> { Athletics, Bow }, Control, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "khuzait"));
            menu.AddNarrativeMenuOption(MakeOption("borderlands_yc_numbers", "your aptitude for numbers.",
                "As a child, you were always the one who excelled at math and problem-solving. You had a natural talent for numbers and a logical mind, which made you excellent at tasks that required analysis and strategy.",
                    "", new List<SkillObject> { Engineering, Trade }, Intelligence, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "khuzait"));
            menu.AddNarrativeMenuOption(MakeOption("borderlands_yc_people", "your way with people.",
                "As a child, you were always the one who got along with everyone. You had a natural charm and charisma that made you popular among your peers, and you had a talent for making friends and influencing others.",
                    "", new List<SkillObject> { Charm, Leadership }, Social, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "khuzait"));
            menu.AddNarrativeMenuOption(MakeOption("borderlands_yc_land", "your ability to read the land.",
                "As a child, you were always the one who could navigate and understand the dangerous environment. You had a natural talent for survival and a deep connection to the land.",
                    "", new List<SkillObject> { Scouting, Medicine }, Endurance, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "khuzait"));

            // WESTLANDS -- visible when SelectedCulture == "empire"
            menu.AddNarrativeMenuOption(MakeOption("westlands_yc_leadership", "your leadership skills.",
                "As a child, you were always the one who took charge. Whether it was organizing games with other children or helping your parents with tasks around the home, you had a natural ability to lead and inspire others.",
                    "", new List<SkillObject> { Leadership, Tactics }, Cunning, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "empire"));
            menu.AddNarrativeMenuOption(MakeOption("westlands_yc_brawn", "your brawn.",
                "As a child, you were always the strongest and most athletic among your peers. You loved to run, climb, and play rough games, and you had a natural talent for physical activities.",
                    "", new List<SkillObject> { OneHanded, Throwing }, Vigor, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "empire"));
            menu.AddNarrativeMenuOption(MakeOption("westlands_yc_detail", "your attention to detail.",
                "As a child, you were always the one who noticed the little things. You had a keen eye for detail and a natural talent for observation, which made you excellent at tasks that required precision and focus.",
                    "", new List<SkillObject> { Athletics, Bow }, Control, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "empire"));
            menu.AddNarrativeMenuOption(MakeOption("westlands_yc_numbers", "your aptitude for numbers.",
                "As a child, you were always the one who excelled at math and problem-solving. You had a natural talent for numbers and a logical mind, which made you excellent at tasks that required analysis and strategy.",
                    "", new List<SkillObject> { Engineering, Trade }, Intelligence, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "empire"));
            menu.AddNarrativeMenuOption(MakeOption("westlands_yc_people", "your way with people.",
                "As a child, you were always the one who got along with everyone. You had a natural charm and charisma that made you popular among your peers, and you had a talent for making friends and influencing others.",
                    "", new List<SkillObject> { Charm, Leadership }, Social, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "empire"));
            menu.AddNarrativeMenuOption(MakeOption("westlands_yc_land", "your ability to read the land.",
                "As a child, you were always the one who could navigate and understand the environment. You had a natural talent for survival and a deep connection to the land.",
                    "", new List<SkillObject> { Scouting, Medicine }, Endurance, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "empire"));

            // COASTLANDS -- visible when SelectedCulture == "battania"
            menu.AddNarrativeMenuOption(MakeOption("coastlands_yc_leadership", "your leadership skills.",
                "As a child, you were always the one who took charge. Whether it was organizing games with other children or helping your parents with tasks around the home, you had a natural ability to lead and inspire others.",
                    "", new List<SkillObject> { Leadership, Tactics }, Cunning, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "battania"));
            menu.AddNarrativeMenuOption(MakeOption("coastlands_yc_brawn", "your brawn.",
                "As a child, you were always the strongest and most athletic among your peers. You loved to run, climb, and play rough games, and you had a natural talent for physical activities.",
                    "", new List<SkillObject> { OneHanded, Throwing }, Vigor, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "battania"));
            menu.AddNarrativeMenuOption(MakeOption("coastlands_yc_detail", "your attention to detail.",
                "As a child, you were always the one who noticed the little things. You had a keen eye for detail and a natural talent for observation, which made you excellent at tasks that required precision and focus.",
                    "", new List<SkillObject> { Athletics, Bow }, Control, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "battania"));
            menu.AddNarrativeMenuOption(MakeOption("coastlands_yc_numbers", "your aptitude for numbers.",
                "As a child, you were always the one who excelled at math and problem-solving. You had a natural talent for numbers and a logical mind, which made you excellent at tasks that required analysis and strategy.",
                    "", new List<SkillObject> { Engineering, Trade }, Intelligence, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "battania"));
            menu.AddNarrativeMenuOption(MakeOption("coastlands_yc_people", "your way with people.",
                "As a child, you were always the one who got along with everyone. You had a natural charm and charisma that made you popular among your peers, and you had a talent for making friends and influencing others.",
                    "", new List<SkillObject> { Charm, Leadership }, Social, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "battania"));
            menu.AddNarrativeMenuOption(MakeOption("coastlands_yc_land", "your ability to read the land.",
                "As a child, you were always the one who could navigate and understand the environment. You had a natural talent for survival and a deep connection to the land.",
                    "", new List<SkillObject> { Scouting, Medicine }, Endurance, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "battania"));

            // SEANCHAN -- visible when SelectedCulture == "vlandia"
            menu.AddNarrativeMenuOption(MakeOption("seanchan_yc_leadership", "your leadership skills.",
                "As a child, you were always the one who took charge. Whether it was organizing games with other children or helping your parents with tasks around the home, you had a natural ability to lead and inspire others.",
                    "", new List<SkillObject> { Leadership, Tactics }, Cunning, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "vlandia"));
            menu.AddNarrativeMenuOption(MakeOption("seanchan_yc_brawn", "your brawn.",
                "As a child, you were always the strongest and most athletic among your peers. You loved to run, climb, and play rough games, and you had a natural talent for physical activities.",
                    "", new List<SkillObject> { OneHanded, Throwing }, Vigor, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "vlandia"));
            menu.AddNarrativeMenuOption(MakeOption("seanchan_yc_detail", "your attention to detail.",
                "As a child, you were always the one who noticed the little things. You had a keen eye for detail and a natural talent for observation, which made you excellent at tasks that required precision and focus.",
                    "", new List<SkillObject> { Athletics, Bow }, Control, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "vlandia"));
            menu.AddNarrativeMenuOption(MakeOption("seanchan_yc_numbers", "your aptitude for numbers.",
                "As a child, you were always the one who excelled at math and problem-solving. You had a natural talent for numbers and a logical mind, which made you excellent at tasks that required analysis and strategy.",
                    "", new List<SkillObject> { Engineering, Trade }, Intelligence, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "vlandia"));
            menu.AddNarrativeMenuOption(MakeOption("seanchan_yc_people", "your way with people.",
                "As a child, you were always the one who got along with everyone. You had a natural charm and charisma that made you popular among your peers, and you had a talent for making friends and influencing others.",
                    "", new List<SkillObject> { Charm, Leadership }, Social, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "vlandia"));
            menu.AddNarrativeMenuOption(MakeOption("seanchan_yc_land", "your ability to read the land.",
                "As a child, you were always the one who could navigate and understand the dangerous Seanchan environment. You had a natural talent for survival and a deep connection to the land.",
                    "", new List<SkillObject> { Scouting, Medicine }, Endurance, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "vlandia"));

            manager.AddNewMenu(menu);
        }

        // =====================================================================
        // AddChildhoodMenu -- all 6 cultures, culture-filtered via onCondition
        // =====================================================================
        public void AddChildhoodMenu(CharacterCreationManager manager, int focusToAdd, int skillLevelToAdd, int attributeLevelToAdd)
        {
            BodyProperties playerProps = CharacterObject.PlayerCharacter.GetBodyProperties(
            CharacterObject.PlayerCharacter.Equipment, -1);
            var characters = new List<NarrativeMenuCharacter>
            {
                new NarrativeMenuCharacter("player_childhood_character", playerProps,
                    CharacterObject.PlayerCharacter.Race,
                    CharacterObject.PlayerCharacter.IsFemale)
            };

            var menu = MakeMenu(
                "narrative_childhood_menu", "narrative_youngchild_menu", "narrative_teenager_menu",
                "Childhood",
                "You also helped out by...",
                new NarrativeMenu.GetNarrativeMenuCharacterArgsDelegate(this.GetChildhoodCharacterArgs), characters);

            // AIEL -- visible when SelectedCulture == "aserai"
            menu.AddNarrativeMenuOption(MakeOption("aiel_ch_sheep", "herded sheep and goats.",
                "You went with other nimble children to take the sheep and goats out to graze the wastes' plantlife. You had to chase errant animals, and defended the herd from lurking predators.",
                    "", new List<SkillObject> { Athletics, Throwing }, Control, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "aserai"));
            menu.AddNarrativeMenuOption(MakeOption("aiel_ch_smithy", "worked in the village smithy.",
                "You spent your childhood working in the village smithy, learning the art of forging weapons and tools. You developed a strong arm and a deep appreciation for craftsmanship.",
                    "", new List<SkillObject> { OneHanded, Smithing }, Vigor, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "aserai"));
            menu.AddNarrativeMenuOption(MakeOption("aiel_ch_repair", "repaired projects.",
                "You spent your childhood repairing projects around the village, digging wells, fixing broken tools and learning the basics of construction. You developed a sense of what it takes to keep the community prosperous.",
                "", new List<SkillObject> { Engineering, Smithing }, Intelligence, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "aserai"));
            menu.AddNarrativeMenuOption(MakeOption("aiel_ch_herbs", "gathered herbs in the wild.",
                "You spent your childhood gathering herbs in the waste, learning about the different plants and their uses. You developed a talent for finding resources in the harsh desert environment.",
                "", new List<SkillObject> { Medicine, Scouting }, Endurance, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "aserai"));
            menu.AddNarrativeMenuOption(MakeOption("aiel_ch_hunting", "hunted small game.",
                "You helped the local hunters in the wastes hunting small game, learning how to track, trap and kill animals for food. You developed a talent for survival and a deep respect for the waste.",
                "", new List<SkillObject> { Bow, Tactics }, Cunning, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "aserai"));
            menu.AddNarrativeMenuOption(MakeOption("aiel_ch_market", "sold product at the market.",
                "You helped local farmers and artisans selling products at the local market, learning about trade and commerce. You developed a talent for negotiation and reading people.",
                "", new List<SkillObject> { Trade, Charm }, Social, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "aserai"));

            // SHADOW -- visible when SelectedCulture == "sturgia"
            menu.AddNarrativeMenuOption(MakeOption("shadow_ch_horses", "herded horses and sheep.",
                "You went with other nimble children to take the horses and sheep out to graze the steppes. You had to chase errant animals, and defended the herd from lurking predators.",
                "", new List<SkillObject> { Athletics, Throwing }, Control, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "sturgia"));
            menu.AddNarrativeMenuOption(MakeOption("shadow_ch_smithy", "worked in the village smithy.",
                "You spent your childhood working in the village smithy, learning the art of forging weapons and tools. You developed a strong arm and a deep appreciation for craftsmanship.",
                "", new List<SkillObject> { OneHanded, Smithing }, Vigor, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "sturgia"));
            menu.AddNarrativeMenuOption(MakeOption("shadow_ch_repair", "repaired projects.",
                "You spent your childhood repairing projects around the village, digging wells, fixing broken tools and learning the basics of construction. You developed a sense of what it takes to keep the community prosperous.",
                "", new List<SkillObject> { Engineering, Smithing }, Intelligence, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "sturgia"));
            menu.AddNarrativeMenuOption(MakeOption("shadow_ch_herbs", "gathered herbs in the wild.",
                "You spent your childhood gathering herbs in the wilderness, learning about the different plants and their uses. You developed a talent for finding resources in the wilderness.",
                "", new List<SkillObject> { Medicine, Scouting }, Endurance, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "sturgia"));
            menu.AddNarrativeMenuOption(MakeOption("shadow_ch_hunting", "hunted small game.",
                "You helped the local trappers hunting small game, learning how to track, trap and kill animals for food. You developed a talent for survival and a deep respect for the rural environment.",
                "", new List<SkillObject> { Bow, Tactics }, Cunning, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "sturgia"));
            menu.AddNarrativeMenuOption(MakeOption("shadow_ch_market", "sold product at the market.",
                "You helped the local farmers selling products at the local market, learning about trade and commerce. You developed a talent for negotiation and reading people.",
                "", new List<SkillObject> { Trade, Charm }, Social, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "sturgia"));

            // BORDERLANDS -- visible when SelectedCulture == "khuzait"
            menu.AddNarrativeMenuOption(MakeOption("borderlands_ch_horses", "herded horses and sheep.",
                "You went with other nimble children to take the horses and sheep out to graze the steppes. You had to chase errant animals, and defended the herd from lurking predators.",
                "", new List<SkillObject> { Athletics, Throwing }, Control, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "khuzait"));
            menu.AddNarrativeMenuOption(MakeOption("borderlands_ch_smithy", "worked in the village smithy.",
                "You spent your childhood working in the village smithy, learning the art of forging weapons and tools. You developed a strong arm and a deep appreciation for craftsmanship.",
                "", new List<SkillObject> { OneHanded, Smithing }, Vigor, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "khuzait"));
            menu.AddNarrativeMenuOption(MakeOption("borderlands_ch_repair", "repaired projects.",
                "You spent your childhood repairing projects around the village, digging wells, fixing broken tools and learning the basics of construction. You developed a sense of what it takes to keep the community prosperous.",
                "", new List<SkillObject> { Engineering, Smithing }, Intelligence, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "khuzait"));
            menu.AddNarrativeMenuOption(MakeOption("borderlands_ch_herbs", "gathered herbs in the wild.",
                "You spent your childhood gathering herbs in the wilderness, learning about the different plants and their uses. You developed a talent for finding resources in the wilderness.",
                "", new List<SkillObject> { Medicine, Scouting }, Endurance, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "khuzait"));
            menu.AddNarrativeMenuOption(MakeOption("borderlands_ch_hunting", "hunted small game.",
                "You helped the local trappers hunting small game, learning how to track, trap and kill animals for food. You developed a talent for survival and a deep respect for the rural environment.",
                "", new List<SkillObject> { Bow, Tactics }, Cunning, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "khuzait"));
            menu.AddNarrativeMenuOption(MakeOption("borderlands_ch_market", "sold product at the market.",
                "You helped the local farmers selling products at the local market, learning about trade and commerce. You developed a talent for negotiation and reading people.",
                "", new List<SkillObject> { Trade, Charm }, Social, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "khuzait"));

            // WESTLANDS -- visible when SelectedCulture == "empire"
            menu.AddNarrativeMenuOption(MakeOption("westlands_ch_cattle", "herded cattle and sheep.",
                "You went with other nimble children to take the cattle and sheep out to graze the meadows. You had to chase errant animals, and defended the herd from lurking predators.",
                "", new List<SkillObject> { Athletics, Throwing }, Control, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "empire"));
            menu.AddNarrativeMenuOption(MakeOption("westlands_ch_smithy", "worked in the village smithy.",
                "You spent your childhood working in the village smithy, learning the art of forging weapons and tools. You developed a strong arm and a deep appreciation for craftsmanship.",
                "", new List<SkillObject> { OneHanded, Smithing }, Vigor, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "empire"));
            menu.AddNarrativeMenuOption(MakeOption("westlands_ch_repair", "repaired projects.",
                "You spent your childhood repairing projects around the village, digging wells, fixing broken tools and learning the basics of construction. You developed a sense of what it takes to keep the community prosperous.",
                "", new List<SkillObject> { Engineering, Smithing }, Intelligence, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "empire"));
            menu.AddNarrativeMenuOption(MakeOption("westlands_ch_herbs", "gathered herbs in the wild.",
                "You spent your childhood gathering herbs in the wilderness, learning about the different plants and their uses. You developed a talent for finding resources in the wilderness.",
                "", new List<SkillObject> { Medicine, Scouting }, Endurance, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "empire"));
            menu.AddNarrativeMenuOption(MakeOption("westlands_ch_hunting", "hunted small game.",
                "You helped the local trappers hunting small game, learning how to track, trap and kill animals for food. You developed a talent for survival and a deep respect for the rural environment.",
                "", new List<SkillObject> { Bow, Tactics }, Cunning, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "empire"));
            menu.AddNarrativeMenuOption(MakeOption("westlands_ch_market", "sold product at the market.",
                "You helped the local farmers selling products at the local market, learning about trade and commerce. You developed a talent for negotiation and reading people.",
                "", new List<SkillObject> { Trade, Charm }, Social, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "empire"));

            // COASTLANDS -- visible when SelectedCulture == "battania"
            menu.AddNarrativeMenuOption(MakeOption("coastlands_ch_cattle", "herded cattle and sheep.",
                "You went with other nimble children to take the cattle and sheep out to graze the luscious plains. You had to chase errant animals, and defended the herd from lurking predators.",
                "", new List<SkillObject> { Athletics, Throwing }, Control, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "battania"));
            menu.AddNarrativeMenuOption(MakeOption("coastlands_ch_smithy", "worked in the village smithy.",
                "You spent your childhood working in the village smithy, learning the art of forging weapons and tools. You developed a strong arm and a deep appreciation for craftsmanship.",
                "", new List<SkillObject> { OneHanded, Smithing }, Vigor, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "battania"));
            menu.AddNarrativeMenuOption(MakeOption("coastlands_ch_repair", "repaired projects.",
                "You spent your childhood repairing projects around the village, digging wells, fixing broken tools and learning the basics of construction. You developed a sense of what it takes to keep the community prosperous.",
                "", new List<SkillObject> { Engineering, Smithing }, Intelligence, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "battania"));
            menu.AddNarrativeMenuOption(MakeOption("coastlands_ch_herbs", "gathered herbs in the wild.",
                "You spent your childhood gathering herbs in the wilderness, learning about the different plants and their uses. You developed a talent for finding resources in the wilderness.",
                "", new List<SkillObject> { Medicine, Scouting }, Endurance, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "battania"));
            menu.AddNarrativeMenuOption(MakeOption("coastlands_ch_hunting", "hunted small game.",
                "You helped the local trappers hunting small game, learning how to track, trap and kill animals for food. You developed a talent for survival and a deep respect for the rural environment.",
                "", new List<SkillObject> { Bow, Tactics }, Cunning, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "battania"));
            menu.AddNarrativeMenuOption(MakeOption("coastlands_ch_market", "sold product at the market.",
                "You helped the local guild selling products at the local market, learning about trade and commerce. You developed a talent for negotiation and reading people.",
                "", new List<SkillObject> { Trade, Charm }, Social, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "battania"));

            // SEANCHAN -- visible when SelectedCulture == "vlandia"
            menu.AddNarrativeMenuOption(MakeOption("seanchan_ch_sheep", "herded sheep and goats.",
                "You went with other nimble children to take the sheep and goats out to graze. You had to chase errant animals, and defended the herd from lurking predators.",
                "", new List<SkillObject> { Athletics, Throwing }, Control, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "vlandia"));
            menu.AddNarrativeMenuOption(MakeOption("seanchan_ch_smithy", "worked in the village smithy.",
                "You spent your childhood working in the village smithy, learning the art of forging weapons and tools. You developed a strong arm and a deep appreciation for craftsmanship.",
                "", new List<SkillObject> { OneHanded, Smithing }, Vigor, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "vlandia"));
            menu.AddNarrativeMenuOption(MakeOption("seanchan_ch_repair", "repaired projects.",
                "You spent your childhood repairing projects around the village, digging wells, fixing broken tools and learning the basics of construction. You developed a sense of what it takes to keep the community prosperous.",
                "", new List<SkillObject> { Engineering, Smithing }, Intelligence, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "vlandia"));
            menu.AddNarrativeMenuOption(MakeOption("seanchan_ch_herbs", "gathered herbs in the wild.",
                "You spent your childhood gathering herbs in the wilderness, learning about the different plants and their uses. You developed a talent for finding resources in the harsh environment.",
                "", new List<SkillObject> { Medicine, Scouting }, Endurance, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "vlandia"));
            menu.AddNarrativeMenuOption(MakeOption("seanchan_ch_hunting", "hunted small game.",
                "You helped the local hunters in the wastes hunting small game, learning how to track, trap and kill animals for food. You developed a talent for survival and a deep respect for the environment.",
                "", new List<SkillObject> { Bow, Tactics }, Cunning, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "vlandia"));
            menu.AddNarrativeMenuOption(MakeOption("seanchan_ch_market", "sold product at the market.",
                "You helped local farmers and artisans selling products at the local market, learning about trade and commerce. You developed a talent for negotiation and reading people.",
                "", new List<SkillObject> { Trade, Charm }, Social, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "vlandia"));

            manager.AddNewMenu(menu);
        }

        // =====================================================================
        // AddTeenagerMenu -- all 6 cultures, culture-filtered via onCondition
        // =====================================================================
        public void AddTeenagerMenu(CharacterCreationManager manager, int focusToAdd, int skillLevelToAdd, int attributeLevelToAdd)
        {
            BodyProperties playerProps = CharacterObject.PlayerCharacter.GetBodyProperties(
            CharacterObject.PlayerCharacter.Equipment, -1);

            var characters = new List<NarrativeMenuCharacter>
            {
                new NarrativeMenuCharacter("player_youth_character", playerProps,
                    CharacterObject.PlayerCharacter.Race,
                    CharacterObject.PlayerCharacter.IsFemale)
            };

            var menu = MakeMenu(
                "narrative_teenager_menu", "narrative_childhood_menu", "narrative_focus_menu",
                "Adolescence",
                "As a youngster, you...",
                new NarrativeMenu.GetNarrativeMenuCharacterArgsDelegate(this.GetTeenagerCharacterArgs), characters);

            // AIEL -- visible when SelectedCulture == "aserai"
            menu.AddNarrativeMenuOption(MakeOption("aiel_teen_spears", "trained with the Spears.",
                "As a teenager, you trained with the Spears, learning the art of combat and warfare. You honed your skills, looking forward to the day when you could prove yourself in battle and earn your place among the warriors of your people.",
                "spear",
                new List<SkillObject> { Athletics, Polearm }, Vigor, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "aserai",
                mgr => { mgr.CharacterCreationContent.SelectedTitleType = "spear"; UpdatePlayerCharacterEquipment(mgr, "player_youth_character", "act_childhood_polearm"); }));
            menu.AddNarrativeMenuOption(MakeOption("aiel_teen_garrison", "stood guard with the garrisons.",
                "As a teenager, you stood guard with the garrisons, learning about discipline and duty. You learned skills and techniques required to defend a settlement from its attackers.",
                "bowman",
                new List<SkillObject> { Bow, Engineering }, Intelligence, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "aserai",
                mgr => { mgr.CharacterCreationContent.SelectedTitleType = "bowman"; UpdatePlayerCharacterEquipment(mgr, "player_youth_character", "act_childhood_ready_bow"); }));
            menu.AddNarrativeMenuOption(MakeOption("aiel_teen_scouts", "ran with the scouts.",
                "As a teenager, you ran with the scouts, learning about reconnaissance and survival. You honed your skills in tracking and navigating the waste, and you developed a deep connection to the land.",
                "bowman",
                new List<SkillObject> { Scouting, Bow }, Cunning, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "aserai",
                mgr => { mgr.CharacterCreationContent.SelectedTitleType = "bowman"; UpdatePlayerCharacterEquipment(mgr, "player_youth_character", "act_childhood_explorer"); }));
            menu.AddNarrativeMenuOption(MakeOption("aiel_teen_wiseones", "trained with the Wise Ones.",
                "As a teenager, you trained with the Wise Ones, learning how to focus and control the One Power. You honed your skills in channeling, and you developed a strong sense of camaraderie with your fellow trainees.",
                "channeler",
                new List<SkillObject> { Throwing, Leadership }, Control, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "aserai",
                mgr => { mgr.CharacterCreationContent.SelectedTitleType = "channeler"; UpdatePlayerCharacterEquipment(mgr, "player_youth_character", "act_childhood_arms"); }));
            menu.AddNarrativeMenuOption(MakeOption("aiel_teen_skirmishers", "joined the skirmishers.",
                "As a teenager, you joined the skirmishers, learning about hit-and-run tactics and guerrilla warfare. You honed your skills in mobility and quick strikes, and you developed a deep respect for the art of war.",
                "spear",
                new List<SkillObject> { Throwing, Tactics }, Endurance, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "aserai",
                mgr => { mgr.CharacterCreationContent.SelectedTitleType = "spear"; UpdatePlayerCharacterEquipment(mgr, "player_youth_character", "act_childhood_roguery"); }));
            menu.AddNarrativeMenuOption(MakeOption("aiel_teen_support", "supported the needs of the war parties.",
                "As a teenager, you supported the needs of the war parties, learning about logistics and strategy. You honed your skills in planning and coordination, and you developed a deep understanding of the operational aspects of warfare.",
                "civillian",
                new List<SkillObject> { Charm, Steward }, Social, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "aserai",
                mgr => { mgr.CharacterCreationContent.SelectedTitleType = "civillian"; UpdatePlayerCharacterEquipment(mgr, "player_youth_character", "act_childhood_apprentice"); }));

            // SHADOW -- visible when SelectedCulture == "sturgia"
            menu.AddNarrativeMenuOption(MakeOption("shadow_teen_soldiers", "trained with the local soldiers.",
                "As a teenager, you trained with the local soldiers, learning the art of combat and warfare. You honed your skills, looking forward to the day when you could get payback on those who had wronged you as a child.",
                "soldier",
                new List<SkillObject> { Riding, Polearm }, Vigor, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "sturgia",
                mgr => { mgr.CharacterCreationContent.SelectedTitleType = "soldier"; UpdatePlayerCharacterEquipment(mgr, "player_youth_character", "act_childhood_tough"); }));
            menu.AddNarrativeMenuOption(MakeOption("shadow_teen_blacksmith", "apprenticed with the local blacksmith.",
                "As a teenager, you apprenticed with the local blacksmith, learning how to forge weapons and tools. You work tirelessly with little reward, something was going to change.",
                "blacksmith",
                new List<SkillObject> { Smithing, Engineering }, Endurance, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "sturgia",
                mgr => { mgr.CharacterCreationContent.SelectedTitleType = "blacksmith"; UpdatePlayerCharacterEquipment(mgr, "player_youth_character", "act_childhood_apprentice"); }));
            menu.AddNarrativeMenuOption(MakeOption("shadow_teen_alley", "ran with the alley crews.",
                "As a teenager, you ran with the alley crews, running scams and learning the ways of the streets. You honed your sixth sense and survival skills to avoid the local guards. If only you could get an edge to stay one step ahead...",
                    "alley",
                new List<SkillObject> { Roguery, Athletics }, Cunning, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "sturgia",
                mgr => { mgr.CharacterCreationContent.SelectedTitleType = "alley"; UpdatePlayerCharacterEquipment(mgr, "player_youth_character", "act_childhood_roguery"); }));
            menu.AddNarrativeMenuOption(MakeOption("shadow_teen_power", "felt a power awakening within you.",
                "As a teenager, you caught the attention of the White Tower who took a keen interest in your potential to wield One Power. They shadowed your daily activities, leaving you no choice but to flee and consider how to get back at them later for destroying your life.",
                    "channeler",
                new List<SkillObject> { Throwing, Crossbow }, Control, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "sturgia",
                mgr => { mgr.CharacterCreationContent.SelectedTitleType = "channeler"; UpdatePlayerCharacterEquipment(mgr, "player_youth_character", "act_childhood_fox"); }));
            menu.AddNarrativeMenuOption(MakeOption("shadow_teen_caravan", "travelled with a caravan of peddlers.",
                "As a teenager, you joined a group of peddlers, learning about trades and professions, hoping to find your calling. You honed your skills in trade and organising caravans, but found it more profitable to help yourself to the goods.",
                    "civilian",
                new List<SkillObject> { Trade, Steward }, Intelligence, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "sturgia",
                mgr => { mgr.CharacterCreationContent.SelectedTitleType = "civilian"; UpdatePlayerCharacterEquipment(mgr, "player_youth_character", "act_childhood_numbers"); }));
            menu.AddNarrativeMenuOption(MakeOption("shadow_teen_study", "chose the path of study.",
                "As a teenager, your family had the means to send you to study in the city, learning about everything from logistics to military strategy. You developed a deep understanding of the operational aspects of warfare while enjoying the social aspects of student life. That's when you met someone rather interesting...",
                    "civilian",
                new List<SkillObject> { Charm, Tactics }, Social, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "sturgia",
                mgr => { mgr.CharacterCreationContent.SelectedTitleType = "civilian"; UpdatePlayerCharacterEquipment(mgr, "player_youth_character", "act_childhood_book"); }));

            // BORDERLANDS -- visible when SelectedCulture == "khuzait"
            menu.AddNarrativeMenuOption(MakeOption("borderlands_teen_soldiers", "trained with the local soldiers.",
                "As a teenager, you trained with the local soldiers, learning the art of combat and warfare. You honed your skills, looking forward to the day when you could prove yourself in battle and earn your place among the Borderlands Guard.",
                "soldier",
                new List<SkillObject> { Riding, Polearm }, Vigor, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "khuzait",
                mgr => { mgr.CharacterCreationContent.SelectedTitleType = "soldier"; UpdatePlayerCharacterEquipment(mgr, "player_youth_character", "act_childhood_polearm"); }));
            menu.AddNarrativeMenuOption(MakeOption("borderlands_teen_blacksmith", "apprenticed with the local blacksmith.",
                "As a teenager, you apprenticed with the local blacksmith, learning how to forge weapons and tools. You developed a talent for problem-solving and innovation.",
                "blacksmith",
                new List<SkillObject> { Smithing, Engineering }, Endurance, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "khuzait",
                mgr => { mgr.CharacterCreationContent.SelectedTitleType = "blacksmith"; UpdatePlayerCharacterEquipment(mgr, "player_youth_character", "act_childhood_apprentice"); }));
            menu.AddNarrativeMenuOption(MakeOption("borderlands_teen_alley", "ran with the alley crews.",
                "As a teenager, you ran with the alley crews, running scams and learning the ways of the streets. You honed your sixth sense and survival skills to avoid the local guards.",
                    "alley",
                new List<SkillObject> { Roguery, Athletics }, Cunning, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "khuzait",
                mgr => { mgr.CharacterCreationContent.SelectedTitleType = "alley"; UpdatePlayerCharacterEquipment(mgr, "player_youth_character", "act_childhood_roguery"); }));
            menu.AddNarrativeMenuOption(MakeOption("borderlands_teen_power", "felt a power awakening within you.",
                "As a teenager, you caught the attention of the White Tower who took a keen interest in your potential to wield One Power. They shadowed your daily activities, leaving you unsure if you should embrace them or flee.",
                    "channeler",
                new List<SkillObject> { Throwing, Crossbow }, Control, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "khuzait",
                mgr => { mgr.CharacterCreationContent.SelectedTitleType = "channeler"; UpdatePlayerCharacterEquipment(mgr, "player_youth_character", "act_childhood_focus"); }));
            menu.AddNarrativeMenuOption(MakeOption("borderlands_teen_caravan", "travelled with a caravan of peddlers.",
                "As a teenager, you joined a group of peddlers, learning about trades and professions, hoping to find your calling. You honed your skills in trade and organising caravans, preparing you to lead your own trade caravan.",
                    "civilian",
                new List<SkillObject> { Trade, Steward }, Intelligence, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "khuzait",
                mgr => { mgr.CharacterCreationContent.SelectedTitleType = "civilian"; UpdatePlayerCharacterEquipment(mgr, "player_youth_character", "act_childhood_appearances"); }));
            menu.AddNarrativeMenuOption(MakeOption("borderlands_teen_study", "chose the path of study.",
                "As a teenager, your family had the means to send you to study in the city, learning about everything from logistics to military strategy. You developed a deep understanding of the operational aspects of warfare while enjoying the social aspects of student life.",
                    "civilian",
                new List<SkillObject> { Charm, Tactics }, Social, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "khuzait",
                mgr => { mgr.CharacterCreationContent.SelectedTitleType = "civilian"; UpdatePlayerCharacterEquipment(mgr, "player_youth_character", "act_childhood_book"); }));

            // WESTLANDS -- visible when SelectedCulture == "empire"
            menu.AddNarrativeMenuOption(MakeOption("westlands_teen_soldiers", "trained with the local soldiers.",
                "As a teenager, you trained with the local soldiers, learning the art of combat and warfare. You honed your skills, looking forward to the day when you could prove yourself in battle and earn your place among the City Watch.",
                "soldier",
                new List<SkillObject> { Riding, OneHanded }, Vigor, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "empire",
                mgr => { mgr.CharacterCreationContent.SelectedTitleType = "soldier"; UpdatePlayerCharacterEquipment(mgr, "player_youth_character", "act_childhood_tough"); }));
            menu.AddNarrativeMenuOption(MakeOption("westlands_teen_blacksmith", "apprenticed with the local blacksmith.",
                "As a teenager, you apprenticed with the local blacksmith, learning how to forge weapons and tools. You developed a talent for problem-solving and innovation.",
                "blacksmith",
                new List<SkillObject> { Smithing, Engineering }, Endurance, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "empire",
                mgr => { mgr.CharacterCreationContent.SelectedTitleType = "blacksmith"; UpdatePlayerCharacterEquipment(mgr, "player_youth_character", "act_childhood_apprentice"); }));
            menu.AddNarrativeMenuOption(MakeOption("westlands_teen_alley", "ran with the alley crews.",
                "As a teenager, you ran with the alley crews, running scams and learning the ways of the streets. You honed your sixth sense and survival skills to avoid the local guards.",
                "alley",
                new List<SkillObject> { Roguery, Athletics }, Cunning, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "empire",
                mgr => { mgr.CharacterCreationContent.SelectedTitleType = "alley"; UpdatePlayerCharacterEquipment(mgr, "player_youth_character", "act_childhood_roguery"); }));
            menu.AddNarrativeMenuOption(MakeOption("westlands_teen_power", "felt a power awakening within you.",
                "As a teenager, you caught the attention of the White Tower who took a keen interest in your potential to wield One Power. They shadowed your daily activities, leaving you unsure if you should embrace them or flee.",
                "channeler",
                new List<SkillObject> { Throwing, Crossbow }, Control, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "empire",
                mgr => { mgr.CharacterCreationContent.SelectedTitleType = "channeler"; UpdatePlayerCharacterEquipment(mgr, "player_youth_character", "act_childhood_clever"); }));
            menu.AddNarrativeMenuOption(MakeOption("westlands_teen_caravan", "travelled with a caravan of peddlers.",
                "As a teenager, you joined a group of peddlers, learning about trades and professions, hoping to find your calling. You honed your skills in trade and organising caravans, preparing you to lead your own trade caravan.",
                "civilian",
                new List<SkillObject> { Trade, Steward }, Intelligence, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "empire",
                mgr => { mgr.CharacterCreationContent.SelectedTitleType = "civilian"; UpdatePlayerCharacterEquipment(mgr, "player_youth_character", "act_childhood_sharp"); }));
            menu.AddNarrativeMenuOption(MakeOption("westlands_teen_study", "chose the path of study.",
                "As a teenager, your family had the means to send you to study in the city, learning about everything from logistics to military strategy. You developed a deep understanding of the operational aspects of warfare while enjoying the social aspects of student life.",
                "civilian",
                new List<SkillObject> { Charm, Tactics }, Social, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "empire",
                mgr => { mgr.CharacterCreationContent.SelectedTitleType = "civilian"; UpdatePlayerCharacterEquipment(mgr, "player_youth_character", "act_childhood_tactician"); }));

            // COASTLANDS -- visible when SelectedCulture == "battania"
            menu.AddNarrativeMenuOption(MakeOption("coastlands_teen_soldiers", "trained with the local soldiers.",
                "As a teenager, you trained with the local soldiers, learning the art of combat and warfare. You honed your skills, looking forward to the day when you could prove yourself in battle and earn your place among the City Watch.",
                "soldier",
                new List<SkillObject> { Riding, OneHanded }, Vigor, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "battania",
                mgr => { mgr.CharacterCreationContent.SelectedTitleType = "soldier"; UpdatePlayerCharacterEquipment(mgr, "player_youth_character", "act_childhood_tough"); }));
            menu.AddNarrativeMenuOption(MakeOption("coastlands_teen_engineers", "apprenticed with the Engineers Guild.",
                "As a teenager, you apprenticed with the Engineers Guild, learning about construction, mechanics, and engineering principles. You developed a talent for problem-solving and innovation.",
                "engineer",
                new List<SkillObject> { Smithing, Engineering }, Intelligence, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "battania",
                mgr => { mgr.CharacterCreationContent.SelectedTitleType = "engineer"; UpdatePlayerCharacterEquipment(mgr, "player_youth_character", "act_childhood_schooled"); }));
            menu.AddNarrativeMenuOption(MakeOption("coastlands_teen_alley", "ran with the alley crews.",
                "As a teenager, you ran with the alley crews, running scams and learning the ways of the streets. You honed your sixth sense and survival skills to avoid the local guards.",
                "alley",
                new List<SkillObject> { Roguery, Athletics }, Cunning, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "battania", 
                mgr => { mgr.CharacterCreationContent.SelectedTitleType = "alley"; UpdatePlayerCharacterEquipment(mgr, "player_youth_character", "act_childhood_streets"); }));
            menu.AddNarrativeMenuOption(MakeOption("coastlands_teen_power", "felt a power awakening within you.",
                "As a teenager, you caught the attention of the White Tower who took a keen interest in your potential to wield One Power. They shadowed your daily activities, leaving you unsure if you should embrace them or flee.",
                "channeler",
                new List<SkillObject> { Throwing, Crossbow }, Control, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "battania", 
                mgr => { mgr.CharacterCreationContent.SelectedTitleType = "channeler"; UpdatePlayerCharacterEquipment(mgr, "player_youth_character", "act_childhood_clever"); }));
            menu.AddNarrativeMenuOption(MakeOption("coastlands_teen_artisan", "undertook an apprenticeship with a local artisan.",
                "As a teenager, you apprenticed to the artisan guilds, learning about trades and professions, hoping to find your calling. You honed your skills in trade and organising caravans, preparing you to lead your own trade caravan.",
                "civilian",
                new List<SkillObject> { Trade, Steward }, Endurance, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "battania", 
                mgr => { mgr.CharacterCreationContent.SelectedTitleType = "civilian"; UpdatePlayerCharacterEquipment(mgr, "player_youth_character", "act_childhood_peddlers"); }));
            menu.AddNarrativeMenuOption(MakeOption("coastlands_teen_study", "chose the path of study.",
                "As a teenager, your family had the means to send you to study in the city, learning about everything from logistics to military strategy. You developed a deep understanding of the operational aspects of warfare while enjoying the social aspects of student life.",
                "civilian",
                new List<SkillObject> { Charm, Tactics }, Social, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "battania", 
                mgr => { mgr.CharacterCreationContent.SelectedTitleType = "civilian"; UpdatePlayerCharacterEquipment(mgr, "player_youth_character", "act_childhood_tactician"); }));

            // SEANCHAN -- visible when SelectedCulture == "vlandia"
            menu.AddNarrativeMenuOption(MakeOption("seanchan_teen_soldiers", "trained with the Seanchan soldiers.",
                "As a teenager, you trained with the Seanchan soldiers, learning the art of combat and warfare. You honed your skills, looking forward to the day when you could prove yourself in battle and earn your place among the Deathwatch Guard.",
                "soldier",
                new List<SkillObject> { Riding, OneHanded }, Vigor, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "vlandia",
                mgr => { mgr.CharacterCreationContent.SelectedTitleType = "soldier"; UpdatePlayerCharacterEquipment(mgr, "player_youth_character", "act_childhood_tough"); }));
            menu.AddNarrativeMenuOption(MakeOption("seanchan_teen_militia", "stood guard with the town militia.",
                "As a teenager, you stood guard with the town militia, learning about discipline and duty. You learned skills and techniques required to defend a settlement from its attackers.",
                "bowman",
                new List<SkillObject> { Bow, Engineering }, Intelligence, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "vlandia",
                mgr => { mgr.CharacterCreationContent.SelectedTitleType = "bowman"; UpdatePlayerCharacterEquipment(mgr, "player_youth_character", "act_childhood_ready_bow"); }));
            menu.AddNarrativeMenuOption(MakeOption("seanchan_teen_alley", "ran with the alley crews.",
                "As a teenager, you ran with the alley crews, running scams and learning the ways of the streets. You honed your sixth sense and survival skills to avoid the local guards.",
                "alley",
                new List<SkillObject> { Roguery, Athletics }, Cunning, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "vlandia",
                mgr => { mgr.CharacterCreationContent.SelectedTitleType = "alley"; UpdatePlayerCharacterEquipment(mgr, "player_youth_character", "act_childhood_streets"); }));
            menu.AddNarrativeMenuOption(MakeOption("seanchan_teen_damane", "were destined for damane testing.",
                "As a teenager, you caught the attention of the sul'dam who took a keen interest in your potential to become their next damane. They guided your daily tasks, having you perform odd exercises clearly meant to prepare you for something.",
                "channeler",
                new List<SkillObject> { Throwing, Crossbow }, Control, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "vlandia",
                mgr => { mgr.CharacterCreationContent.SelectedTitleType = "channeler"; UpdatePlayerCharacterEquipment(mgr, "player_youth_character", "act_childhood_ready_throw"); }));
            menu.AddNarrativeMenuOption(MakeOption("seanchan_teen_artisan", "undertook an apprenticeship with a local artisan.",
                "As a teenager, you apprenticed to the artisan guilds, learning about trades and professions, hoping to find your calling. You honed your skills in trade and smithing, preparing you for a mundane future.",
                "civilian",
                new List<SkillObject> { Trade, Smithing }, Endurance, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "vlandia",
                mgr => { mgr.CharacterCreationContent.SelectedTitleType = "civilian"; UpdatePlayerCharacterEquipment(mgr, "player_youth_character", "act_childhood_apprentice"); }));
            menu.AddNarrativeMenuOption(MakeOption("seanchan_teen_study", "chose the path of study.",
                "As a teenager, your family had the means to send you to study in the great Seanchan cities, learning about everything from logistics to military strategy. You developed a deep understanding of the operational aspects of warfare while enjoying the social aspects of student life.",
                "civilian",
                new List<SkillObject> { Charm, Tactics }, Social, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "vlandia",
                mgr => { mgr.CharacterCreationContent.SelectedTitleType = "civilian"; UpdatePlayerCharacterEquipment(mgr, "player_youth_character", "act_childhood_book"); }));

            manager.AddNewMenu(menu);
        }

        // =====================================================================
        // AddFocusMenu -- all 6 cultures, culture-filtered via onCondition
        // =====================================================================
        public void AddFocusMenu(CharacterCreationManager manager, int focusToAdd, int skillLevelToAdd, int attributeLevelToAdd)
        {
            BodyProperties playerProps = CharacterObject.PlayerCharacter.GetBodyProperties(
            CharacterObject.PlayerCharacter.Equipment, -1);

            var characters = new List<NarrativeMenuCharacter>
            {
                new NarrativeMenuCharacter("player_adulthood_character", playerProps,
                    CharacterObject.PlayerCharacter.Race,
                    CharacterObject.PlayerCharacter.IsFemale)
            };

            var menu = MakeMenu(
                "narrative_focus_menu", "narrative_teenager_menu", "narrative_age_selection_menu",
                "Your Greatest Achievement",
                "Before you set out for adventure, your biggest achievement was...",
                new NarrativeMenu.GetNarrativeMenuCharacterArgsDelegate(this.GetFocusCharacterArgs), characters);

            // AIEL -- visible when SelectedCulture == "aserai"
            menu.AddNarrativeMenuOption(MakeOption("aiel_focus_battle", "you defeated an enemy in battle.",
                "You proved your worth and value as a warrior, earning the respect of your clan by defeating an enemy clan's spear in a one-on-one trial by combat.", "", 
                new List<SkillObject> { OneHanded, Polearm }, Vigor, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "aserai", null,
                new List<TraitObject> { Valor }, 1, renown: 20));
            menu.AddNarrativeMenuOption(MakeOption("aiel_focus_raid", "you led a successful raid.",
                "You demonstrated your leadership and strategic skills by leading a successful raid against Sharan Raiders.","",
                new List<SkillObject> { Leadership, Tactics }, Cunning, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "aserai", null,
                new List<TraitObject> { Valor }, 1, renown: 10));
            menu.AddNarrativeMenuOption(MakeOption("aiel_focus_trade", "you made an excellent profit trading goods at market.",
                "You made a wise investment in marketplace goods, which has provided you with a reputation for shrewdness and financial acumen.", "", 
                new List<SkillObject> { Trade, Steward }, Intelligence, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "aserai", null,
                new List<TraitObject> { Calculating }, 1, renown: 15));
            menu.AddNarrativeMenuOption(MakeOption("aiel_focus_hunt", "you hunted a dangerous animal.",
                "You proved your skill and bravery by hunting and killing a dangerous animal that was threatening your village.","",
                new List<SkillObject> { Athletics, Bow }, Control, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "aserai", null,
                new List<TraitObject> { Valor }, 1, renown: 5));
            menu.AddNarrativeMenuOption(MakeOption("aiel_focus_escapade", "you had a famous escapade in town.",
                "You had a memorable adventure in town that became the talk of the village, earning you a reputation for daring and charisma.", "", 
                new List<SkillObject> { Athletics, Roguery }, Endurance, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "aserai", null,
                new List<TraitObject> { Valor }, 1, renown: 5));
            menu.AddNarrativeMenuOption(MakeOption("aiel_focus_kindness", "you treated people well.",
                "You were known for your kindness and generosity, always willing to help those in need. Your good deeds earned you the respect and admiration of your community.", "", 
                new List<SkillObject> { Charm, Steward }, Social, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "aserai", null,
                new List<TraitObject> { Mercy, Generosity, Honor }, 1, renown: 5));

            // SHADOW -- visible when SelectedCulture == "sturgia"
            menu.AddNarrativeMenuOption(MakeOption("shadow_focus_battle", "you defeated an enemy in battle.",
                "You proved your worth and value as a fighter, defeating your childhood bully. Unfortunately, the victory came at a cost given they were the local lord's only son, leaving you no choice but to find a new home.","",
                new List<SkillObject> { OneHanded, Polearm }, Vigor, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "sturgia", null,
                new List<TraitObject> { Valor }, 1, renown: 20));
            menu.AddNarrativeMenuOption(MakeOption("shadow_focus_raid", "you led a successful raid.",
                "You demonstrated your leadership and strategic skills by leading a successful hunt for a party of Bandits. This caught the eye of a local Darkfriend who offered you a place in their service, giving you the resources to encourage your skills.","",
                new List<SkillObject> { Leadership, Tactics }, Cunning, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "sturgia", null,
                new List<TraitObject> { Valor }, 1, renown: 10));
            menu.AddNarrativeMenuOption(MakeOption("shadow_focus_trade", "you made an excellent profit trading goods at market.",
                "You made many profitable trades working the markets of cities and villages in the area earning you a reputation for shrewdness. We just won't talk about what happened to the merchants who didnt offer you generous prices...", "",
                new List<SkillObject> { Trade, Steward }, Intelligence, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "sturgia", null,
                new List<TraitObject> { Calculating }, 1, renown: 15));
            menu.AddNarrativeMenuOption(MakeOption("shadow_focus_hunt", "you hunted a dangerous animal.",
                "You proved your skill and bravery by hunting and killing a dangerous animal that was threatening your village. Such skills are much needed by the Shadow and their agents are very persuasive...","",
                new List<SkillObject> { Athletics, Bow }, Control, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "sturgia", null,
                new List<TraitObject> { Valor }, 1, renown: 5));
            menu.AddNarrativeMenuOption(MakeOption("shadow_focus_escapade", "you had a famous escapade in town.",
                "You had a memorable adventure in town that became the talk of the village, earning you a reputation for daring and charisma. The blackmail that followed however, required you to make some arrangements shall we say.", "",
                new List<SkillObject> { Athletics, Roguery }, Endurance, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "sturgia", null,
                new List<TraitObject> { Valor }, 1, renown: 5));
            menu.AddNarrativeMenuOption(MakeOption("shadow_focus_kindness", "you treated people well.",
                "You were known for your kindness and generosity, always willing to help those in need. Your good deeds however were taken advantage of by your community, and you have found a way to make sure that doesn't happen again.", "",
                new List<SkillObject> { Charm, Steward }, Social, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "sturgia", null,
                new List<TraitObject> { Mercy, Generosity, Honor }, 1, renown: 5));

            // BORDERLANDS -- visible when SelectedCulture == "khuzait"
            menu.AddNarrativeMenuOption(MakeOption("borderlands_focus_battle", "you defeated an enemy in battle.",
                "You proved your worth and value as a soldier, earning the respect of your commanders by defeating a Trolloc in one-on-one combat during a raid.", "",
                new List<SkillObject> { OneHanded, Polearm }, Vigor, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "khuzait", null,
                new List<TraitObject> { Valor }, 1, renown: 20));
            menu.AddNarrativeMenuOption(MakeOption("borderlands_focus_raid", "you led a successful raid.",
                "You demonstrated your leadership and strategic skills by leading a successful hunt for a party of Darkfriends.", "",
                new List<SkillObject> { Leadership, Tactics }, Cunning, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "khuzait", null,
                new List<TraitObject> { Valor }, 1, renown: 10));
            menu.AddNarrativeMenuOption(MakeOption("borderlands_focus_trade", "you made an excellent profit trading goods at market.",
                "You made many a wise investment in marketplace goods, which has provided you with a reputation as a merchant in the making.", "",
                new List<SkillObject> { Trade, Steward }, Intelligence, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "khuzait", null,
                new List<TraitObject> { Calculating }, 1, renown: 15));
            menu.AddNarrativeMenuOption(MakeOption("borderlands_focus_hunt", "you hunted a dangerous animal.",
                "You proved your skill and bravery by hunting and killing a dangerous animal that was threatening your village.", "",
                new List<SkillObject> { Athletics, Bow }, Control, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "khuzait", null,
                new List<TraitObject> { Valor }, 1, renown: 5));
            menu.AddNarrativeMenuOption(MakeOption("borderlands_focus_escapade", "you had a famous escapade in town.",
                "You had a memorable adventure in town that became the talk of the village, earning you a reputation for daring and charisma.", "",
                new List<SkillObject> { Athletics, Roguery }, Endurance, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "khuzait", null,
                new List<TraitObject> { Valor }, 1, renown: 5));
            menu.AddNarrativeMenuOption(MakeOption("borderlands_focus_kindness", "you treated people well.",
                "You were known for your kindness and generosity, always willing to help those in need. Your good deeds earned you the respect and admiration of your community.", "",
                new List<SkillObject> { Charm, Steward }, Social, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "khuzait", null,
                new List<TraitObject> { Mercy, Generosity, Honor }, 1, renown: 5));

            // WESTLANDS -- visible when SelectedCulture == "empire"
            menu.AddNarrativeMenuOption(MakeOption("westlands_focus_battle", "you defeated an enemy in battle.",
                "You proved your worth and value as a soldier, earning the respect of your commanders by defeating an enemy Captain in a one-on-one trial by combat.", "",
                new List<SkillObject> { OneHanded, Polearm }, Vigor, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "empire", null,
                new List<TraitObject> { Valor }, 1, renown: 20));
            menu.AddNarrativeMenuOption(MakeOption("westlands_focus_raid", "you led a successful raid.",
                "You demonstrated your leadership and strategic skills by leading a successful hunt for a party of bandits.", "",
                new List<SkillObject> { Leadership, Tactics }, Cunning, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "empire", null,
                new List<TraitObject> { Valor }, 1, renown: 10));
            menu.AddNarrativeMenuOption(MakeOption("westlands_focus_trade", "you made an excellent profit trading goods at market.",
                "You made a wise investment in marketplace goods, which has provided you with a reputation for shrewdness and financial acumen.", "",
                new List<SkillObject> { Trade, Steward }, Intelligence, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "empire", null,
                new List<TraitObject> { Calculating }, 1, renown: 15));
            menu.AddNarrativeMenuOption(MakeOption("westlands_focus_hunt", "you hunted a dangerous animal.",
                "You proved your skill and bravery by hunting and killing a dangerous animal that was threatening your village.", "",
                new List<SkillObject> { Athletics, Bow }, Control, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "empire", null,
                new List<TraitObject> { Valor }, 1, renown: 5));
            menu.AddNarrativeMenuOption(MakeOption("westlands_focus_escapade", "you had a famous escapade in town.",
                "You had a memorable adventure in town that became the talk of the village, earning you a reputation for daring and charisma.", "",
                new List<SkillObject> { Athletics, Roguery }, Endurance, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "empire", null,
                new List<TraitObject> { Valor }, 1, renown: 5));
            menu.AddNarrativeMenuOption(MakeOption("westlands_focus_kindness", "you treated people well.",
                "You were known for your kindness and generosity, always willing to help those in need. Your good deeds earned you the respect and admiration of your community.", "",
                new List<SkillObject> { Charm, Steward }, Social, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "empire", null,
                new List<TraitObject> { Mercy, Generosity, Honor }, 1, renown: 5));

            // COASTLANDS -- visible when SelectedCulture == "battania"
            menu.AddNarrativeMenuOption(MakeOption("coastlands_focus_battle", "you defeated an enemy in battle.",
                "You proved your worth and value as a soldier, earning the respect of your commanders by defeating an enemy Captain in a one-on-one trial by combat.", "",
                new List<SkillObject> { OneHanded, Polearm }, Vigor, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "battania", null,
                new List<TraitObject> { Valor }, 1, renown: 20));
            menu.AddNarrativeMenuOption(MakeOption("coastlands_focus_raid", "you led a successful raid.",
                "You demonstrated your leadership and strategic skills by leading a successful hunt for a party of bandits.", "",
                new List<SkillObject> { Leadership, Tactics }, Cunning, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "battania", null,
                new List<TraitObject> { Valor }, 1, renown: 10));
            menu.AddNarrativeMenuOption(MakeOption("coastlands_focus_trade", "you made an excellent profit trading goods at market.",
                "You made many a wise investment in marketplace goods, which has provided you with a reputation as a trader in the making.", "",
                new List<SkillObject> { Trade, Steward }, Intelligence, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "battania", null,
                new List<TraitObject> { Calculating }, 1, renown: 15));
            menu.AddNarrativeMenuOption(MakeOption("coastlands_focus_hunt", "you hunted a dangerous animal.",
                "You proved your skill and bravery by hunting and killing a dangerous animal that was threatening your village.", "",
                new List<SkillObject> { Athletics, Bow }, Control, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "battania", null,
                new List<TraitObject> { Valor }, 1, renown: 5));
            menu.AddNarrativeMenuOption(MakeOption("coastlands_focus_escapade", "you had a famous escapade in town.",
                "You had a memorable adventure in town that became the talk of the village, earning you a reputation for daring and charisma.", "",
                new List<SkillObject> { Athletics, Roguery }, Endurance, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "battania", null,
                new List<TraitObject> { Valor }, 1, renown: 5));
            menu.AddNarrativeMenuOption(MakeOption("coastlands_focus_kindness", "you treated people well.",
                "You were known for your kindness and generosity, always willing to help those in need. Your good deeds earned you the respect and admiration of your community.", "",
                new List<SkillObject> { Charm, Steward }, Social, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "battania", null,
                new List<TraitObject> { Mercy, Generosity, Honor }, 1, renown: 5));

            // SEANCHAN -- visible when SelectedCulture == "vlandia"
            menu.AddNarrativeMenuOption(MakeOption("seanchan_focus_battle", "you defeated an enemy in battle.",
                "You proved your worth and value as a soldier, earning the respect of your commanders by defeating an enemy Captain in a one-on-one trial by combat.", "",
                new List<SkillObject> { OneHanded, Polearm }, Vigor, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "vlandia", null,
                new List<TraitObject> { Valor }, 1, renown: 20));
            menu.AddNarrativeMenuOption(MakeOption("seanchan_focus_raid", "you led a successful raid.",
                "You demonstrated your leadership and strategic skills by leading a successful hunt for a male channeller.", "",
                new List<SkillObject> { Leadership, Tactics }, Cunning, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "vlandia", null,
                new List<TraitObject> { Valor }, 1, renown: 10));
            menu.AddNarrativeMenuOption(MakeOption("seanchan_focus_trade", "you made an excellent profit trading goods at market.",
                "You made many a wise investment in marketplace goods, which has provided you with a reputation as a merchant in the making.", "",
                new List<SkillObject> { Trade, Steward }, Intelligence, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "vlandia", null,
                new List<TraitObject> { Calculating }, 1, renown: 15));
            menu.AddNarrativeMenuOption(MakeOption("seanchan_focus_hunt", "you hunted a dangerous animal.",
                "You proved your skill and bravery by hunting and killing a dangerous animal that was threatening your village.", "",
                new List<SkillObject> { Athletics, Bow }, Control, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "vlandia", null,
                new List<TraitObject> { Valor }, 1, renown: 5));
            menu.AddNarrativeMenuOption(MakeOption("seanchan_focus_escapade", "you had a famous escapade in town.",
                "You had a memorable adventure in town that became the talk of the village, earning you a reputation for daring and charisma.", "",
                new List<SkillObject> { Athletics, Roguery }, Endurance, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "vlandia", null,
                new List<TraitObject> { Valor }, 1, renown: 5));
            menu.AddNarrativeMenuOption(MakeOption("seanchan_focus_kindness", "you treated people well.",
                "You were known for your kindness and generosity, always willing to help those in need. Your good deeds earned you the respect and admiration of your community.", "",
                new List<SkillObject> { Charm, Steward }, Social, focusToAdd, skillLevelToAdd, attributeLevelToAdd,
                mgr => mgr.CharacterCreationContent.SelectedCulture?.StringId == "vlandia", null,
                new List<TraitObject> { Mercy, Generosity, Honor }, 1, renown: 5));

            manager.AddNewMenu(menu);
        }

    }
}
