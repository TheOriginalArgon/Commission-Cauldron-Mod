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
    public class WorkGiver_StirPot : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(DefDatabase<ThingDef>.GetNamed("CM_Cauldron"));
        public override PathEndMode PathEndMode => PathEndMode.Touch;

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Building_FoodPot building_FoodPot = t as Building_FoodPot;

            if (building_FoodPot == null || building_FoodPot.NutritionSpaceLeft <= 0f) { return false; }
            if (t.IsForbidden(pawn) || !pawn.CanReserve(t, 1, -1, null, forced)) { return false; }
            if (pawn.Map.designationManager.DesignationOn(t, DesignationDefOf.Deconstruct) != null) {  return false; }
            return !t.IsBurning() && t.TryGetComp<CompCauldron>().NeedsStirringNow;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Building_FoodPot building_FoodPot = (Building_FoodPot)t;
            return JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed("CM_StirPot"), building_FoodPot);
        }
    }
}
