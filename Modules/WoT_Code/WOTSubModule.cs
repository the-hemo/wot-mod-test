using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using HarmonyLib;
using MCM;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using WoT_Code.Behaviours;
using WoT_Code.MagicModel;
using WoT_Code.Support;

namespace WoT_Code
{
    public class WOTSubModule : MBSubModuleBase
    {
        private Harmony _harmony;

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            //Removing Start screen options which are not needed for the mod
            TextObject coreContentDisabledReason = new TextObject("Disabled during installation.", null);
            //startScreenSupport.removeInitialStateOption("CustomBattle");
            startScreenSupport.removeInitialStateOption("StoryModeNewGame");

            //TaleWorlds.MountAndBlade.Module.CurrentModule.AddInitialStateOption(new InitialStateOption("SandBoxNewGame", new TextObject("New WoT Campaign", null), 3, delegate ()
            //{
            //    MBGameManager.StartNewGame(new WoTCampaignManager());
            //}, () => new ValueTuple<bool, TextObject>(TaleWorlds.MountAndBlade.Module.CurrentModule.IsOnlyCoreContentEnabled, coreContentDisabledReason)))

            _harmony = new Harmony("WoT_Code.HarmonyPatches");
            Harmony.DEBUG = false;
            _harmony.PatchAll();

            
        }

        protected override void OnBeforeInitialModuleScreenSetAsRoot()
        {
            base.OnBeforeInitialModuleScreenSetAsRoot();
            // Displays when the mod is loading before the start screen.
            InformationManager.DisplayMessage(new InformationMessage("All was shattered, and all but memory lost, and one memory above all others, of him who brought the Shadow and the Breaking of the World. And him they named Dragon.", new Color(134f, 114f, 250f, 1f)));
        }
        /*Wheel of Time 1.4.7 Test Version
        /*  This was for some custom battle scenes i beleive, but i have not used it yet, so i commented it out for now.
        public override void OnMissionBehaviorInitialize(Mission mission)
        {
            base.OnMissionBehaviorInitialize(mission);
            // Add mission behaviours you need (minimal at first)
            mission.AddMissionBehavior(new DeathBarrier2());
        }
        */

        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            if (game.GameType is Campaign)
            {
                new WoTTraits();

                CampaignGameStarter campaignGameStarter = (CampaignGameStarter)gameStarterObject;
                 // Add models/behaviours incrementally
                campaignGameStarter.AddModel(new WOTTroopTier());
                campaignGameStarter.AddBehavior(new OnePowerRecruitBehavior());
                campaignGameStarter.AddModel(new WoTAgentApplyDamageModel());
                campaignGameStarter.AddBehavior(new QuestManager());
                campaignGameStarter.AddModel(new WoTPartyHealingModel());
                campaignGameStarter.AddModel(new WoTPartyImpairmentModel());
                campaignGameStarter.AddBehavior(new KingdomlessRecruitBehavior());
                campaignGameStarter.AddBehavior(new KingdomlessClanSurvivalBehavior());
                campaignGameStarter.AddModel(new WoTTournamentModel());
                campaignGameStarter.AddModel(new WoTAielScoutSpeedModel());
                campaignGameStarter.AddBehavior(new WoTPlayerMarriageBehavior());
                campaignGameStarter.AddBehavior(new WoTLoreIncidents());
                campaignGameStarter.AddBehavior(new WoTLordConversations());
                //campaignGameStarter.AddBehavior(new WoTCharacterCreationBehavior()); - this method didnt work had to use harmony
                // Message to appears when player first loads into game
                InformationManager.DisplayMessage(new InformationMessage("The Wheel of Time turns, and Ages come and pass leaving memories that become legend, then fade to myth, and are long forgot when that Age comes again. - Robert Jordan (The Great Hunt)", new Color(0f, 1f, 0f, 1f)));
            }
        }
        // In SubModule.cs
        
        public override void OnMissionBehaviorInitialize(Mission mission)
        {
            base.OnMissionBehaviorInitialize(mission);
            if (mission.CombatType == Mission.MissionCombatType.Combat)
            {
                mission.AddMissionBehavior(new HornOfValereBehaviour());
                mission.AddMissionBehavior(new ShotgunEquipmentMissionLogic());
            }
        }
        
    }
}
