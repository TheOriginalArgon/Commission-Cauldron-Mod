using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace CauldronMod
{
    public class JobDriver_StirPot : JobDriver
    {
        protected Building_FoodPot Pot => (Building_FoodPot)job.targetA.Thing;
        public override bool TryMakePreToilReservations(bool errorOnFailed) => pawn.Reserve(Pot, job, 1, -1, null, errorOnFailed);
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.FailOnBurningImmobile(TargetIndex.A);
            this.FailOn(() => Pot.Empty);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            yield return Toils_General.Wait(200).FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch)
                .WithProgressBarToilDelay(TargetIndex.A);
            // Reset only stirring; an unlit fire must not have its spoilage erased.
            yield return Toils_General.DoAtomic(() => Pot.GetComp<CompCauldron>().ResetStir());
        }
    }
}
