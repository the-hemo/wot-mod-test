using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

/* Note: the behaviour doesnt quite worrk as intended as we wanted to set the relations between the lords and the player to be 100, 
 * but it seems that the relations are not being set correctly. We will need to investigate further to see why this is happening.
 * At least though it does set the Friend/Enemy status correctly. 
 The _pendingRelationSetup flag is used to ensure that the relations are only set on a new game and dont overwrite existing games on load. */

namespace WoT_Code.Behaviours
{
    public class WoTLordRelations : CampaignBehaviorBase
    {
        private bool _pendingRelationSetup = false;

        public override void RegisterEvents()
        {
            CampaignEvents.OnNewGameCreatedEvent.AddNonSerializedListener(this, OnNewGameCreated);
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, LogRelationsAfterSession);
        }

        public override void SyncData(IDataStore dataStore) { }

        private void OnNewGameCreated(CampaignGameStarter starter)
        {
            _pendingRelationSetup = true;
        }

        private void LogRelationsAfterSession(CampaignGameStarter starter)

        {
            if (!_pendingRelationSetup) return;
            {

                List<string> heroIds = new List<string>
                {
                    "lord_1_25",    // Egwene
                    "lord_1_17",    // Nynaeve
                    "lord_1_422",   // Blacktower loyalist leader
                    "lord_2_11",    // Rhuarc
                    "lord_4_23",    // Bashere
                    "lord_1_1",     // Elayne
                    "lord_3_18_3",  // Darlin
                    "lord_3_19_3"   // Belearne (Mayene ruler)
                };

                List<string> darkfriendsIds = new List<string>
                {
                    "lord_1_27_3"   // Marzim tiam
                };

                string GaladID = "lord_SE9_l";
                string ElayneID = "lord_1_1";
                string RandId = "lord_1_48_1";
                string IshmaelID = "lord_5_20";

                Hero Rand = Hero.FindFirst(x => x.StringId == RandId);
                Hero Ishmael = Hero.FindFirst(x => x.StringId == IshmaelID);
                Hero Galad = Hero.FindFirst(x => x.StringId == GaladID);
                Hero Elayne = Hero.FindFirst(x => x.StringId == ElayneID);

                if (Rand != null)
                {
                    foreach (string id in heroIds)
                    {
                        Hero hero = Hero.FindFirst(x => x.StringId == id);
                        if (hero != null)
                        {
                            CharacterRelationManager.SetHeroRelation(Rand, hero, 100);
                        }
                    }
                }

                if (Ishmael != null)
                {
                    foreach (string id in darkfriendsIds)
                    {
                        Hero hero = Hero.FindFirst(x => x.StringId == id);
                        if (hero != null)
                        {
                            CharacterRelationManager.SetHeroRelation(Ishmael, hero, 100);
                        }
                    }
                }

                if (Galad != null && Elayne != null)
                {
                    CharacterRelationManager.SetHeroRelation(Elayne, Galad, 100);
                }
                _pendingRelationSetup = false;
            }
        }
    }
}

