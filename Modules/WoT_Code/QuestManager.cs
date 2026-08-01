using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;
using WoT_Code.Quests;

namespace WoT_Code
{
    public class QuestManager : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.OnCharacterCreationIsOverEvent.AddNonSerializedListener(this, new Action(this.OnNewGameCreatedEventAction));
            CampaignEvents.OnGameLoadFinishedEvent.AddNonSerializedListener(this, new Action(this.AddDialog));
        }

        public override void SyncData(IDataStore dataStore)
        {
        }

        private void AddDialog()
        {
            Campaign.Current.ConversationManager.AddDialogFlow(DialogFlow.CreateDialogFlow("start", 120).NpcLine(new TextObject("Times are tough. The Trolloc Hordes have ravaged my farmstead, killing my dear wife Anita, the only sunshine in this lightforsaken world. It is prophesized that only the Dragon can save us... You are a renowned warrior, who knows, maybe you are the Dragon?, or maybe a Hunter of the Horn?", null), null, null).Condition(() => Hero.OneToOneConversationHero != null && Hero.OneToOneConversationHero.FirstName.ToString() == "Marathen").NpcLine(new TextObject("I have inherited a Horn from my grandfather. He was a weird man. He was a rich noble from Andor, but he decided to take his fortunes to the blight. In the last years of his live he started to have delusions of killing the Dark One and all the Forsaken himself. In the end he took his sword and went into the blight...", null), null, null).NpcLine(new TextObject("Not soon after we found him dead at a nearby Trolloc camp. Yet he did have this horn. He called it 'The Horn of Valere' and always insisted that it must be given to the 'Dragon'.", null), null, null).NpcLine(new TextObject("I am desperate enough to consider that you may be able to help the Dragon. Prove it and I'll give you the Horn. Blow it at a time of need and mighty Heroes of old will come to your aid.", null), null, null).BeginPlayerOptions().PlayerOption(new TextObject("Yes, I'll prove that I am worthy.", null), null).Consequence(new ConversationSentence.OnConsequenceDelegate(this.StartHornOfValereQuest)).CloseDialog().PlayerOption(new TextObject("I'll consider it.", null), null).Consequence(null).CloseDialog().EndPlayerOptions(), null);
        }

        private void OnNewGameCreatedEventAction()
        {
            Hero hero = HeroCreator.CreateSpecialHero(CharacterObject.Find("marathen"), null, null, null, -1);
            hero.SetName(new TextObject("Marathen", null), new TextObject("Marathen", null));
            this.questGiverHornOfValere = hero;
            List<Settlement> list = Settlement.All.ToList<Settlement>();
            Settlement settlement = null;
            foreach (Settlement settlement2 in list)
            {
                //bool flag = settlement2.GetName().ToString() == "Eye of the World";
                bool flag = settlement2.StringId == "castle_malkier_1";
                if (flag)
                {
                    settlement = settlement2;
                }
            }
            bool flag2 = settlement == null;
            if (flag2)
            {
                settlement = Settlement.GetFirst;
            }
            hero.StayingInSettlement = settlement;
            this.AddDialog();
        }

        public void StartHornOfValereQuest()
        {
            QuestBase questBase = new HornOfValereQuest(this.questGiverHornOfValere);
            questBase.StartQuest();
        }

        private Hero questGiverHornOfValere;

        public class QuestManagerTypeDefiner : SaveableTypeDefiner
        {
            public QuestManagerTypeDefiner() : base(345723856)
            {
            }

            protected override void DefineClassTypes()
            {
                base.AddClassDefinition(typeof(HornOfValereQuest), 1, null);
            }
        }
    }
}
