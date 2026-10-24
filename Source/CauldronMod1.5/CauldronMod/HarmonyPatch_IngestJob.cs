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

        //[HarmonyPrefix]
        //[HarmonyPatch(typeof(JobDriver_Ingest), "PrepareToIngestToils")]
        //private static bool PrefixToils(ref IEnumerable<Toil> __result, JobDriver_Ingest __instance, Toil chewToil)
        //{
        //    if (__instance.job.GetTarget(TargetIndex.A).Thing is Building_FoodPot)
        //    {
        //        __result = PrepareToIngestToils_Pot(__instance.pawn);
        //        //Log.Warning($"The prefix was executed for {__instance.pawn.Name} and {__instance.job.GetTarget(TargetIndex.A).Thing.def.label}");
        //        return false;
        //    }
        //    return true;
        //}

        //[HarmonyTranspiler]
        //[HarmonyPatch(typeof(JobDriver_Ingest), "MakeNewToils", MethodType.Enumerator)]
        //private static IEnumerable<CodeInstruction> Transpiler_Check(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        //{
        //    List<CodeInstruction> code = instructions.ToList();

        //    Label jmpLbl = il.DefineLabel();
        //    for (int i = 0; i < code.Count; i++)
        //    {
        //        yield return code[i];
        //        if (i > 2)
        //        {
        //            if (code[i].LoadsField(AccessTools.Field(typeof(JobDriver), "pawn")) && code[i + 1].opcode == OpCodes.Ldloc_2)
        //            {
        //                code[i - 2].labels.Add(jmpLbl);
        //            }
        //            if (code[i - 2].LoadsField(AccessTools.Field(typeof(JobDriver_Ingest), "usingNutrientPasteDispenser")) && code[i - 1].opcode == OpCodes.Brtrue_S)
        //            {
        //                yield return new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(JobDriver_Ingest), "IngestibleSource"));
        //                yield return new CodeInstruction(OpCodes.Isinst, typeof(Building_FoodPot));
        //                yield return new CodeInstruction(OpCodes.Brtrue_S, jmpLbl);
        //                yield return new CodeInstruction(OpCodes.Ldloc_2);
        //            }
        //        }
        //    }
        //    yield break;
        //}

        //private static IEnumerable<Toil> PrepareToIngestToils_Pot(Pawn p)
        //{
        //    //Log.Warning("Attempted to start toils of going to the cauldron!");
        //    yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).FailOnDespawnedNullOrForbidden(TargetIndex.A);
        //    yield return Toils_Pot.TakeMealFromPot(TargetIndex.A, p);
        //    yield return Toils_Ingest.CarryIngestibleToChewSpot(p, TargetIndex.A).FailOnDestroyedNullOrForbidden(TargetIndex.A);
        //    yield return Toils_Ingest.FindAdjacentEatSurface(TargetIndex.B, TargetIndex.A);
        //    yield break;
        //}

    }

    //public class Toils_Pot
    //{
    //    public static Toil TakeMealFromPot(TargetIndex ind, Pawn eater)
    //    {
    //        Toil toil = ToilMaker.MakeToil("TakeMealFromPot");
    //        toil.initAction = delegate ()
    //        {
    //            Pawn actor = toil.actor;
    //            Thing thing = ((Building_FoodPot)actor.jobs.curJob.GetTarget(ind).Thing).TryDispenseMeal();
    //            if (thing == null)
    //            {
    //                actor.jobs.curDriver.EndJobWith(JobCondition.Incompletable);
    //                return;
    //            }
    //            actor.carryTracker.TryStartCarry(thing);
    //            actor.CurJob.SetTarget(ind, actor.carryTracker.CarriedThing);
    //        };
    //        toil.FailOnCannotTouch(ind, PathEndMode.Touch);
    //        toil.defaultCompleteMode = ToilCompleteMode.Delay;
    //        toil.defaultDuration = 175; // Hardcoded; shouldn't be much of an inconvenience. In any case, it can be replaced with a value in the thing class.
    //        return toil;
    //    }
    //}
}
