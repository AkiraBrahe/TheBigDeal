using BattleTech.Framework;

namespace TBD.Patches
{
    internal class AdditionalPlayerMechs
    {
        /// <summary>
        /// Allows for additional player mechs in TBD contracts.
        /// </summary>
        [HarmonyPatch(typeof(ContractOverride), "FromJSONFull")]
        [HarmonyPatch(typeof(ContractOverride), "FullRehydrate")]
        public static class ContractOverride_Patches
        {
            [HarmonyPrepare]
            public static bool Prepare() => Main.Settings.AdditionalPlayerMechs && Main.TBDContractIds.Count > 0;

            [HarmonyPostfix]
            [HarmonyPriority(Priority.Last)]
            public static void Postfix(ContractOverride __instance)
            {
                if (Main.TBDContractIds.Contains(__instance.ID) &&
                    __instance.maxNumberOfPlayerUnits == 4)
                {
                    __instance.maxNumberOfPlayerUnits = Main.CACDetected ? 12 : 8;
                    Main.Log.LogDebug($"Patching TBD contract '{__instance.ID}' to allow for {__instance.maxNumberOfPlayerUnits} player mechs.");
                }
            }
        }

        [HarmonyPatch(typeof(MissionControl.MissionControl), "AreAdditionalPlayerMechsAllowed")]
        public static class MissionControl_AreAdditionalPlayerMechsAllowed
        {
            [HarmonyPrepare]
            public static bool Prepare() => Main.Settings.AdditionalPlayerMechs && Main.TBDContractIds.Count > 0;

            [HarmonyPostfix]
            public static void Postfix(MissionControl.MissionControl __instance, ref bool __result)
            {
                var contract = __instance.CurrentContract;
                if (contract == null) return;

                var contractOverride = contract.Override;
                if (contractOverride == null) return;

                string contractId = contractOverride.ID;
                if (contractId == null) return;

                if (Main.TBDContractIds.Contains(contractId))
                {
                    __result = true;
                }
            }
        }
    }
}
