using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace CauldronMod
{
    // Keep the game's normal food selection for everything except a food pot.
    [HarmonyPatch]
    public static class FoodPotFoodValidatorPatch
    {
        private static readonly Func<Thing, Pawn, Pawn, bool, bool> SociallyProper =
            AccessTools.MethodDelegate<Func<Thing, Pawn, Pawn, bool, bool>>(
                AccessTools.Method(typeof(FoodUtility), "IsFoodSourceOnMapSociallyProper"));

        public static MethodBase TargetMethod()
        {
            // Identify the actual food validator by its dispenser power check, rather
            // than a compiler-generated method number (which changes between releases).
            FieldInfo powerField = AccessTools.Field(typeof(Building_NutrientPasteDispenser), "powerComp");
            return typeof(FoodUtility).GetNestedTypes(AccessTools.all)
                .SelectMany(AccessTools.GetDeclaredMethods)
                .Single(method => method.Name.Contains("BestFoodSourceOnMap") &&
                    method.ReturnType == typeof(bool) && method.GetParameters().Length == 1 &&
                    method.GetParameters()[0].ParameterType == typeof(Thing) &&
                    PatchProcessor.GetOriginalInstructions(method).Any(code => code.LoadsField(powerField)));
        }

        [HarmonyPrefix]
        public static bool Prefix(Thing t, ref bool __result, Pawn ___getter, Pawn ___eater,
            bool ___getterCanManipulate, bool ___allowDispenserFull, bool ___allowForbidden,
            bool ___allowSociallyImproper, bool ___allowVenerated,
            FoodPreferability ___minPref, FoodPreferability ___maxPref)
        {
            if (!(t is Building_FoodPot pot)) return true;
            // Use the invocation's captured fields, never a global copy of the last
            // food-search arguments. Nested searches and animals stay independent.
            __result = false;
            if (!___allowDispenserFull || !___getterCanManipulate || !pot.CanDispenseMealNow ||
                (t.Faction != ___getter.Faction && t.Faction != ___getter.HostFaction) ||
                (!___allowForbidden && t.IsForbidden(___getter)) ||
                !t.InteractionCell.Standable(t.Map)) return false;
            ThingDef mealDef = pot.DispensableDef;
            __result = mealDef.ingestible.preferability >= ___minPref &&
                mealDef.ingestible.preferability <= ___maxPref &&
                ___eater.WillEat(mealDef, ___getter, true, ___allowVenerated) &&
                SociallyProper(t, ___getter, ___eater, ___allowSociallyImproper) &&
                ___getter.Map.reachability.CanReachNonLocal(___getter.Position,
                    new TargetInfo(t.InteractionCell, t.Map), PathEndMode.OnCell,
                    TraverseParms.For(___getter, Danger.Some)) &&
                (!___getter.roping.IsRoped || t.InteractionCell.InHorDistOf(___getter.roping.RopedTo.Cell, 8f));
            return false;
        }
    }

    [HarmonyPatch(typeof(Building_NutrientPasteDispenser), nameof(Building_NutrientPasteDispenser.CanDispenseNow), MethodType.Getter)]
    public static class FoodPotDispensePatch
    {
        [HarmonyPrefix]
        public static bool Prefix(Building_NutrientPasteDispenser __instance, ref bool __result)
        {
            if (!(__instance is Building_FoodPot pot)) return true;
            __result = pot.CanDispenseMealNow;
            return false;
        }
    }

    [HarmonyPatch(typeof(ThingListGroupHelper), nameof(ThingListGroupHelper.Includes))]
    public static class FoodPotThingRequestPatch
    {
        [HarmonyPostfix]
        public static void Postfix(ThingRequestGroup group, ThingDef def, ref bool __result)
        {
            // Add pots without replacing the game's rules or other mods' results.
            if ((group == ThingRequestGroup.FoodSource || group == ThingRequestGroup.FoodSourceNotPlantOrTree) &&
                typeof(Building_FoodPot).IsAssignableFrom(def.thingClass)) __result = true;
        }
    }
}
