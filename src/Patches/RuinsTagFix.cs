using BattleTech;
using System;
using System.Linq;

namespace TBD.Patches
{
    internal class RuinsTagFix
    {
        /// <summary>
        /// Replaces the ruins tag to allow Opportunity missions in more star systems.
        /// </summary>
        [HarmonyPatch(typeof(StarSystem), nameof(StarSystem.Rehydrate))]
        public static class StarSystem_Rehydrate
        {
            [HarmonyPrepare]
            public static bool Prepare() => AppDomain.CurrentDomain.GetAssemblies().Any(asm => asm.GetName().Name.Equals("OpportunityMissions"));

            [HarmonyPostfix]
            public static void Postfix(StarSystem __instance)
            {
                if (__instance.Tags.Contains("planet_ruins"))
                {
                    __instance.Tags.Remove("planet_ruins");
                    __instance.Tags.Add("planet_other_ruins");
                }
            }
        }
    }
}