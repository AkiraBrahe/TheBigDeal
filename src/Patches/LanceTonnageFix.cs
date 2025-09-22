using BattleTech.UI;

namespace TBD.Patches
{
    internal class LanceTonnageFix
    {
        /// <summary>
        /// Applies the last valid tonnage requirements to all extra slots, similar to what CAC does.
        /// </summary>
        [HarmonyPatch(typeof(LanceConfiguratorPanel), "SetData")]
        public static class LanceConfiguratorPanel_SetData
        {
            [HarmonyPrepare]
            public static bool Prepare() => Main.CACDetected == false;

            [HarmonyPostfix]
            public static void Postfix(LanceConfiguratorPanel __instance)
            {
                if (__instance == null || __instance.slotMaxTonnages == null)
                    return;

                float lastValidMinTonnage = -1f;
                for (int i = __instance.slotMinTonnages.Length - 1; i >= 0; i--)
                {
                    if (i < __instance.maxUnits && __instance.slotMinTonnages[i] >= 0f)
                    {
                        lastValidMinTonnage = __instance.slotMinTonnages[i];
                        break;
                    }
                }

                float lastValidMaxTonnage = -1f;
                for (int i = __instance.slotMaxTonnages.Length - 1; i >= 0; i--)
                {
                    if (i < __instance.maxUnits && __instance.slotMaxTonnages[i] >= 0f)
                    {
                        lastValidMaxTonnage = __instance.slotMaxTonnages[i];
                        break;
                    }
                }

                for (int i = 0; i < __instance.slotMinTonnages.Length; i++)
                {
                    __instance.slotMinTonnages[i] = i >= __instance.maxUnits ? 0f : lastValidMinTonnage;
                    __instance.slotMaxTonnages[i] = i >= __instance.maxUnits ? 0f : lastValidMaxTonnage;

                    if (__instance.loadoutSlots != null && i < __instance.loadoutSlots.Length)
                    {
                        var slot = __instance.loadoutSlots[i];
                        if (slot.dropTonnageElement != null && slot.dropTonnageText != null)
                        {
                            bool showTonnage = (__instance.slotMinTonnages[i] >= 0f) || (__instance.slotMaxTonnages[i] >= 0f);
                            slot.dropTonnageElement.SetActive(showTonnage);

                            if (__instance.slotMinTonnages[i] >= 0f && __instance.slotMaxTonnages[i] >= 0f)
                                slot.dropTonnageText.SetText("{0} - {1} Tons", __instance.slotMinTonnages[i], __instance.slotMaxTonnages[i]);
                            else if (__instance.slotMinTonnages[i] >= 0f)
                                slot.dropTonnageText.SetText("Min: {0} Tons", __instance.slotMinTonnages[i]);
                            else if (__instance.slotMaxTonnages[i] >= 0f)
                                slot.dropTonnageText.SetText("Max: {0} Tons", __instance.slotMaxTonnages[i]);
                        }
                    }
                }
            }
        }
    }
}
