using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace WoT_Code.Behaviours
{
    
    internal class HornOfValereBehaviour : MissionLogic
    {
        // StringId of the Horn of Valere item in the XML
        private const string HornStringId = "Hornofvalere1";

        // How many hero types exist
        private const int HeroCount = 10;

        public override void OnAgentShootMissile(
            Agent shooterAgent,
            EquipmentIndex weaponIndex,
            Vec3 position,
            Vec3 velocity,
            Mat3 orientation,
            bool hasRigidBody,
            int forcedMissileIndex)
        {
            base.OnAgentShootMissile(
                shooterAgent, weaponIndex, position, velocity,
                orientation, hasRigidBody, forcedMissileIndex);

            if (!shooterAgent.IsMainAgent) return;

            // Resolve which MissionWeapon to check — consumable ranged = itself,
            // otherwise check the ammo weapon (e.g. crossbow bolt)
            MissionWeapon wieldedWeapon = shooterAgent.Equipment[weaponIndex];
            MissionWeapon weaponToCheck = (wieldedWeapon.CurrentUsageItem != null
                && wieldedWeapon.CurrentUsageItem.IsRangedWeapon
                && wieldedWeapon.CurrentUsageItem.IsConsumable)
                    ? wieldedWeapon
                    : wieldedWeapon.AmmoWeapon;

            // Check via StringId — more reliable than display name
            // Falls back to name check if StringId isn't set
            bool isHorn = weaponToCheck.Item?.StringId == HornStringId
                || weaponToCheck.GetModifiedItemName().ToString().Contains("Horn of Valere");

            if (!isHorn) return;

            SpawnHeroesOfTheHorn(shooterAgent);
        }

        private void SpawnHeroesOfTheHorn(Agent shooterAgent)
        {
            int heroIndex = 1;

            // Spawn in a line around the shooter — 5 to each side
            for (int i = -5; i < 6 && heroIndex <= HeroCount; i++)
            {
                // Spread heroes along the side axis relative to shooter
                Vec3 spawnPosition = shooterAgent.Position
                    + shooterAgent.LookRotation.f       // slightly in front
                    + shooterAgent.LookRotation.s * i;  // spread sideways

                CharacterObject heroCharacter = Game.Current.ObjectManager
                    .GetObject<CharacterObject>("heroofthehorn" + heroIndex);

                if (heroCharacter == null)
                {
                    InformationManager.DisplayMessage(new InformationMessage(
                        $"[WoT] Could not find heroofthehorn{heroIndex} — check XML StringId.",
                        Color.FromUint(0xFF0000FF)));
                    heroIndex++;
                    continue;
                }

                // Null guard on mount key — heroes do not have mounts defined
                ItemObject mountItem = heroCharacter
                    .Equipment[EquipmentIndex.ArmorItemEndSlot].Item;
                string mountKey = mountItem != null
                    ? MountCreationKey.GetRandomMountKeyString(
                        mountItem, heroCharacter.GetMountKeySeed())
                    : string.Empty;

                Vec2 lookDirection = shooterAgent.LookRotation.f.AsVec2;

                AgentBuildData buildData = new AgentBuildData(heroCharacter)
                    .Team(Mission.PlayerTeam)
                    .InitialPosition(in spawnPosition)
                    .InitialDirection(in lookDirection)
                    .CivilianEquipment(false)
                    .NoHorses(true)
                    .NoWeapons(false)
                    .ClothingColor1(Mission.PlayerTeam.Color)
                    .ClothingColor2(Mission.PlayerTeam.Color2)
                    .TroopOrigin(new PartyAgentOrigin(
                        PartyBase.MainParty, heroCharacter, -1,
                        default(UniqueTroopDescriptor), false))
                    .Controller(AgentControllerType.AI); // ← enum not integer

                if (!string.IsNullOrEmpty(mountKey))
                    buildData = buildData.MountKey(mountKey);

                Agent spawnedAgent = Mission.SpawnAgent(buildData, false);

                // Set team and watch state using enums not integers
                spawnedAgent.SetTeam(Mission.PlayerTeam, false);
                spawnedAgent.SetWatchState(Agent.WatchState.Alarmed);

                heroIndex++;
            }
        }
    }
}
