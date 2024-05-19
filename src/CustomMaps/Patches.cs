using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace MooAPI.CustomMaps
{
    internal class Patches
    {
        [HarmonyPatch(typeof(MainManager), nameof(MainManager.LoadMap), [typeof(int)])]
        public static class LoadMap_Patch
        {
            [HarmonyTranspiler]
            static IEnumerable<CodeInstruction> Patch(IEnumerable<CodeInstruction> insts, ILGenerator iLGen)
            {
                var codematch = new CodeMatcher(insts, iLGen);
                codematch.MatchForward(true,
                    new CodeMatch(OpCodes.Ldstr),
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Box),
                    new CodeMatch(OpCodes.Call),
                    new CodeMatch(OpCodes.Call)
                ).Set(OpCodes.Call, AccessTools.Method(typeof(Core), nameof(Core.LoadMap)));
                return codematch.InstructionEnumeration();
            }
        }
    }
}
