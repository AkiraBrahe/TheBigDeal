namespace TBD
{
    public class ModSettings
    {
        public EasyModeSettings EasyMode { get; set; } = new EasyModeSettings();
        public int ExternalHeatPerActivationCap { get; set; } = 45;
        public bool ScaleObjectiveBuildingStructure { get; set; } = false;
    }

    public class EasyModeSettings
    {
        public bool AdditionalPlayerMechs { get; set; } = false;
        public bool SaveBetweenConsecutiveDrops { get; set; } = false;
    }
}
