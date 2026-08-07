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
            public static bool Prepare() => !Main.CACDetected;

            [HarmonyPostfix]
            public static void Postfix(LanceConfiguratorPanel __instance)
            {
                if (__instance == null || __instance.maxUnits <= __instance.slotMinTonnages.Length)
                    return;

                float minLimit = GetLastValidTonnage(__instance.slotMinTonnages, __instance.maxUnits);
                float maxLimit = GetLastValidTonnage(__instance.slotMaxTonnages, __instance.maxUnits);

                if (minLimit >= 0 || maxLimit >= 0)
                {
                    float[] newMinArray = new float[__instance.maxUnits];
                    float[] newMaxArray = new float[__instance.maxUnits];

                    for (int i = 0; i < __instance.maxUnits; i++)
                    {
                        newMinArray[i] = i < __instance.slotMinTonnages.Length ? __instance.slotMinTonnages[i] : minLimit;
                        newMaxArray[i] = i < __instance.slotMaxTonnages.Length ? __instance.slotMaxTonnages[i] : maxLimit;

                        if (i < __instance.loadoutSlots.Length)
                        {
                            var slot = __instance.loadoutSlots[i];
                            UpdateSlotUI(slot, newMinArray[i], newMaxArray[i]);
                        }
                    }

                    __instance.slotMinTonnages = newMinArray;
                    __instance.slotMaxTonnages = newMaxArray;
                }
            }

            private static float GetLastValidTonnage(float[] tonnageArray, int maxUnits)
            {
                for (int i = tonnageArray.Length - 1; i >= 0; i--)
                {
                    if (i < maxUnits && tonnageArray[i] >= 0f)
                    {
                        return tonnageArray[i];
                    }
                }
                return -1f;
            }

            private static void UpdateSlotUI(LanceLoadoutSlot slot, float min, float max)
            {
                if (slot?.dropTonnageElement == null || slot.dropTonnageText == null) return;

                bool hasLimit = min >= 0f || max >= 0f;
                slot.dropTonnageElement.SetActive(hasLimit);

                if (hasLimit)
                {
                    if (min >= 0f && max >= 0f)
                        slot.dropTonnageText.SetText("{0} - {1} Tons", min, max);
                    else if (min >= 0f)
                        slot.dropTonnageText.SetText("Min: {0} Tons", min);
                    else
                        slot.dropTonnageText.SetText("Max: {0} Tons", max);
                }
            }
        }
    }
}