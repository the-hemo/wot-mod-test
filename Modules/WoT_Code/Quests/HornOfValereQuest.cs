using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace WoT_Code.Quests
{
    internal class HornOfValereQuest : QuestBase
    {
        public override TextObject Title
        {
            get
            {
                return new TextObject("Find the Horn", null);
            }
        }

        public override bool IsRemainingTimeHidden
        {
            get
            {
                return true;
            }
        }

        public override string SpecialQuestType
        {
            get { return "FindHornOfValere"; }
        }

        /*public override bool IsSpecialQuest
       {
           get
           {
               return true;
           }
       }*/

        public HornOfValereQuest(Hero questGiver) : base("FindHornOfValere", questGiver, CampaignTime.Years(10000f), 0)
        {
            base.AddLog(new TextObject("Prove that you are a Hero worthy of the Hunter of the Horn.", null), false);
            this.questGiver = questGiver;
            this.trollocWeaponLog = base.AddDiscreteLog(new TextObject("Gather enough Trolloc weapons to prove that you are a true Hunter.", null), new TextObject("Gather Trolloc weapons", null), 0, 100, null, false);
            this.SetDialogs();
        }

        protected override void OnStartQuest()
        {
            base.OnStartQuest();
            this.SetDialogs();
        }

        protected override void RegisterEvents()
        {
            base.RegisterEvents();
            CampaignEvents.PlayerInventoryExchangeEvent.AddNonSerializedListener(this, new Action<List<ValueTuple<ItemRosterElement, int>>, List<ValueTuple<ItemRosterElement, int>>, bool>(this.KeepTrackOfSupplies));
            CampaignEvents.OnUnitRecruitedEvent.AddNonSerializedListener(this, new Action<CharacterObject, int>(this.KeepTrackOfSupplies));
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(this.KeepTrackOfSupplies));
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(this.SetDialogs));
            CampaignEvents.TickEvent.AddNonSerializedListener(this, new Action<float>(this.KeepTrackOfSupplies));
        }

        protected override void InitializeQuestOnGameLoad()
        {
            this.SetDialogs();
        }

        protected override void SetDialogs()
        {
            this.AddDialog();
        }

        private void SetDialogs(CampaignGameStarter campaignGameStarter)
        {
            this.AddDialog();
        }

        private void AddDialog()
        {
            Campaign.Current.ConversationManager.AddDialogFlow(DialogFlow.CreateDialogFlow("start", 121).NpcLine(new TextObject("I do not think that you worthy of inclusion to heroic ranks of the Hunters of the Horn... kill more Trollocs, then we'll talk.", null), null, null).Condition(() => Hero.OneToOneConversationHero != null && Hero.OneToOneConversationHero.FirstName.ToString() == "Marathen" && !this.AreRequirmentsMet()).BeginPlayerOptions().PlayerOption(new TextObject("I'll slaughter them.", null), null).NpcLine(new TextObject("They all always say that.", null), null, null).CloseDialog().EndPlayerOptions().CloseDialog(), this);
            Campaign.Current.ConversationManager.AddDialogFlow(DialogFlow.CreateDialogFlow("start", 122).NpcLine(new TextObject("You did what? You slaughtered more than a hundred Trollocs? This is no feat of an ordinary Hunter... Praise the light! You truely are worthy of the Horn! Here take the Horn and go and slay the Dark One... Save us all!", null), null, null).Condition(() => Hero.OneToOneConversationHero != null && Hero.OneToOneConversationHero.FirstName.ToString() == "Marathen" && this.AreRequirmentsMet()).BeginPlayerOptions().PlayerOption(new TextObject("I will use the Horn to the best of my abilites.", null), null).Consequence(new ConversationSentence.OnConsequenceDelegate(this.CompleteQuest)).CloseDialog().EndPlayerOptions().CloseDialog(), this);
        }

        private bool AreRequirmentsMet()
        {
            List<JournalLog> list = base.JournalEntries.ToList<JournalLog>();
            foreach (JournalLog journalLog in list)
            {
                bool flag = journalLog != null && journalLog.TaskName != null;
                if (flag)
                {
                    bool flag2 = journalLog.TaskName.ToString() == "Gather Trolloc weapons" && journalLog.HasBeenCompleted();
                    if (flag2)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void KeepTrackOfSupplies(CampaignGameStarter campaign)
        {
            this.KeepTrackOfBars();
        }

        private void KeepTrackOfSupplies(List<ValueTuple<ItemRosterElement, int>> purchasedItems, List<ValueTuple<ItemRosterElement, int>> soldItems, bool isTrading)
        {
            this.KeepTrackOfBars();
        }

        private void KeepTrackOfSupplies(CharacterObject a, int x)
        {
            this.KeepTrackOfBars();
        }

        private void KeepTrackOfSupplies(float d)
        {
            this.KeepTrackOfBars();
        }

        private void KeepTrackOfBars()
        {
            List<ItemRosterElement> list = MobileParty.MainParty.ItemRoster.ToList<ItemRosterElement>();
            int num = 0;
            string[] source = new string[]
            {
                "Trollocshield1",
                "Trollocshield2",
                "Trollocweapon1",
                "Trollocweapon2",
                "Trollocweapon3",
                "Trollocweapon4",
                "Trollocweapon6",
                "Trollocweapon5",
                "TrollocBow"
            };
            foreach (ItemRosterElement itemRosterElement in list)
            {
                bool flag = source.Contains(itemRosterElement.EquipmentElement.Item.StringId.ToString());
                if (flag)
                {
                    num += itemRosterElement.Amount;
                }
            }
            List<JournalLog> list2 = base.JournalEntries.ToList<JournalLog>();
            foreach (JournalLog journalLog in list2)
            {
                bool flag2 = journalLog != null && journalLog.TaskName != null;
                if (flag2)
                {
                    bool flag3 = journalLog.TaskName.ToString() == "Gather Trolloc weapons";
                    if (flag3)
                    {
                        journalLog.UpdateCurrentProgress(num);
                    }
                }
            }
        }

        private void CompleteQuest()
        {
            ItemObject @object = Game.Current.ObjectManager.GetObject<ItemObject>("Hornofvalere1");
            PartyBase.MainParty.ItemRoster.AddToCounts(@object, 1);
            KillCharacterAction.ApplyByMurder(Hero.FindFirst((Hero hero) => hero.FirstName.Value == "Marathen"), null, true);
            base.CompleteQuestWithSuccess();
        }

        protected override void HourlyTick()
        {
        }

        private Hero questGiver = null;

        private JournalLog trollocWeaponLog = null;
    }
}
