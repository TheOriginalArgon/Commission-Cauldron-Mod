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
    public class HarmonyPatch_GetFood
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(FoodUtility), nameof(FoodUtility.GetFinalIngestibleDef))]
        private static bool GetPotIngestible(ref ThingDef __result, Thing foodSource, bool harvest)
        {
            Building_FoodPot building_FoodPot = foodSource as Building_FoodPot;
            if (building_FoodPot != null)
            {
                //Log.Warning($"Getting to the meal! {foodSource.def.defName}");
                __result = building_FoodPot.DispensableDef; // Not good.
                return false;
            }
            return true;
        }
    }
}
