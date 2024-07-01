using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace MooAPI.CustomAttack
{
    internal class Patches
    {
        [HarmonyPatch(typeof(MainManager), nameof(MainManager.SetVariables))]
        public static class SetVariables_Patch
        {
            [HarmonyPostfix]
            static void Patch()
            {
                Plugin.OG_skilldata ??= MainManager.skilldata;
                Core.RefreshCustomMoves();
            }
        }
        [HarmonyPatch(typeof(MainManager), nameof(MainManager.RefreshSkills))]
        public static class RefreshSkills_Patch // PROTOTYPE USE - MAKES ALL CUSTOM SKILLS AVAILABLE
        {
            [HarmonyPostfix]
            static void Patch()
            {
                var playerdata = MainManager.instance.playerdata;
                for (var i = 0; i < playerdata.Length; i++)
                {
                    switch (playerdata[i].trueid)
                    {
                        case 0:
                            playerdata[i].skills.AddRange(Core.Skills.Where(i => i.vi).Select(i => i.id));
                            break;
                        case 1:
                            playerdata[i].skills.AddRange(Core.Skills.Where(i => i.kabbu).Select(i => i.id));
                            break;
                        case 2:
                            playerdata[i].skills.AddRange(Core.Skills.Where(i => i.leif).Select(i => i.id));
                            break;
                    }
                }
            }
        }
        [HarmonyPatch(typeof(BattleControl), "DoAction")]
        public static class Method1_DoAction
        {
            [HarmonyPostfix]
            static IEnumerator Patch(IEnumerator r, EntityControl entity, int actionid)
            {
                var i = 0;
                while (r.MoveNext())
                {
                    if (r.Current is not null && r.Current.GetType() == typeof(WaitForSeconds))
                    {
                        if (i == 0)
                        {
                            yield return Core.Handler(entity, actionid);
                            i++;
                        }
                    }
                    yield return r.Current;
                }
            }
        }
        [HarmonyPatch(typeof(BattleControl), nameof(BattleControl.DoAction), MethodType.Enumerator)]
        public static class Method2_DoAction
        {
            [HarmonyTranspiler]
            static IEnumerable<CodeInstruction> Patch(IEnumerable<CodeInstruction> insts, ILGenerator iLGen)
            {
                var entity = insts.First(i => i.opcode == OpCodes.Ldfld && ((FieldInfo)i.operand).Name == "entity");
                var actionid = insts.First(i => i.opcode == OpCodes.Ldfld && ((FieldInfo)i.operand).Name == "actionid");
                var current = insts.First(i => i.opcode == OpCodes.Stfld && ((FieldInfo)i.operand).Name.EndsWith("current"));
                var codematch = new CodeMatcher(insts, iLGen);
                codematch.MatchForward(true,
                    new CodeMatch(OpCodes.Br),
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldnull),
                    new CodeMatch(current)
                ).Advance(-1).Set(OpCodes.Call, AccessTools.Method(typeof(Core), nameof(Core.Handler)))
                .Advance(-2).Set(OpCodes.Nop, null)
                .Advance(2).Insert(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(entity),
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(actionid)
                ).MatchForward(true,
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldc_I4_M1),
                    new CodeMatch(OpCodes.Stfld)
                ).Advance(1).Nopify(2);
                return codematch.InstructionEnumeration();
            }
        }
        [HarmonyPatch(typeof(BattleControl), nameof(BattleControl.DoAction), MethodType.Enumerator)]
        public static class Method3_DoAction
        {
            [HarmonyTranspiler]
            static IEnumerable<CodeInstruction> Patch(IEnumerable<CodeInstruction> insts, ILGenerator iLGen)
            {
                var entity = insts.First(i => i.opcode == OpCodes.Ldfld && ((FieldInfo)i.operand).Name == "entity");
                var actionid = insts.First(i => i.opcode == OpCodes.Ldfld && ((FieldInfo)i.operand).Name == "actionid");
                var codematcher = new CodeMatcher(insts, iLGen);
                codematcher.MatchForward(false,
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(i => i.opcode == OpCodes.Ldfld && ((FieldInfo)i.operand).Name == "actionid"),
                    new CodeMatch(OpCodes.Stloc_S)
                ).InjectYield(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(entity),
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(actionid),
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Core), nameof(Core.Handler)))
                );
                return codematcher.InstructionEnumeration();
            }
        }
    }
}
