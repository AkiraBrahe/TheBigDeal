using HBS.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace TBD
{
    public class Main
    {
        internal static Harmony harmony;
        internal static string modDir;
        internal static ILog Log { get; private set; }
        internal static ModSettings Settings { get; private set; }
        internal static HashSet<string> TBDContractIds { get; private set; } = [];
        internal static bool CACDetected { get; private set; } = false;

        public class ModSettings
        {
            public bool AdditionalPlayerMechs { get; set; } = false;
            public int ExternalHeatPerActivationCap { get; set; } = 45;
            public bool ScaleObjectiveBuildingStructure { get; set; } = false;
        }

        public static void Init(string directory, string settingsJSON)
        {
            modDir = directory;
            Log = Logger.GetLogger("TBD");
            Logger.SetLoggerLevel("TBD", LogLevel.Debug);

            try
            {
                Settings = JsonConvert.DeserializeObject<ModSettings>(settingsJSON) ?? new ModSettings();
                harmony = new Harmony("com.github.Hounfor.TBD");
                ApplySettings();
                CACDetected = TrySetupCACIntegration();
                harmony.PatchAll(Assembly.GetExecutingAssembly());
                Log.Log("Mod initialized!");
            }
            catch (Exception ex)
            {
                Log.LogException(ex);
            }
        }

        internal static void ApplySettings()
        {
            if (Settings.AdditionalPlayerMechs)
                LoadTBDContractIds();

            if (FullXotlTables.Core.Settings?.UnitTableReferences != null)
                AddTBDUnitTableReferences();

            // Disable heat cap per mech activation
            if (Settings.ExternalHeatPerActivationCap != 45)
                Extended_CE.Core.Settings.ExternalHeatPerActivationCap = Settings.ExternalHeatPerActivationCap;

            // Disable building structure scaling
            if (!Settings.ScaleObjectiveBuildingStructure)
                IRTweaks.Mod.Config.Fixes.ScaleObjectiveBuildingStructure = false;
        }

        internal static void LoadTBDContractIds()
        {
            string contractsPath = Path.Combine(modDir, "contracts");
            if (!Directory.Exists(contractsPath))
                return;

            string[] files = Directory.GetFiles(contractsPath, "*.json", SearchOption.AllDirectories);
            foreach (string file in files)
            {
                TBDContractIds.Add(Path.GetFileNameWithoutExtension(file));
            }

            Log.LogDebug($"Loaded {TBDContractIds.Count} contract IDs from contracts subfolder.");
        }

        internal static void AddTBDUnitTableReferences()
        {
            try
            {
                var newEntries = new Dictionary<string, FullXotlTables.UnitInfo>
                {
                    { "BattleBudgies", new FullXotlTables.UnitInfo { PrimaryFaction = "MercenariesD", Vehicles = "Kurita" } },
                    { "BlackSabbath", new FullXotlTables.UnitInfo { PrimaryFaction = "PiratesDamned", Vehicles = "Kurita", SecondaryFaction = "KuritaLocals", SecondaryChance = 20.0f } },
                    { "BowmanBH", new FullXotlTables.UnitInfo { PrimaryFaction = "MercenariesC", Vehicles = "Steiner" } },
                    { "CommandosMiguel", new FullXotlTables.UnitInfo { PrimaryFaction = "TaurianConcordat", Vehicles = "TaurianConcordat" } },
                    { "ComStarUnknown", new FullXotlTables.UnitInfo { PrimaryFaction = "ComStar", Vehicles = "ComStar" } },
                    { "DabtonBrothers", new FullXotlTables.UnitInfo { PrimaryFaction = "DavionLocals", Vehicles = "Davion", SecondaryFaction = "SteinerLocals", SecondaryChance = 20.0f } },
                    { "DCRecords", new FullXotlTables.UnitInfo { PrimaryFaction = "MercenariesC", Vehicles = "Davion" } },
                    { "FirestarterRecords", new FullXotlTables.UnitInfo { PrimaryFaction = "DavionA", Vehicles = "Davion" } },
                    { "HadleyCrew", new FullXotlTables.UnitInfo { PrimaryFaction = "PiratesDamned", Vehicles = "Davion" } },
                    { "LocalPunks", new FullXotlTables.UnitInfo { PrimaryFaction = "DavionLocals", Vehicles = "Davion" } },
                    { "MonolithBrigade", new FullXotlTables.UnitInfo { PrimaryFaction = "MercenariesA" } },
                    { "PiratesASP", new FullXotlTables.UnitInfo { PrimaryFaction = "KuritaLocals", Vehicles = "Kurita" } },
                    { "PiratesCDE", new FullXotlTables.UnitInfo { PrimaryFaction = "KuritaLocals", Vehicles = "Kurita" } },
                    { "PurpleHaze", new FullXotlTables.UnitInfo { PrimaryFaction = "TaurianConcordatA", Vehicles = "TaurianConcordat" } },
                    { "RebelsCalypso", new FullXotlTables.UnitInfo { PrimaryFaction = "LiaoC", Vehicles = "Liao" } },
                    { "ThugsTarkin", new FullXotlTables.UnitInfo { PrimaryFaction = "TaurianConcordatA", Vehicles = "TaurianConcordat" } }
                };

                foreach (var entry in newEntries)
                {
                    if (!FullXotlTables.Core.Settings.UnitTableReferences.ContainsKey(entry.Key))
                    {
                        FullXotlTables.Core.Settings.UnitTableReferences.Add(entry.Key, entry.Value);
                    }
                }

                Log.LogDebug($"Added {newEntries.Count} TBD unit table references.");
            }
            catch (Exception ex)
            {
                Log.LogException(ex);
            }
        }

        internal static bool TrySetupCACIntegration()
        {
            try
            {
                Assembly cacAssembly = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(asm => asm.GetName().Name.Equals("CustomAmmoCategories"));
                if (cacAssembly == null) return false;

                Type PreForceTakeContractSaveType = cacAssembly.GetType("CustAmmoCategories.PreForceTakeContractSave");
                if (PreForceTakeContractSaveType == null) return false;

                FieldInfo PreForceTakeContractSaveField = PreForceTakeContractSaveType.GetField("SkipSave", BindingFlags.Public | BindingFlags.Static);
                if (PreForceTakeContractSaveField == null) return false;
                PreForceTakeContractSaveField.SetValue(null, true);

                Log.LogDebug("CustomAmmoCategories reflection setup successful.");
                return true;
            }
            catch (Exception)
            {
                Log.LogDebug("CustomAmmoCategories reflection setup failed. CAC-C was not detected or isn't present.");
                return false;
            }
        }
    }
}