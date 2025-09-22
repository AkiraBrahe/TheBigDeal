using BattleTech.Framework;
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

        /// <summary>
        /// Allows saving between consecutive drops in TBD contracts.
        /// </summary>
        [HarmonyPatch]
        public static class PreForceTakeContractSave_ApplyEventAction_prefix_Transpiler
        {
            public static MethodBase TargetMethod()
            {
                var type = AccessTools.TypeByName("CustAmmoCategories.PreForceTakeContractSave");
                return type != null ? AccessTools.Method(type, "ApplyEventAction_prefix") : null;
            }

            [HarmonyPrepare]
            public static bool Prepare() => Main.CACDetected && !Main.Settings.EasyMode.SaveBetweenConsecutiveDrops;

            public static bool IsTBDContract(ContractOverride contractOverride)
            {
                if (contractOverride == null) return false;
                return Main.TBDContractIds.Contains(contractOverride.ID);
            }

            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var matcher = new CodeMatcher(instructions);
                matcher.MatchForward(false,
                    new CodeMatch(i => i.IsLdloc()),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(ContractOverride), "disableCancelButton")),
                    new CodeMatch(OpCodes.Ldc_I4_0),
                    new CodeMatch(OpCodes.Ceq)
                );

                if (matcher.IsInvalid)
                {
                    Main.Log.LogError("Failed to transpile PreForceTakeContractSave.");
                    return instructions;
                }

                var instruction = matcher.Instruction;
                matcher.Advance(3).Insert(
                    new CodeInstruction(instruction.opcode, instruction.operand),
                    CodeInstruction.Call(typeof(PreForceTakeContractSave_ApplyEventAction_prefix_Transpiler), nameof(IsTBDContract)),
                    new CodeInstruction(OpCodes.Or)
                );

                Main.Log.LogDebug("Transpiler for PreForceTakeContractSave applied successfully.");
                return matcher.InstructionEnumeration();
            }
        }
    }
}
