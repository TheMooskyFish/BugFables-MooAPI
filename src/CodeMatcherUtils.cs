using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;


namespace MooAPI
{
    public static class CodeMatcherUtils
    {
        public static CodeMatcher Nopify(this CodeMatcher codeMatcher, int instsnumber)
        {
            foreach (var _ in codeMatcher.InstructionsWithOffsets(0, instsnumber))
            {
                codeMatcher.SetAndAdvance(OpCodes.Nop, null);
            }
            return codeMatcher;
        }
        public static CodeMatcher GoTo(this CodeMatcher codeMatcher, int pos)
        {
            AccessTools.PropertySetter(typeof(CodeMatcher), "Pos").Invoke(codeMatcher, [pos]);
            return codeMatcher;
        }
        public static CodeMatcher SetAt(this CodeMatcher codeMatcher, int pos, CodeInstruction inst)
        {
            var oldpos = codeMatcher.Pos;
            codeMatcher.GoTo(pos)
            .SetInstruction(inst)
            .GoTo(oldpos);
            return codeMatcher;
        }
        public class InstructionLabels()
        {
            public int switchpos;
            public List<Label> list = [];
        }
        public static InstructionLabels ExtractLabels(CodeMatcher codematcher)
        {
            codematcher = codematcher.Clone();
            InstructionLabels labels = new();
            codematcher.Start().MatchForward(false, new CodeMatch(OpCodes.Switch));
            labels.switchpos = codematcher.Pos;
            labels.list = (codematcher.Operand as Label[]).ToList();
            return labels;
        }
        public static CodeMatcher InjectYield(this CodeMatcher codematcher, params CodeInstruction[] instructions)
        {
            var current = codematcher.Instructions().First(i => i.opcode == OpCodes.Stfld && ((FieldInfo)i.operand).Name.EndsWith("_current"));
            var state = codematcher.Instructions().First(i => i.opcode == OpCodes.Stfld && ((FieldInfo)i.operand).Name.EndsWith("_state"));
            var labels = ExtractLabels(codematcher);
            codematcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
            .InsertAndAdvance(instructions)
            .InsertAndAdvance(current)
            .InsertAndAdvance(
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldc_I4, labels.list.Count),
                state,
                new CodeInstruction(OpCodes.Ldc_I4_1),
                new CodeInstruction(OpCodes.Ret),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldc_I4_M1),
                state
            ).Advance(-3).CreateLabel(out var label);
            labels.list.Add(label);
            codematcher.SetAt(labels.switchpos, new CodeInstruction(OpCodes.Switch, labels.list.ToArray()));
            codematcher.Advance(3);
            return codematcher;
        }
    }
}
