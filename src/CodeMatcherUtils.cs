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
            while (true)
            {
                codematcher.MatchForward(false,
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldc_I4_M1),
                    new CodeMatch(i => i.opcode == OpCodes.Stfld && ((FieldInfo)i.operand).Name.EndsWith("_state")));
                if (codematcher.IsInvalid) break;
                labels.list.Add(codematcher.Labels[0]);
                codematcher.Advance(5);
            }
            return labels;
        }
        public static CodeMatcher InjectYield(this CodeMatcher codematcher, params CodeInstruction[] instructions)
        {
            var current = codematcher.Instructions().First(i => i.opcode == OpCodes.Stfld && ((FieldInfo)i.operand).Name.EndsWith("_current"));
            var state = codematcher.Instructions().First(i => i.opcode == OpCodes.Stfld && ((FieldInfo)i.operand).Name.EndsWith("_state"));
            codematcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
            .InsertAndAdvance(instructions)
            .InsertAndAdvance(current)
            .InsertAndAdvance(
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldc_I4_S, -127),
                state,
                new CodeInstruction(OpCodes.Ldc_I4_1),
                new CodeInstruction(OpCodes.Ret),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldc_I4_M1),
                state
            ).Advance(-3).CreateLabel(out var label);
            var labels = ExtractLabels(codematcher);
            codematcher.SetAt(labels.switchpos, new CodeInstruction(OpCodes.Switch, labels.list.ToArray()));
            MoveStates(ref codematcher, state);
            codematcher.Advance(3);
            return codematcher;
        }
        public static void MoveStates(ref CodeMatcher codematcher, CodeInstruction state)
        {
            var oldpos = codematcher.Pos;
            codematcher.MatchBack(false,
                new CodeMatch(OpCodes.Ldc_I4_S, -127)
            );
            var statepos = codematcher.Pos;
            codematcher.Advance(-2).MatchBack(false,
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(i => i.opcode != OpCodes.Ldc_I4_M1),
                new CodeMatch(i => i.opcode == OpCodes.Stfld && ((FieldInfo)i.operand).Name.EndsWith("_state"))
            ).Advance(1)
            .SetAt(statepos, GenerateState(codematcher.Instruction))
            .GoTo(oldpos);
            while (true)
            {
                codematcher.MatchForward(true,
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(i => i.opcode != OpCodes.Ldc_I4_M1),
                    new CodeMatch(state)
                );
                if (codematcher.IsInvalid) break;
                codematcher.Advance(-1)
                .SetInstruction(GenerateState(codematcher.Instruction))
                .Advance(3);
            }
            codematcher.GoTo(oldpos);
        }
        public static CodeInstruction GenerateState(CodeInstruction instruction)
        {
            switch (instruction.opcode.Name)
            {
                default:
                    var num = (int)char.GetNumericValue(instruction.opcode.Name[instruction.opcode.Name.Length - 1]);
                    if (num == 8)
                    {
                        return new CodeInstruction(OpCodes.Ldc_I4_S, 9);
                    }
                    return new CodeInstruction((OpCode)AccessTools.Field(typeof(OpCodes), $"Ldc_I4_{num + 1}").GetValue(null));
                case "ldc.i4.s":
                    if (instruction.operand == (object)127)
                    {
                        return new CodeInstruction(OpCodes.Ldc_I4, 128);
                    }
                    else
                    {
                        return new CodeInstruction(OpCodes.Ldc_I4_S, (instruction.operand as sbyte?) + 1);
                    }
                case "ldc.i4":
                    return new CodeInstruction(OpCodes.Ldc_I4, (instruction.operand as int?) + 1);
            }
        }
    }
}
