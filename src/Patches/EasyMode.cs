using BattleTech.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace TBD.Patches
{
    internal class EasyMode
    {
        /// <summary>
        /// Allows for additional player mechs in TBD contracts.
        /// </summary>
        [HarmonyPatch(typeof(ContractOverride), "FromJSONFull")]
        [HarmonyPatch(typeof(ContractOverride), "FullRehydrate")]
        public static class ContractOverride_Patches
        {
            [HarmonyPrepare]
            public static bool Prepare() => Main.Settings.EasyMode.AdditionalPlayerMechs;

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
            public static bool Prepare() => Main.Settings.EasyMode.AdditionalPlayerMechs;

            [HarmonyPostfix]
            public static void Postfix(MissionControl.MissionControl __instance, ref bool __result)
            {
                string contractId = __instance?.CurrentContract?.Override?.ID;
                if (contractId != null)
                {
                    if (Main.TBDContractIds.Contains(contractId))
                    {
                        __result = true;
                    }
                }
            }
        }

        /// <summary>
        /// Allows saving between consecutive drops in TBD contracts.
        /// </summary>
        [HarmonyPatch]
        public static class PreForceTakeContractSave_Patch
        {
            [HarmonyTargetMethod]
            public static MethodBase TargetMethod()
            {
                var type = AccessTools.TypeByName("CustAmmoCategories.PreForceTakeContractSave");
                return type != null ? AccessTools.Method(type, "ApplyEventAction_prefix") : null;
            }

            [HarmonyPrepare]
            public static bool Prepare() => Main.CACDetected && !Main.Settings.EasyMode.SaveBetweenConsecutiveDrops;

            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                try
                {
                    var matcher = new CodeMatcher(instructions)
                        .MatchForward(false,
                            new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(ContractOverride), "disableCancelButton")),
                            new CodeMatch(i => i.opcode.FlowControl == FlowControl.Cond_Branch));
                    if (matcher.IsInvalid) return instructions;
                    matcher.SetInstruction(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PreForceTakeContractSave_Patch), "IsConsecutiveAndNotTBDContract")));

                    Main.Log.LogDebug("Patched PreForceTakeContractSave to skip TBD contracts.");
                    return matcher.InstructionEnumeration();
                }
                catch (Exception ex)
                {
                    Main.Log.LogException("Failed to transpile PreForceTakeContractSave. Patch will not be applied.", ex);
                    return instructions;
                }
            }

            public static bool IsConsecutiveAndNotTBDContract(ContractOverride contractOverride)
            {
                return contractOverride != null && (contractOverride.disableCancelButton && !Main.TBDContractIds.Contains(contractOverride.ID));
            }
        }
    }
}
