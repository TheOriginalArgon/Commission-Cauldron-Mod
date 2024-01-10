using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace CauldronMod
{
    [HarmonyPatch]
    public class HarmonyPatch_PrepareToIngestToils
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(JobDriver_Ingest), "PrepareToIngestToils")]
        private static bool PrefixToils(ref IEnumerable<Toil> __result, JobDriver_Ingest __instance, Toil chewToil)
        {
            if (__instance.job.GetTarget(TargetIndex.A).Thing is Building_FoodPot)
            {
                __result = PrepareToIngestToils_Pot(__instance.pawn);
                return false;
            }
            return true;
        }


        private static IEnumerable<Toil> PrepareToIngestToils_Pot(Pawn p)
        {
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).FailOnDespawnedNullOrForbidden(TargetIndex.A);
            yield return TakeMealFromPot(TargetIndex.A, p);
            yield return Toils_Ingest.CarryIngestibleToChewSpot(p, TargetIndex.A).FailOnDespawnedNullOrForbidden(TargetIndex.A);
            yield return Toils_Ingest.FindAdjacentEatSurface(TargetIndex.B, TargetIndex.A);
            yield break;
        }

        private static Toil TakeMealFromPot(TargetIndex ind, Pawn eater)
        {
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
            toil.defaultDuration = 1; // Change to something from the pot building class???
            return toil;
        }
    }
}
