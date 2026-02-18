using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using RimWorld;
using Verse;
using Verse.AI;
using HarmonyLib;

namespace CauldronMod
{
    [HarmonyPatch]
    public class HarmonyPatch_FoodUtility
    {
        public static object[] ___argParms;
        [HarmonyPrefix]
        [HarmonyPatch(typeof(FoodUtility), "BestFoodSourceOnMap_NewTemp")]
        private static bool Prefix_ParamScrapper(object[] __args)
        {
            ___argParms = new object[__args.Length];
            for (int i = 0; i < __args.Length; i++)
            {
                ___argParms[i] = __args[i];
            }
            return true;
        }
    }

    [HarmonyPatch]
    public class HarmonyPatch_Delegate
    {
        public static MethodBase TargetMethod { [HarmonyTargetMethod] get; } = typeof(FoodUtility).GetNestedTypes(AccessTools.all).SelectMany(AccessTools.GetDeclaredMethods).First(mi => mi.Name.Contains("BestFoodSourceOnMap_NewTemp"));

        // This method needs a way of accessing the arguments of its container, as it messes with animals.
        [HarmonyPrefix]
        private static bool Prefix_Delegate(ref bool __result, Thing t)
        {
            //Log.Warning("We're patching the delegate!");
            foreach (var a in HarmonyPatch_FoodUtility.___argParms)
            {
                Log.Message(a == null ? "null" : a.ToString());
            }
            var Method_FoodUtility = Traverse.Create(typeof(FoodUtility));

            Pawn getter = HarmonyPatch_FoodUtility.___argParms[0] as Pawn;
            Pawn eater = HarmonyPatch_FoodUtility.___argParms[1] as Pawn;
            bool allowVenerated = (bool)HarmonyPatch_FoodUtility.___argParms[18];
            bool allowForbidden = (bool)HarmonyPatch_FoodUtility.___argParms[10];
            bool allowSociallyImproper = (bool)HarmonyPatch_FoodUtility.___argParms[11];
            if (getter != null && eater != null)
            {
                Log.Warning($"==> {getter.Name} is getting food to {eater.Name}");
            }

            Building_FoodPot building_FoodPot = t as Building_FoodPot;
            Log.Warning(t == null ? "Source is null" : "Source is " + t.ToString());
            Log.Warning(building_FoodPot == null ? "Food Pot is null" : "Food pot is " + building_FoodPot.ToString()); 
            if (building_FoodPot != null)
            {
                if (!eater.WillEat_NewTemp(ThingDefOf.MealNutrientPaste, getter, true, allowVenerated) || !building_FoodPot.CanDispenseMealNow && (t.Faction != getter.Faction && t.Faction != getter.HostFaction) || (!allowForbidden && t.IsForbidden(getter)) || !Method_FoodUtility.Method("IsFoodSourceOnMapSociallyProper", new object[] { t, getter, eater, allowSociallyImproper }).GetValue<bool>() || !getter.Map.reachability.CanReachNonLocal(getter.Position, new TargetInfo(t), PathEndMode.Touch, TraverseParms.For(getter, Danger.Some, TraverseMode.ByPawn, false, false, false)))
                {
                    __result = false;
                    return false;
                }
                __result = true;
                return false;
            }
            return true;
        }

    }
}
