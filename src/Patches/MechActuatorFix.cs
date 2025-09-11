using Extended_CE;
using Newtonsoft.Json;
using System.IO;
using System.Linq;

namespace TBD.Patches
{
    /// <summary>
    /// Loads custom actuator settings from TBD's MechSettings.json and merges them with the settings from BT_Extended_CE.
    /// </summary>
    [HarmonyPatch(typeof(BTComponents), "Actuators", MethodType.Getter)]
    public static class MechActuatorFix
    {
        private static bool _patched = false;

        [HarmonyPostfix]
        public static void Postfix(ref ActuatorInfo __result)
        {
            if (_patched || __result == null)
                return;

            try
            {
                string tbdMechSettingsPath = Path.Combine(Main.modDir, "MechSettings.json");
                if (!File.Exists(tbdMechSettingsPath))
                {
                    Main.Log.LogDebug("TBD MechSettings.json not found, skipping merge.");
                    _patched = true;
                    return;
                }

                var tbdActuatorInfo = JsonConvert.DeserializeObject<ActuatorInfo>(File.ReadAllText(tbdMechSettingsPath));
                __result.MechsWithoutLeftArmLower.AddRange(tbdActuatorInfo.MechsWithoutLeftArmLower.Except(__result.MechsWithoutLeftArmLower));
                __result.MechsWithoutRightArmLower.AddRange(tbdActuatorInfo.MechsWithoutRightArmLower.Except(__result.MechsWithoutRightArmLower));
                __result.MechsWithoutLeftHand.AddRange(tbdActuatorInfo.MechsWithoutLeftHand.Except(__result.MechsWithoutLeftHand));
                __result.MechsWithoutRightHand.AddRange(tbdActuatorInfo.MechsWithoutRightHand.Except(__result.MechsWithoutRightHand));

                Main.Log.LogDebug("Successfully merged TBD MechSettings.");
            }
            catch (System.Exception ex)
            {
                Main.Log.LogException("Failed to merge TBD MechSettings.", ex);
            }

            _patched = true;
        }
    }
}