using System;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace WoT_Code.Behaviours
{
    /*
    class DeathBarrier2 : MissionLogic
    {
        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);
        }
        private Blow CreateMissileBlow(Agent attackerAgent)
        {
            Blow blow = new Blow(attackerAgent.Index);
            blow.BlowFlag = BlowFlags.NoSound;
            blow.Direction = Vec3.Forward;
            blow.SwingDirection = blow.Direction;
            blow.GlobalPosition = Vec3.Zero;
            blow.BoneIndex = 0;
            blow.StrikeType = StrikeType.Swing;
            blow.DamageType = DamageTypes.Cut;
            blow.VictimBodyPart = BoneBodyPartType.Head;
            blow.BaseMagnitude = 1.0f;
            blow.MovementSpeedDamageModifier = 1;
            blow.AbsorbedByArmor = 1f;
            blow.InflictedDamage = 169;
            blow.SelfInflictedDamage = 69;
            blow.DamageCalculated = true;
            return blow;
        }
    }
    */
}
