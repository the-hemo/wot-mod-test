using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

// The purpose of this class is to allow for a shotgun effect when shooting certain weapons.
// It will check if the weapon has the "multiple projectiles" tag and if so, it will spawn additional projectiles with a spread and speed variation.

namespace WoT_Code.Behaviours
{
    internal class ShotgunEquipmentMissionLogic : MissionLogic
    {
        public int Num = 7;
        public float Spread = 20;
        private bool _once;
        public float SpeedDiff = 1;

        public override void OnAgentShootMissile(Agent shooterAgent, EquipmentIndex weaponIndex, Vec3 position, Vec3 velocity, Mat3 orientation, bool hasRigidBody, int forcedMissileIndex)
        {
            base.OnAgentShootMissile(shooterAgent, weaponIndex, position, velocity, orientation, hasRigidBody, forcedMissileIndex);
            if (this._once)
            {
                return;
            }

            MissionWeapon missileWeapon;
            if (shooterAgent.Equipment[weaponIndex].CurrentUsageItem.IsRangedWeapon && shooterAgent.Equipment[weaponIndex].CurrentUsageItem.IsConsumable)
            {
                missileWeapon = shooterAgent.Equipment[weaponIndex];
            }
            else
            {
                missileWeapon = shooterAgent.Equipment[weaponIndex].AmmoWeapon;
            }
            if (!missileWeapon.GetModifiedItemName().Value.Contains("multiple projectiles"))
            {
                return;
            }
            foreach (Mission.Missile missile in Mission.Current.MissilesList)
            {
                if (missile.ShooterAgent == shooterAgent)
                {
                    this._once = true;
                    for (int i = 0; i < Num; i++)
                    {
                        Vec3 vec = velocity;
                        float num4 = (float)missileWeapon.CurrentUsageItem.MissileSpeed + MBRandom.RandomFloatNormal * SpeedDiff;
                        if (num4 < 1f)
                        {
                            num4 = 1f;
                        }
                        vec.x += MBRandom.RandomFloatNormal * Spread;
                        vec.y += MBRandom.RandomFloatNormal * Spread;
                        vec.z += MBRandom.RandomFloatNormal * Spread;
                        float speed = vec.Normalize();
                        Vec3 vec2 = position + vec;
                        Mat3 orientation2 = LookAtWorld(vec2, position);
                        Mission.Current.AddCustomMissile(shooterAgent, missileWeapon, position, vec, orientation2, num4, speed, hasRigidBody, missile.MissionObjectToIgnore, forcedMissileIndex);

                        foreach (MissionBehavior missionBehavior in Mission.Current.MissionBehaviors)
                        {
                            missionBehavior.OnAgentShootMissile(shooterAgent, weaponIndex, position, vec, orientation, hasRigidBody, forcedMissileIndex);
                        }
                    }
                    this._once = false;
                    break;
                }
            }

        }
        public static Mat3 LookAtWorld(in Vec3 At, in Vec3 Eye)
        {
            Vec3 vec = (At - Eye).NormalizedCopy();
            Vec3 vec2 = Vec3.CrossProduct(vec, Vec3.Up).NormalizedCopy();
            Vec3 vec3 = Vec3.CrossProduct(vec2, vec);
            vec2.w = -Vec3.DotProduct(vec2, Eye);
            vec.w = -Vec3.DotProduct(vec, Eye);
            vec3.w = -Vec3.DotProduct(vec3, Eye);
            return new Mat3(vec2, vec, vec3);
        }
    }
}
