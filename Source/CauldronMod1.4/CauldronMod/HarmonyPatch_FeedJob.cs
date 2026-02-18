using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace CauldronMod
{
    [HarmonyPatch]
    public class HarmonyPatch_FeedJob
    {
        //[HarmonyTranspiler]
        //[HarmonyPatch(typeof(JobDriver_FoodFeedPatient), nameof(JobDriver_FoodFeedPatient.TryMakePreToilReservations))]
        //private static IEnumerable<CodeInstruction> Transpiler_FeedPatient(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        //{
        //    List<CodeInstruction> code = instructions.ToList();
        //    Label jmpLabel = il.DefineLabel();

        //    for (int i = 0; i < code.Count; i++)
        //    {
        //        if (code[i].opcode == OpCodes.Ldc_I4_1 && code[i + 1].opcode == OpCodes.Ret)
        //        {
        //            code[i].labels.Add(jmpLabel);
        //        }
        //        yield return code[i];
        //        //Log.Message(code[i].ToString());
        //        if (code[i].opcode == OpCodes.Brtrue_S && code[i - 1].opcode == OpCodes.Isinst)
        //        {
        //            CodeInstruction c1 = new CodeInstruction(OpCodes.Ldarg_0);
        //            CodeInstruction c2 = new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(JobDriver), "TargetThingA"));
        //            CodeInstruction c3 = new CodeInstruction(OpCodes.Isinst, typeof(Building_FoodPot));
        //            CodeInstruction c4 = new CodeInstruction(OpCodes.Brtrue_S, jmpLabel);
        //            //Log.Message(c1.ToString());
        //            //Log.Message(c2.ToString());
        //            //Log.Message(c3.ToString());
        //            //Log.Message(c4.ToString());
        //            yield return c1;
        //            yield return c2;
        //            yield return c3;
        //            yield return c4;
        //        }
        //    }

        //    yield break;
        //}

        //[HarmonyPostfix]
        //[HarmonyPatch(typeof(JobDriver_Feed), "MakeNewToils")]
        //private static IEnumerable<Toil> PostFix_Toils(IEnumerable<Toil> values, JobDriver_Feed __instance)
        //{
        //    var __targetThingA = AccessTools.Property(typeof(JobDriver), "TargetThingA");
        //    if (__targetThingA != null && __targetThingA.GetValue(__instance) is Building_FoodPot)
        //    {
        //        List<Toil> toils = values.ToList();

        //        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch, false).FailOnForbidden(TargetIndex.A);
        //        yield return Toils_Pot.TakeMealFromPot(TargetIndex.A, __instance.pawn);
        //        for (int i = 2; i < toils.Count; i++)
        //        {
        //            yield return toils[i];
        //        }
        //    }
        //    yield break;
        //}

        //[HarmonyPostfix]
        //[HarmonyPatch(typeof(JobDriver_FoodFeedPatient), "MakeNewToils")]
        //private static IEnumerable<Toil> PostFix_Toils_Patient(IEnumerable<Toil> values, JobDriver_FoodFeedPatient __instance)
        //{
        //    var __targetThingA = AccessTools.Property(typeof(JobDriver), "TargetThingA");
        //    List<Toil> toils = values.ToList();

        //    Toil goToFoodPot = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch, false).FailOnForbidden(TargetIndex.A);
        //    for (int i = 0; i < toils.Count; i++)
        //    {
        //        yield return toils[i];
        //        if (i == 2)
        //        {
        //            yield return Toils_Jump.JumpIf(goToFoodPot, () => __targetThingA.GetValue(__instance) is Building_FoodPot);
        //        }
        //        if (i == 12)
        //        {
        //            yield return goToFoodPot;
        //            yield return Toils_Pot.TakeMealFromPot(TargetIndex.A, __instance.pawn);
        //        }
        //    }
        //    yield break;
        //}

        //// Deliver food.
        //[HarmonyTranspiler]
        //[HarmonyPatch(typeof(JobDriver_FoodDeliver), "Notify_Starting")]
        //private static IEnumerable<CodeInstruction> Transpiler_FoodDeliver(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        //{
        //    List<CodeInstruction> code = instructions.ToList();
        //    Label jmpLabel_ld = il.DefineLabel();
        //    Label jmpLabel_stf = il.DefineLabel();

        //    for (int i = 0; i < code.Count; i++)
        //    {
        //        yield return code[i];
        //        if (code[i].opcode == OpCodes.Isinst)
        //        {
        //            yield return new CodeInstruction(OpCodes.Brtrue_S, jmpLabel_ld);
        //            yield return new CodeInstruction(OpCodes.Ldarg_0);
        //            yield return new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(JobDriver), "TargetThingA"));
        //            yield return new CodeInstruction(OpCodes.Isinst, typeof(Building_FoodPot));
        //        }
        //        if (code[i].opcode == OpCodes.Cgt_Un)
        //        {
        //            code[i + 1].labels.Add(jmpLabel_stf);
        //            yield return new CodeInstruction(OpCodes.Br_S, jmpLabel_stf);
        //            CodeInstruction ld = new CodeInstruction(OpCodes.Ldc_I4_1);
        //            ld.labels.Add(jmpLabel_ld);
        //            yield return ld;
        //        }
        //    }

        //    yield break;
        //}

        // Dispenser Patch.
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Toils_Ingest), nameof(Toils_Ingest.TakeMealFromDispenser))]
        private static bool Prefix_Dispenser(ref Toil __result, TargetIndex ind, Pawn eater)
        {
            if (!(eater.jobs.curJob.GetTarget(ind).Thing is Building_FoodPot))
            {
                return true;
            }
            Toil toil = ToilMaker.MakeToil("TakeMealFromPot");
            toil.initAction = delegate ()
            {
                Pawn actor = toil.actor;
                Thing thing = ((Building_FoodPot)actor.jobs.curJob.GetTarget(ind).Thing).TryDispenseMeal();
                if (thing == null)
                {
                    actor.jobs.curDriver.EndJobWith(JobCondition.Incompletable);
                    return;
                }
                actor.carryTracker.TryStartCarry(thing);
                actor.CurJob.SetTarget(ind, actor.carryTracker.CarriedThing);
            };
            toil.FailOnCannotTouch(ind, PathEndMode.Touch);
            toil.defaultCompleteMode = ToilCompleteMode.Delay;
            toil.defaultDuration = 175; // Hardcoded; shouldn't be much of an inconvenience. In any case, it can be replaced with a value in the thing class.
            __result = toil;
            return false;
        }
    }
}
