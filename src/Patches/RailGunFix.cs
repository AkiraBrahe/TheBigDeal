using BattleTech;

namespace TBD.Patches
{
    internal class RailGunFix
    {
        /// <summary>
        /// Fixes the Rail Gun weapon definition to have correct ranges and description.
        /// </summary>
        [HarmonyPatch(typeof(WeaponDef), "FromJSON")]
        public static class WeaponDef_FromJSON
        {
            [HarmonyPostfix]
            [HarmonyAfter("BEX.BattleTech.Extended_CE")]
            public static void Postfix(WeaponDef __instance)
            {
                if (__instance.WeaponSubType == WeaponSubType.Gauss &&
                    __instance.ComponentTags.Contains("component_railgun"))
                {
                    float TTWpnRangeMetresPerPoint = Extended_CE.Core.Settings.TTWpnRangeMetresPerPoint;

                    __instance.MinRange = 6f * TTWpnRangeMetresPerPoint;
                    __instance.MaxRange = 28f * TTWpnRangeMetresPerPoint;
                    __instance.RangeSplit = [ 9f * TTWpnRangeMetresPerPoint,
                                             19f * TTWpnRangeMetresPerPoint,
                                             28f * TTWpnRangeMetresPerPoint ];

                    if (__instance.Description.Id is "Weapon_Gauss_RailGun_2-BL" or "Weapon_Gauss_RailGun_2-BS")
                        __instance.Description.Details = "A Star League era relic using lost technology, the Rail Gun electromagnetically hurls massive metal projectiles. Those who assembled The General stripped the weapon of everything they could, which allowed it to be lightened to 25 tons, but at the expense of significantly worse heat dissipation. And there's no guarantee it's safe to fire...";
                    else
                        __instance.Description.Details = "A Star League era relic using lost technology, the Rail Gun electromagnetically hurls massive metal projectiles. Like Gauss Rifles, Rail Guns do not suffer recoil effects from firing.";
                    __instance.Description.Details += "\n\nRail Guns explode if destroyed, taking the entire mounted location with them.";
                }
            }
        }
    }
}