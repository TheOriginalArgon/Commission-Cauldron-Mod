using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace CauldronMod
{
    public class JobDriver_StirPot : JobDriver
    {
        protected Building_FoodPot Pot => (Building_FoodPot)job.GetTarget(TargetIndex.A).Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(Pot, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.FailOnBurningImmobile(TargetIndex.A);
            base.AddEndCondition(delegate
            {
                if (Pot.NutritionSpaceLeft > 0f)
                {
                    return JobCondition.Ongoing;
                }
                return JobCondition.Ongoing;
            });
            yield return Toils_General.DoAtomic(delegate
            {
                job.count = (int)Pot.NutritionSpaceLeft;
            });
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            yield return Toils_General.Wait(200, TargetIndex.None).FailOnDestroyedNullOrForbidden(TargetIndex.A).FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch).WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
            Toil toil = ToilMaker.MakeToil("MakeNewToils");
            toil.initAction = () =>
            {
                Pot.TryGetComp<CompCauldron>().Reset();
            };
            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return toil;
            yield break;
        }
    }
}
