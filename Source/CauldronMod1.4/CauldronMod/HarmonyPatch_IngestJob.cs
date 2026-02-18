using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace CauldronMod
{
    [HarmonyPatch]
    public class HarmonyPatch_IngestJob
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(JobDriver_Ingest), "Notify_Starting")]
        private static IEnumerable<CodeInstruction> Transpiler_Notify_Starting(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            List<CodeInstruction> code = instructions.ToList();
            Label jmpLabel_ld = il.DefineLabel();
            Label jmpLabel_stf = il.DefineLabel();

            for (int i = 0; i < code.Count; i++)
            {
                yield return code[i];
                if (code[i].opcode == OpCodes.Isinst)
                {
                    yield return new CodeInstruction(OpCodes.Brtrue_S, jmpLabel_ld);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(JobDriver_Ingest), "IngestibleSource"));
                    yield return new CodeInstruction(OpCodes.Isinst, typeof(Building_FoodPot));
                }
                if (code[i].opcode == OpCodes.Cgt_Un)
                {
                    code[i + 1].labels.Add(jmpLabel_stf);
                    yield return new CodeInstruction(OpCodes.Br_S, jmpLabel_stf);
                    CodeInstruction ld = new CodeInstruction(OpCodes.Ldc_I4_1);
                    ld.labels.Add(jmpLabel_ld);
                    yield return ld;
                }
            }

            yield break;
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(JobDriver_Ingest), "TryMakePreToilReservations")]
        private static IEnumerable<CodeInstruction> Transpiler_Reservations(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = instructions.ToList();
            bool flag = false;

            for (int i = 0; i < code.Count; i++)
            {
                yield return code[i];
                if (code[i].opcode == OpCodes.Brfalse_S && !flag)
                {
                    flag = true;
                }
                if (code[i].opcode == OpCodes.Brtrue_S && flag)
                {
                    yield return code[i - 3];
                    yield return code[i - 2];
                    yield return new CodeInstruction(OpCodes.Isinst, typeof(Building_FoodPot));
                    yield return code[i];
                    flag = false;
                }
            }

            yield break;
        }

        // Debug
        [HarmonyPostfix]
        [HarmonyPatch(typeof(JobDriver_Ingest), "PrepareToIngestToils")]
        private static void Postfix_Debug(bool ___usingNutrientPasteDispenser)
        {
            Log.Message(___usingNutrientPasteDispenser.ToString());
        }
    }
}
