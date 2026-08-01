using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using static TaleWorlds.Core.ItemObject;

namespace WoT_Code.Behaviours
{
    public class WoTTournamentModel : DefaultTournamentModel
    {
        private const int RegularMin = 2500;
        private const int RegularMax = 7500;
        private const int EliteMin = 7501;
        private const int EliteMax = 15000;

        public override MBList<ItemObject> GetRegularRewardItems(
            Town town, int regularRewardMinValue, int regularRewardMaxValue)
        {
            // Ignore the vanilla 1600/5000 passed in — use our own range
            return base.GetRegularRewardItems(town, RegularMin, RegularMax);
        }

        public override MBList<ItemObject> GetEliteRewardItems(
            Town town, int regularRewardMinValue, int regularRewardMaxValue)
        {
            // Replace hardcoded list with dynamic value-based selection
            // Ignoring vanilla 1600/5000 — using EliteMin/EliteMax instead
            MBList<ItemObject> result = new MBList<ItemObject>();

            foreach (ItemObject item in Game.Current.ObjectManager
                .GetObjectTypeList<ItemObject>())
            {
                if (item.Value < EliteMin || item.Value > EliteMax) continue;

                // Only rewarding combat-relevant item types
                if (item.ItemType != ItemTypeEnum.OneHandedWeapon &&
                    item.ItemType != ItemTypeEnum.TwoHandedWeapon &&
                    item.ItemType != ItemTypeEnum.Polearm &&
                    item.ItemType != ItemTypeEnum.Bow &&
                    item.ItemType != ItemTypeEnum.Crossbow &&
                    item.ItemType != ItemTypeEnum.Thrown &&
                    item.ItemType != ItemTypeEnum.HeadArmor &&
                    item.ItemType != ItemTypeEnum.BodyArmor &&
                    item.ItemType != ItemTypeEnum.LegArmor &&
                    item.ItemType != ItemTypeEnum.HandArmor &&
                    item.ItemType != ItemTypeEnum.Horse &&
                    item.ItemType != ItemTypeEnum.HorseHarness)
                    continue;

                // Exclude One Power items — too powerful as random tournament rewards
                if (item.StringId?.StartsWith("Onepower",
                    StringComparison.OrdinalIgnoreCase) == true) continue;

                result.Add(item);
            }

            // Safety fallback — if our value range yields nothing, use vanilla list
            if (result.IsEmpty())
                return base.GetEliteRewardItems(town, regularRewardMinValue, regularRewardMaxValue);

            return result;
        }
    }
}
