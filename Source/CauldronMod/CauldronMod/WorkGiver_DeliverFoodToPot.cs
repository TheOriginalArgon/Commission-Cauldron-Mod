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
    public class WorkGiver_DeliverFoodToPot : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest
        {
            get
            {
                return ThingRequest.ForDef(DefDatabase<ThingDef>.GetNamed("CM_Cauldron"));
            }
        }

        public override PathEndMode PathEndMode
        {
            get
            {
                return PathEndMode.Touch;
            }
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Building_FoodPot building_FoodPot = t as Building_FoodPot;
            if (building_FoodPot == null || building_FoodPot.NutritionSpaceLeft <= 0)
            {
                return false;
            }
            if (t.IsForbidden(pawn) || !pawn.CanReserve(t, 1, -1, null, forced))
            {
                return false;
            }
            if (pawn.Map.designationManager.DesignationOn(t, DesignationDefOf.Deconstruct) != null)
            {
                return false;
            }
            if (FindFood(pawn, building_FoodPot) == null)
            {
                JobFailReason.Is("Cannot find food", null);
                return false;
            }
            return !t.IsBurning();
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Building_FoodPot building_FoodPot = (Building_FoodPot)t;
            Thing food = FindFood(pawn, building_FoodPot);
            return JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed("CM_TakeFoodToPot"), t, food);
        }

        private Thing FindFood(Pawn pawn, Building_FoodPot pot)
        {
            Predicate<Thing> predicate = (Thing x) => !x.IsForbidden(pawn) && pawn.CanReserve(x, 1, -1, null, false) && (x.HasThingCategory(ThingCategoryDefOf.MeatRaw) || x.HasThingCategory(ThingCategoryDefOf.PlantFoodRaw));
            return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.FoodSourceNotPlantOrTree), PathEndMode.ClosestTouch, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false, false, false), 9999f, predicate, null, 0, -1, false, RegionType.Set_Passable, false);
        }
    }
}
