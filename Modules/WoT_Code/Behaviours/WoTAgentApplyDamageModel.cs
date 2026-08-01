using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SandBox.GameComponents;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ComponentInterfaces;

namespace WoT_Code.Behaviours
{
    public class WoTAgentApplyDamageModel : SandboxAgentApplyDamageModel
    {
        public override bool CanWeaponIgnoreFriendlyFireChecks(WeaponComponentData weapon)
        {
            if (base.CanWeaponIgnoreFriendlyFireChecks(weapon))
                return true;

            if (weapon?.ItemUsage?.StartsWith("onepower", StringComparison.OrdinalIgnoreCase) == true) return true;
            else if (weapon?.TrailParticleName?.StartsWith("onepower", StringComparison.OrdinalIgnoreCase) == true) return true;
            return false;
            //return weapon?.ItemUsage?.StartsWith("onepower", StringComparison.OrdinalIgnoreCase) == true;

            //return weapon?.TrailParticleName
            //    ?.StartsWith("onepower", StringComparison.OrdinalIgnoreCase) == true;
        }

        public override bool IsDamageIgnored(in AttackInformation attackInformation, in AttackCollisionData collisionData)
        {
            if (base.IsDamageIgnored(attackInformation, collisionData)) return true;

            if (attackInformation.IsFriendlyFire)
            {
                //string weaponId = attackInformation.AttackerWeapon.Item?.Id.ToString();
                string weaponId = attackInformation.AttackerWeapon.Item?.StringId;
                if (!string.IsNullOrEmpty(weaponId) && weaponId.StartsWith("onepower", StringComparison.OrdinalIgnoreCase))
                return true;
            }

            return false;

            //if (attackInformation.AttackerWeapon.Item?.Id.ToString().StartsWith("onepower", StringComparison.OrdinalIgnoreCase) == true) return true;
            //return false;

        }
    }
}
