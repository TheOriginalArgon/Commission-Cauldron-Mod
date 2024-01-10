using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace CauldronMod
{
    public class JobDriver_TakeFoodToPot : JobDriver
    {
        protected Building_FoodPot Pot
        {
            get
            {
                return (Building_FoodPot)job.GetTarget(TargetIndex.A).Thing;
            }
        }

        protected Thing Food
        {
            get
            {
                return job.GetTarget(TargetIndex.B).Thing;
            }
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(Pot, job, 1, -1, null, errorOnFailed) && pawn.Reserve(Food, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.FailOnBurningImmobile(TargetIndex.A);
            AddEndCondition(delegate
            {
                if (Pot.NutritionSpaceLeft > 0)
                {
                    return JobCondition.Ongoing;
                }
                return JobCondition.Succeeded;
            });
            yield return Toils_General.DoAtomic(delegate
            {
                job.count = (int)(Pot.NutritionSpaceLeft / job.GetTarget(TargetIndex.B).Thing.GetStatValue(StatDefOf.Nutrition));
            });
            Toil reserveFood = Toils_Reserve.Reserve(TargetIndex.B, 1, -1, null);
            yield return reserveFood;
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.B).FailOnSomeonePhysicallyInteracting(TargetIndex.B);
            yield return Toils_Haul.StartCarryThing(TargetIndex.B, false, true, false, true).FailOnDestroyedNullOrForbidden(TargetIndex.B);
            yield return Toils_Haul.CheckForGetOpportunityDuplicate(reserveFood, TargetIndex.B, TargetIndex.None, true, null);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            yield return Toils_General.Wait(200, TargetIndex.None).FailOnDestroyedNullOrForbidden(TargetIndex.B).FailOnDestroyedNullOrForbidden(TargetIndex.A).FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch).WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
            Toil toil = ToilMaker.MakeToil("MakeNewToils");
            toil.initAction = delegate ()
            {
                Pot.AddNutrition(Food);
            };
            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return toil;
            yield break;
        }
    }
}
