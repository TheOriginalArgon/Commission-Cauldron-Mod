using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using Verse.AI;
using HarmonyLib;

namespace CauldronMod
{
    [HarmonyPatch]
    public class HarmonyPatch_FeedPatientJob
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(JobDriver_FoodFeedPatient), "MakeNewToils")]
        private static IEnumerable<Toil> Postfix_FeedPatient(IEnumerable<Toil> values, JobDriver_FoodFeedPatient __instance)
        {
            List<Toil> toils = values.ToList();
            Toil goToCauldron = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).FailOnForbidden(TargetIndex.A);

            for (int i = 0; i < toils.Count; i++)
            {
                if (i == 2)
                {
                    yield return Toils_Jump.JumpIf(goToCauldron, () => __instance.job.targetA.Thing is Building_FoodPot);
                }
                if (i == 11)
                {
                    yield return goToCauldron;
                }
                yield return toils[i];
            }
        }
    }
}
