using HarmonyLib;
using System.Reflection.Emit;

namespace MooAPI
{
    public static class Utils
    {
        public static void Nopify(ref CodeMatcher codeMatcher, int instsnumber)
        {
            foreach (var _ in codeMatcher.InstructionsWithOffsets(0, instsnumber))
            {
                codeMatcher.SetAndAdvance(OpCodes.Nop, null);
            }
        }
    }
}
