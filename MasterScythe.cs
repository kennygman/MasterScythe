using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace MasterScythe
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    [BepInDependency("com.github.denikson.BepInEx.ConfigurationManager", BepInDependency.DependencyFlags.HardDependency)]
    public class MasterScythePlugin : BaseUnityPlugin
    {
        private const string ModGUID = "com.kenny.masterscythe";
        private const string ModName = "MasterScythe";
        private const string ModVersion = "1.0.0";

        private static ConfigEntry<float> scytheRange;
        private static ConfigEntry<float> scytheCooldown;
        private static Dictionary<string, ConfigEntry<bool>> additionalPlants;
        private static float lastScytheUseTime = 0f;

        private readonly Harmony harmony = new Harmony(ModGUID);

        private void Awake()
        {
            try
            {
                // Initialize configuration
                InitializeConfig();

                // Apply patches
                harmony.PatchAll();

                Logger.LogInfo($"Plugin {ModGUID} is loaded!");
            }
            catch (System.Exception e)
            {
                Logger.LogError($"Error initializing {ModGUID}: {e.Message}");
                Logger.LogError(e.StackTrace);
            }
        }

        private void InitializeConfig()
        {
            try
            {
                // Scythe range configuration
                scytheRange = Config.Bind("General", "ScytheRange", 2f, 
                    new ConfigDescription("Range of the scythe effect", 
                    new AcceptableValueRange<float>(1f, 50f)));

                // Scythe cooldown configuration
                scytheCooldown = Config.Bind("General", "ScytheCooldown", 0.5f,
                    new ConfigDescription("Cooldown between scythe uses in seconds",
                    new AcceptableValueRange<float>(0.1f, 5f)));

                // Additional plants configuration
                additionalPlants = new Dictionary<string, ConfigEntry<bool>>
                {
                    ["JotunPuff"] = Config.Bind("Added Plants", "JotunPuff", true,
                        new ConfigDescription("Allow scythe to work on Jotun Puffs")),
                    ["Magecap"] = Config.Bind("Added Plants", "Magecap", true,
                        new ConfigDescription("Allow scythe to work on Magecaps"))
                };

                // Add reset button for range
                Config.Bind("General", "ResetRange", false,
                    new ConfigDescription("Reset scythe range to default", null, new ConfigurationManagerAttributes { IsAdvanced = true }));
            }
            catch (System.Exception e)
            {
                Logger.LogError($"Error initializing configuration: {e.Message}");
                throw;
            }
        }

        public static float GetScytheRange() => scytheRange.Value;
        public static float GetScytheCooldown() => scytheCooldown.Value;
        public static bool IsAdditionalPlantEnabled(string plantName) => 
            additionalPlants.ContainsKey(plantName) && additionalPlants[plantName].Value;
        
        public static bool CanUseScythe()
        {
            float currentTime = Time.time;
            if (currentTime - lastScytheUseTime >= GetScytheCooldown())
            {
                lastScytheUseTime = currentTime;
                return true;
            }
            return false;
        }
    }
} 