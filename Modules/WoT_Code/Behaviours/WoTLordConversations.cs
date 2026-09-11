using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.BarterSystem;
using TaleWorlds.CampaignSystem.BarterSystem.Barterables;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace WoT_Code
{
    public class WoTLordConversations : CampaignBehaviorBase
    {

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(
                this, OnSessionLaunched);
        }

        public override void SyncData(IDataStore dataStore) { }

        private void OnSessionLaunched(CampaignGameStarter starter)
        {
            AddWoTLordConversationsDialogs(starter);
            AddWoTOathDialogs(starter);
            AddWoTObligationsDialogs(starter);
            AddWoTExtra1Dialogs(starter);
            AddWoTRebelDialogs(starter);
        }

        private void AddWoTLordConversationsDialogs(CampaignGameStarter starter)
        {
            // ── Player offer line ─────────────────────────────────────────────
            starter.AddPlayerLine(
                "wot_want_to_end_mercenary_service",
                "lord_talk_speak_diplomacy_2",
                "lord_ask_exit_service",
                "{=wot_end_mercenary}I would like to end my contract with the {SERVED_FACTION}.",
                new ConversationSentence.OnConditionDelegate(this.conversation_player_want_to_end_service_as_mercenary_on_condition),
                null,
                110); // above vanilla's 100
        }

        // =====================================================================
        // swapping out Culture Formal Name for Kingdom Adjective
        // =====================================================================

        public bool conversation_player_want_to_end_service_as_mercenary_on_condition()
        {
            if (Hero.OneToOneConversationHero.MapFaction == null || Hero.OneToOneConversationHero.Clan == null || MobileParty.MainParty.Army != null)
            {
                return false;
            }
            if (Hero.OneToOneConversationHero.MapFaction.IsKingdomFaction)
            {
                MBTextManager.SetTextVariable("SERVED_FACTION", FactionHelper.GetAdjectiveForFactionCulture(Hero.OneToOneConversationHero.Clan.Kingdom.Culture), false);
            }
            return !Hero.OneToOneConversationHero.IsPrisoner && Hero.MainHero.MapFaction == Hero.OneToOneConversationHero.MapFaction && Hero.OneToOneConversationHero.Clan != Hero.MainHero.Clan && Hero.MainHero.Clan.IsUnderMercenaryService;
        }

        // =====================================================================
        // Replacement for vanilla's vassal lines which have hard coded culture
        // references
        // =====================================================================

        private void AddWoTOathDialogs(CampaignGameStarter starter)
        {
            starter.AddDialogLine(
                "wot_lord_give_oath_2",
                "lord_give_oath_2",
                "lord_give_oath_3",
                "{=54PbMkNw}Good. Then repeat the words of the oath with me: {OATH_LINE_1}",
                conversation_set_wot_oath_phrases_on_condition,
                null,
                101); // above vanilla's 100
        }

        public TextObject GetLiegeTitle()
        {
            Hero leader = Hero.OneToOneConversationHero.MapFaction.Leader;
            if (!leader.IsFemale)
            {
                return Campaign.Current.ConversationManager.FindMatchingTextOrNull("str_liege_title", leader.CharacterObject);
            }
            return Campaign.Current.ConversationManager.FindMatchingTextOrNull("str_liege_title_female", leader.CharacterObject);
        }

        public bool conversation_set_wot_oath_phrases_on_condition()
        {
            string stringId = Hero.OneToOneConversationHero.Culture.StringId;
            MBTextManager.SetTextVariable("FACTION_TITLE", this.GetLiegeTitle(), false);
            StringHelpers.SetCharacterProperties("LORD", CharacterObject.OneToOneConversationCharacter, null, false);

            if (stringId == "aserai") // Aiel
            {
                MBTextManager.SetTextVariable("OATH_LINE_1", "{=MqIg6Mh2}I swear homage to you as lawful {FACTION_TITLE}.", false);    
                MBTextManager.SetTextVariable("OATH_LINE_2", "{=kc3tLqGy}You command the clans of the Aiel in war and govern them in peace...", false);
                MBTextManager.SetTextVariable("OATH_LINE_3", "{=bue9AShm}I swear to fight your enemies and give shade and water to your friends...", false);
                MBTextManager.SetTextVariable("OATH_LINE_4", "{=qObicX7y}I swear to heed your judgements according to the laws of the Aiel, and ensure that my clan heed them as well...", false);
            }
            else if (stringId == "sturgia") // Shadow
            {
                MBTextManager.SetTextVariable("OATH_LINE_1", "{=MqIg6Mh2}I swear homage to you as lawful {FACTION_TITLE}.", false);
                MBTextManager.SetTextVariable("OATH_LINE_2", "{=Qs7qs3b0}You are the chosen voice of the Shadow.", false);
                MBTextManager.SetTextVariable("OATH_LINE_3", "{=U3u2D6Ze}I give you my word and soul, to stand by your banner in battle so long as my breath remains...", false);
                MBTextManager.SetTextVariable("OATH_LINE_4", "{=HpWYfcgw}..and to uphold your commands, even if it means giving my life, and to avenge all Darkfriends blood as thought it were my own.", false);
            }
            else if (stringId == "khuzait") // Borderlands
            {
                MBTextManager.SetTextVariable("OATH_LINE_1", "{=PP8VeNiC}I swear that you are my {?LORD.GENDER}Queen{?}King{\\?}, my {?LORD.GENDER}mother{?}father{\\?}, my protector...", false);
                MBTextManager.SetTextVariable("OATH_LINE_2", "{=QSPMKz2R}You are chosen to defend that which comes from the Blight, and I shall follow your banner as long as my breath remains...", false);
                MBTextManager.SetTextVariable("OATH_LINE_3", "{=8lOCOcXw}Your word shall direct the strike of my blade, the thrust of my lance and the flight of my arrow...", false);
                MBTextManager.SetTextVariable("OATH_LINE_4", "{=xDzxaYed}Your word shall divide the spoils of victory and the bounties of peace.", false);
            }
            else if (stringId == "battania") // Coastlands
            {
                MBTextManager.SetTextVariable("OATH_LINE_1", "{=ya8VF98X}I swear by my ancestors that you are lawful {FACTION_TITLE}.", false);
                MBTextManager.SetTextVariable("OATH_LINE_2", "{=vuEyisBW}I affirm that you hold the will of the Lords, Ladies and people...", false);
                MBTextManager.SetTextVariable("OATH_LINE_3", "{=UwbhGhGw}I shall stand by your side and not foresake you, and fight until my life leaves my body...", false);
                MBTextManager.SetTextVariable("OATH_LINE_4", "{=6KbDn1HS}I shall heed your judgements and pay you the tribute that is your due, so that this land may have a strong protector.", false);
            }
            else if (stringId == "vlandia") // Seanchan
            {
                MBTextManager.SetTextVariable("OATH_LINE_1", "{=ya8VF98X}I swear by my ancestors that you are lawful {?LORD.GENDER}Empress{?}Emperor{\\?}.", false);
                MBTextManager.SetTextVariable("OATH_LINE_2", "{=PypPEj5Z}I will be your loyal soldier as long as my breath remains...", false);
                MBTextManager.SetTextVariable("OATH_LINE_3", "{=2o7U1bNV}..and I will be at your feet to fight your enemies should you need my weapon.", false);
                MBTextManager.SetTextVariable("OATH_LINE_4", "{=waoSd6tj}.. and I shall defend you and all your legitimate heirs, and others of the Blood.", false);
            }
            else // empire, Westlands — also fallback default
            {
                MBTextManager.SetTextVariable("OATH_LINE_1", "{=ya8VF98X}I swear by my ancestors that you are lawful {FACTION_TITLE}.", false);
                MBTextManager.SetTextVariable("OATH_LINE_2", "{=OHJYAaW5}The powers of the Source and of the Pattern, have entrusted to you the guardianship of this sacred land...", false);
                MBTextManager.SetTextVariable("OATH_LINE_3", "{=LWFDXeQc}Furthermore, I accept that i am here to serve the will of our people, giving my life to defend theirs.", false);
                MBTextManager.SetTextVariable("OATH_LINE_4", "{=EsF8sEaQ}And as such, that you are my commander, and I shall follow you wherever you lead.", false);
            }

            return true;
        }

        // =====================================================================
        // Replacement for vanilla's accepted vassal lines which have hard coded 
        // culture references
        // =====================================================================

        private bool _receivedVassalRewards;
        private void ReceiveVassalRewards()
        {
            VassalRewardsModel vassalRewardsModel = Campaign.Current.Models.VassalRewardsModel;
            InventoryScreenHelper.OpenScreenAsReceiveItems(vassalRewardsModel.GetEquipmentRewardsForJoiningKingdom(Hero.OneToOneConversationHero.Clan.Kingdom), new TextObject("{=exbSCGzi}Reward Items", null), null);
            PartyScreenHelper.OpenScreenAsReceiveTroops(vassalRewardsModel.GetTroopRewardsForJoiningKingdom(Hero.OneToOneConversationHero.Clan.Kingdom), new TextObject("{=tKW8m6bZ}Reward Troops", null), null);
            ChangeRelationAction.ApplyPlayerRelation(Hero.OneToOneConversationHero.Clan.Kingdom.Leader, vassalRewardsModel.RelationRewardWithLeader, true, true);
            this._receivedVassalRewards = true;
        }

        public void conversation_player_is_accepted_as_a_vassal_on_consequence()
        {
            if (Hero.MainHero.Clan.Kingdom == Hero.OneToOneConversationHero.Clan.Kingdom)
            {
                EndMercenaryServiceAction.EndByBecomingVassal(Hero.MainHero.Clan);
            }
            else
            {
                if (Clan.PlayerClan.IsUnderMercenaryService)
                {
                    EndMercenaryServiceAction.EndByLeavingKingdom(Hero.MainHero.Clan);
                }
                ChangeKingdomAction.ApplyByJoinToKingdom(Hero.MainHero.Clan, Hero.OneToOneConversationHero.Clan.Kingdom, default(CampaignTime), true);
            }
            if (!this._receivedVassalRewards)
            {
                this.ReceiveVassalRewards();
            }
            GainKingdomInfluenceAction.ApplyForJoiningFaction(Hero.MainHero, Campaign.Current.Models.VassalRewardsModel.InfluenceReward);
            if (PlayerEncounter.Current != null)
            {
                PlayerEncounter.LeaveEncounter = true;
            }
        }

        private void AddWoTObligationsDialogs(CampaignGameStarter starter)
        {
            starter.AddDialogLine(
                "wot_player_is_accepted_as_a_vassal",
                "lord_give_oath_go_on_2",
                "lord_give_oath_go_on_3",
                "{=XqWloWK0}{PLAYER_ACCEPTED_AS_VASSAL}", new ConversationSentence.OnConditionDelegate(this.conversation_set_wot_obligations_phrases_on_condition), 
                new ConversationSentence.OnConsequenceDelegate(this.conversation_player_is_accepted_as_a_vassal_on_consequence), 101, null);
        }

        public bool conversation_set_wot_obligations_phrases_on_condition()
        {
            string stringId = Hero.OneToOneConversationHero.Culture.StringId;
            MBTextManager.SetTextVariable("FACTION_TITLE", this.GetLiegeTitle(), false);
            StringHelpers.SetCharacterProperties("LORD", CharacterObject.OneToOneConversationCharacter, null, false);

            if (stringId == "aserai") // Aiel
            {
                MBTextManager.SetTextVariable("PLAYER_ACCEPTED_AS_VASSAL", "{=3v3ZTccn}You shall be numbered among the clans of the Aiel. Your blood is our blood. Our spears shall defend you as you defend us. You may drink from our wells and rest in the shade of our trees. You may be granted the authority to judge disputes and collect revenues from oases and towns.", false);
            }
            else if (stringId == "sturgia") // Shadow
            {
                MBTextManager.SetTextVariable("PLAYER_ACCEPTED_AS_VASSAL", "{=fInFLbAV}I accept you as my sworn follower. You shall have the shadow's due: the warmth of his hearth, food for your belly and rewards for your valor. I shall uphold your position and avenge your blood if you fall.", false);
            }
            else if (stringId == "khuzait") // Borderlands
            {
                MBTextManager.SetTextVariable("PLAYER_ACCEPTED_AS_VASSAL", "{=iWBrManr}Let it be known that you are adopted into the {FACTION_TITLE}. You may sit in our councils of war and of peace. We shall ride to defend your borders and avenge your blood if you fall. Your herds may graze in our lands and drink from our springs.", false);
            }
            else if (stringId == "battania") // Coastlands
            {
                MBTextManager.SetTextVariable("PLAYER_ACCEPTED_AS_VASSAL", "{=dhi3ggBC}Let it be known that you are one of the {FACTION_TITLE}. You may till our soil and sit in our councils. Who quarrels with you, quarrels with all of us.", false);
            }
            else if (stringId == "vlandia") // Seanchan
            {
                MBTextManager.SetTextVariable("PLAYER_ACCEPTED_AS_VASSAL", "{=6oevXUSa}Let it be known that from this day forward, you are my sworn subject and vassal. I give you my protection and grant you the right to bear arms and draw breath in my name.", false);
            }
            else // empire, Westlands — also fallback default
            {
                MBTextManager.SetTextVariable("PLAYER_ACCEPTED_AS_VASSAL", "{=IMSCdhyy}I proclaim you a citizen of the Westlands. Your life and property shall be protected by our laws, and shall not be taken from you except by law. You may serve as a magistrate over towns and villages and as a general over armies, if we call upon you to do so.", false);
            }

            return true;
        }

        // =====================================================================
        // Random other lines that have hard coded references.
        // =====================================================================

        private void AddWoTExtra1Dialogs(CampaignGameStarter starter)
        {
            starter.AddDialogLine(
                "lord_ask_pardon_answer_low_right_to_rule", 
                "lord_ask_pardon", 
                "lord_pretalk", 
                "{=UfpmWfbG}{PLAYER.NAME}, you are a {?PLAYER.GENDER}lady{?}lord{\\?} without a master, holding lands in your name, with only the barest scrap of a claim to legitimacy. No ruler would accept a lasting peace with you.", 
                new ConversationSentence.OnConditionDelegate(this.conversation_lord_ask_pardon_answer_low_right_to_rule_on_condition), null, 101, null);
        }

        public bool conversation_lord_ask_pardon_answer_low_right_to_rule_on_condition()
        {
            return false;
        }

        // =====================================================================
        // Rebel clan lines that have hard coded references.
        // =====================================================================

        private void AddWoTRebelDialogs(CampaignGameStarter starter)
        {
            starter.AddDialogLine(
                "player_wants_to_make_peace_npc_response", 
                "lord_talk_speak_diplomacy_3", 
                "player_wants_to_make_peace_answer", 
                "{=!}{LORD_PEACE_OFFER_ANSWER}", 
                new ConversationSentence.OnConditionDelegate(this.conversation_player_wants_to_make_peace_answer_on_condition), 
                new ConversationSentence.OnConsequenceDelegate(this.conversation_player_wants_to_make_peace_on_consequence), 101, null);
        }

        private bool _willDoPeaceBarter;
        private bool conversation_player_wants_to_make_peace_answer_on_condition()
        {
            this._willDoPeaceBarter = false;
            TextObject textObject;
            if (Hero.OneToOneConversationHero.Clan.IsRebelClan && Hero.OneToOneConversationHero.Clan.IsAtWarWith(Hero.MainHero.MapFaction))
            {
                textObject = new TextObject("{=lH3cgbVX}We will not sign a peace until we are recognized by all the kingdoms of this land.", null);
            }
            else if (Hero.OneToOneConversationHero.Clan.IsUnderMercenaryService)
            {
                textObject = new TextObject("{=bdetTQa6}We are only mercenaries serving under {EMPLOYER_FACTION_INFORMAL_NAME}. We cannot negotiate peace on behalf of our employers.", null);
                textObject.SetTextVariable("EMPLOYER_FACTION_INFORMAL_NAME", Hero.OneToOneConversationHero.Clan.Kingdom.InformalName);
            }
            else if (Hero.OneToOneConversationHero.Clan.IsMinorFaction && Campaign.Current.Models.DiplomacyModel.IsAtConstantWar(Hero.OneToOneConversationHero.MapFaction, Hero.MainHero.MapFaction))
            {
                textObject = new TextObject("{=VWoHoUin}There will be no peace between us and the {ENEMY_INFORMAL_NAME}, any more than the shadow makes peace with the light.", null);
                textObject.SetTextVariable("ENEMY_INFORMAL_NAME", Hero.MainHero.MapFaction.InformalName);
            }
            else if (Hero.OneToOneConversationHero.Clan.Kingdom != null && Hero.OneToOneConversationHero.MapFaction.IsAtWarWith(Hero.MainHero.MapFaction) && Hero.MainHero.Clan.Kingdom != null)
            {
                if (Hero.OneToOneConversationHero.Clan.Kingdom.Leader == Hero.OneToOneConversationHero)
                {
                    textObject = new TextObject("{=efTah9rk}I do not have the authority to make peace on behalf of the {ENEMY_INFORMAL_NAME}. Our council should decide whether to offer peace, and what the terms will be.", null);
                }
                else
                {
                    textObject = new TextObject("{=JY717hPW}I do not have the authority to make peace on behalf of the {ENEMY_INFORMAL_NAME}. {ENEMY_RULER.NAME} and {?ENEMY_RULER.GENDER}her{?}his{\\?} council should decide whether to offer peace, and what the terms will be.", null);
                    StringHelpers.SetCharacterProperties("ENEMY_RULER", Hero.OneToOneConversationHero.Clan.Kingdom.Leader.CharacterObject, null, false);
                }
                textObject.SetTextVariable("ENEMY_INFORMAL_NAME", Hero.OneToOneConversationHero.Clan.Kingdom.InformalName);
            }
            else
            {
                textObject = TextObject.GetEmpty();
                this._willDoPeaceBarter = true;
            }
            MBTextManager.SetTextVariable("LORD_PEACE_OFFER_ANSWER", textObject, false);
            return true;
        }

        private void conversation_player_wants_to_make_peace_on_consequence()
        {
            if (this._willDoPeaceBarter)
            {
                BarterManager instance = BarterManager.Instance;
                Hero mainHero = Hero.MainHero;
                Hero oneToOneConversationHero = Hero.OneToOneConversationHero;
                PartyBase mainParty = PartyBase.MainParty;
                MobileParty partyBelongedTo = Hero.OneToOneConversationHero.PartyBelongedTo;
                instance.StartBarterOffer(mainHero, oneToOneConversationHero, mainParty, (partyBelongedTo != null) ? partyBelongedTo.Party : null, null, new BarterManager.BarterContextInitializer(BarterManager.Instance.InitializeMakePeaceBarterContext), 0, false, new Barterable[]
                {
                    new PeaceBarterable(Hero.OneToOneConversationHero, Clan.PlayerClan.MapFaction, Hero.OneToOneConversationHero.MapFaction, CampaignTime.Years(1f))
                });
            }
            this._willDoPeaceBarter = false;
        }
    }
}
