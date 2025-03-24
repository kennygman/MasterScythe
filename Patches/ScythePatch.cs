using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using BepInEx.Logging;

namespace MasterScythe.Patches
{
    [HarmonyPatch(typeof(ItemDrop.ItemData), nameof(ItemDrop.ItemData.IsWeapon))]
    public static class ScythePatch
    {
        private static readonly ManualLogSource Log = BepInEx.Logging.Logger.CreateLogSource("MasterScythe.ScythePatch");

        [HarmonyPrefix]
        public static bool Prefix(ItemDrop.ItemData __instance, ref bool __result)
        {
            try
            {
                if (__instance?.m_shared?.m_name == "$item_scythe")
                {
                    __result = true;
                    return false;
                }
                return true;
            }
            catch (System.Exception e)
            {
                Log.LogError($"Error in ScythePatch.Prefix: {e.Message}");
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.UseItem))]
    public static class ScytheUsePatch
    {
        private static readonly ManualLogSource Log = BepInEx.Logging.Logger.CreateLogSource("MasterScythe.ScytheUsePatch");

        [HarmonyPrefix]
        public static bool Prefix(Player __instance, ItemDrop.ItemData item)
        {
            try
            {
                if (item?.m_shared?.m_name != "$item_scythe")
                    return true;

                if (!MasterScythePlugin.CanUseScythe())
                {
                    Log.LogDebug("Scythe is on cooldown");
                    return false;
                }

                var range = MasterScythePlugin.GetScytheRange();
                var hits = Physics.OverlapSphere(__instance.transform.position, range);
                int plantsHarvested = 0;

                foreach (var hit in hits)
                {
                    if (hit == null) continue;

                    var plant = hit.GetComponent<Plant>();
                    if (plant == null) continue;

                    try
                    {
                        // Check if it's an additional plant that's enabled
                        if (MasterScythePlugin.IsAdditionalPlantEnabled(plant.m_name))
                        {
                            plant.Interact(__instance, false, false);
                            plantsHarvested++;
                        }
                    }
                    catch (System.Exception e)
                    {
                        Log.LogError($"Error harvesting plant {plant.m_name}: {e.Message}");
                    }
                }

                if (plantsHarvested > 0)
                {
                    Log.LogDebug($"Harvested {plantsHarvested} plants with scythe");
                }

                return false;
            }
            catch (System.Exception e)
            {
                Log.LogError($"Error in ScytheUsePatch.Prefix: {e.Message}");
                return true;
            }
        }
    }
} 