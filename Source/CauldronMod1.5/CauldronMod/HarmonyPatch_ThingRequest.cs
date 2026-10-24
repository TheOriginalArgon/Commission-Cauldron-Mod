using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using HarmonyLib;
using System.Reflection.Emit;

namespace CauldronMod
{
    [HarmonyPatch]
    public class HarmonyPatch_ThingRequest
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ThingListGroupHelper), nameof(ThingListGroupHelper.Includes))]
        private static bool ThingRequest_Prefix(ref bool __result, ThingRequestGroup group, ThingDef def)
        {
            switch (group)
            {
                case ThingRequestGroup.FoodSource:
                    __result = def.IsNutritionGivingIngestible || def.thingClass == typeof(Building_NutrientPasteDispenser) || def.thingClass == typeof(Building_FoodPot);
                    return false;
                case ThingRequestGroup.FoodSourceNotPlantOrTree:
                    __result = (def.IsNutritionGivingIngestible && (def.ingestible.foodType & ~FoodTypeFlags.Plant & ~FoodTypeFlags.Tree) != FoodTypeFlags.None) || def.thingClass == typeof(Building_NutrientPasteDispenser) || def.thingClass == typeof(Building_FoodPot);
                    return false;
            }
            return true;
        }
    }
}
