using System;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.GauntletUI.Mission;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;

[OverrideView(typeof(MissionCrosshair))]
public class WoTMissionGauntletCrosshair : MissionGauntletCrosshair
{
    protected override bool GetShouldCrosshairBeVisible()
    {
        // Run vanilla logic first
        if (base.GetShouldCrosshairBeVisible())
            return true;

        // Vanilla returned false — check if it's a One Power crossbow
        // that still has magazine ammo remaining
        Agent mainAgent = Mission.MainAgent;
        if (mainAgent == null) return false;

        MissionWeapon wieldedWeapon = mainAgent.WieldedWeapon;
        if (wieldedWeapon.IsEmpty) return false;

        WeaponComponentData currentUsage = wieldedWeapon.CurrentUsageItem;
        if (currentUsage == null) return false;

        // Only override for crossbows that are reloading with ammo remaining
        if (currentUsage.WeaponClass != WeaponClass.Crossbow) return false;
        if (!wieldedWeapon.IsReloading) return false;
        // Check magazine ammo OR ammo weapon stack
        bool hasAmmoRemaining = wieldedWeapon.Amount > 0
            || wieldedWeapon.AmmoWeapon.Amount > 0;

        if (!hasAmmoRemaining) return false;

        // Check One Power weapon via trail particle field
        if (!IsOnePowerWeapon(wieldedWeapon)) return false;

        return true;
    }

    private bool IsOnePowerWeapon(MissionWeapon wieldedWeapon)
    {
        // Check trail particle on the ammo item, not the crossbow itself
        WeaponComponentData ammoUsage = wieldedWeapon.AmmoWeapon.CurrentUsageItem;
        if (ammoUsage?.TrailParticleName == null) return false;

        // Check ammo weapon's ItemUsage — set to "onepower_*" in XML if working in friendly fire
        return wieldedWeapon.AmmoWeapon.CurrentUsageItem?.ItemUsage
            ?.StartsWith("onepower", StringComparison.OrdinalIgnoreCase) == true;
    }
}