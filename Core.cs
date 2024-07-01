using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
namespace MooAPI
{
    [BepInPlugin("dev.mooskyfish.MooAPI", "MooAPI", "1.0.0")]
    [BepInProcess("Bug Fables.exe")]
    public class Plugin : BaseUnityPlugin
    {
        public const string Version = "1.0.0-PROTOTYPE";
        internal static new ManualLogSource Logger;
        internal static string[,] OG_skilldata;
        private Harmony Harmony;
        public static ConfigEntry<bool> CustomAttackMethod;
        public void Awake()
        {
            Harmony = new Harmony("dev.mooskyfish.MooAPI");
            Logger = base.Logger;
            CustomAttackMethod = Config.Bind("Config", "Use Method 3 for Custom Attacks", true, "");
            try
            {
                if (CustomAttackMethod.Value)
                {
                    Logger.LogInfo("Custom Attacks - Using Method 3");
                    Harmony.PatchAll(typeof(CustomAttack.Patches.Method3_DoAction));
                }
                else
                {
                    Logger.LogInfo("Custom Attacks - Using Method 1");
                    Harmony.PatchAll(typeof(CustomAttack.Patches.Method1_DoAction));
                }
                Harmony.PatchAll(typeof(CustomAttack.Patches.RefreshSkills_Patch));
                Harmony.PatchAll(typeof(CustomAttack.Patches.SetVariables_Patch));
                Harmony.PatchAll(typeof(CustomMaps.Patches.LoadMap_Patch));
            }
            catch (Exception e)
            {
                Logger.LogError($"{e}\nUNPATCHING");
                Harmony.UnpatchSelf();
            }
        }
    }
}
