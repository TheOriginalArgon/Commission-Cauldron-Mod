using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace CauldronMod
{
    // Use native dispenser jobs for eating and patient feeding, but store nutrition
    // directly: the pot needs neither electricity nor a hopper.
    public class Building_FoodPot : Building_NutrientPasteDispenser, IStoreSettingsParent
    {
        public const float Capacity = 25f;
        public const float MealCost = 0.5f;
        private float nutritionCount;
        private Dictionary<ThingDef, float> ingredients = new Dictionary<ThingDef, float>();
        private List<ThingDef> ingredientDefs;
        private List<float> ingredientValues;
        private StorageSettings storageSettings;
        private ThingDef nextMealDef;
        private static Material potFull;

        public bool CanDispenseMealNow => nutritionCount + 0.00001f >= MealCost;
        public float NutritionSpaceLeft => Mathf.Max(0f, Capacity - nutritionCount);
        public bool Empty => nutritionCount <= 0f;
        public bool StorageTabVisible => true;

        // Decide once, so food restrictions refer to the meal actually served.
        public override ThingDef DispensableDef => nextMealDef ?? (nextMealDef = RollMealDef());

        public override void PostMake()
        {
            base.PostMake();
            EnsureStorageSettings();
        }

        private void EnsureStorageSettings()
        {
            if (storageSettings != null) return;
            storageSettings = new StorageSettings(this);
            storageSettings.CopyFrom(def.building.defaultStorageSettings);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref nutritionCount, "nutritionCount", 0f);
            Scribe_Collections.Look(ref ingredients, "ingredients", LookMode.Def, LookMode.Value,
                ref ingredientDefs, ref ingredientValues);
            Scribe_Deep.Look(ref storageSettings, "storageSettings", this);
            Scribe_Defs.Look(ref nextMealDef, "nextMealDef");
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                EnsureStorageSettings();
                nutritionCount = Mathf.Clamp(nutritionCount, 0f, Capacity);
                ingredients = ingredients ?? new Dictionary<ThingDef, float>();
                // Repair mismatched ingredient totals from older saves without losing food.
                foreach (ThingDef key in ingredients.Keys.ToArray())
                    if (ingredients[key] <= 0f) ingredients.Remove(key);
                float total = ingredients.Values.Sum();
                if (total > 0f)
                    foreach (ThingDef key in ingredients.Keys.ToArray())
                        ingredients[key] *= nutritionCount / total;
                if (Empty) ingredients.Clear();
            }
        }

        protected override void ReceiveCompSignal(string signal)
        {
            base.ReceiveCompSignal(signal);
            if (signal != "Ruined") return;
            nutritionCount = 0f;
            ingredients.Clear();
            nextMealDef = null;
            if (Spawned)
            {
                Messages.Message("CM_MessageRuined".Translate(), this, MessageTypeDefOf.NegativeEvent);
                DirtyMapMesh(Map);
            }
        }

        public int FoodCountToAccept(Thing food)
        {
            if (food == null || food.Destroyed || !food.def.IsNutritionGivingIngestible ||
                !GetStoreSettings().AllowedToAccept(food)) return 0;
            float perItem = food.GetStatValue(StatDefOf.Nutrition);
            // Consume whole items that fit; never destroy the rest of a delivered stack.
            return perItem > 0f
                ? Mathf.Min(food.stackCount, Mathf.FloorToInt((NutritionSpaceLeft + 0.00001f) / perItem)) : 0;
        }

        public void AddNutrition(Thing food)
        {
            int count = FoodCountToAccept(food);
            if (count <= 0) return;
            float amount = count * food.GetStatValue(StatDefOf.Nutrition);
            ThingDef ingredient = food.def;
            nutritionCount = Mathf.Min(Capacity, nutritionCount + amount);
            ingredients.TryGetValue(ingredient, out float existing);
            ingredients[ingredient] = existing + amount;
            food.SplitOff(count).Destroy(DestroyMode.Vanish);
            if (Spawned) DirtyMapMesh(Map);
        }

        private static ThingDef RollMealDef()
        {
            float roll = Rand.Value;
            if (roll < 0.5f) return ThingDefOf.MealNutrientPaste;
            if (roll < 0.8f) return ThingDefOf.MealSimple;
            if (roll < 0.95f) return ThingDefOf.MealFine;
            return DefDatabase<ThingDef>.GetNamed("MealLavish");
        }

        public Thing TryDispenseMeal() => TryDispenseFood();

        public override Thing TryDispenseFood()
        {
            if (!CanDispenseMealNow) return null;
            Thing meal = ThingMaker.MakeThing(DispensableDef);
            CompIngredients mealIngredients = meal.TryGetComp<CompIngredients>();
            // Stew is mixed: every contributing ingredient goes on each meal, and
            // each ingredient's nutrition is reduced proportionally.
            float remaining = Mathf.Max(0f, nutritionCount - MealCost);
            if (remaining < 0.00001f) remaining = 0f;
            foreach (ThingDef ingredient in ingredients.Keys.ToArray())
            {
                mealIngredients?.RegisterIngredient(ingredient);
                ingredients[ingredient] *= remaining / nutritionCount;
            }
            nutritionCount = remaining;
            if (Empty)
            {
                ingredients.Clear();
                GetComp<CompCauldron>()?.Reset();
            }
            nextMealDef = null;
            if (Spawned) DirtyMapMesh(Map);
            return meal;
        }

        public override bool HasEnoughFeedstockInHoppers() => CanDispenseMealNow;
        public override Thing FindFeedInAnyHopper() => null;
        public override Building AdjacentReachableHopper(Pawn reacher) => null;

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                // Hide the dispenser's hopper shortcut; pots do not use hoppers.
                if (gizmo is Designator_Build build && build.PlacingDef == ThingDefOf.Hopper) continue;
                yield return gizmo;
            }
        }

        public override string GetInspectString() => base.GetInspectString().TrimEndNewlines() + "\n" +
            "CM_Nutrition".Translate(nutritionCount.ToString("0.##"), Capacity.ToString("0"));

        public override void Print(SectionLayer layer)
        {
            base.Print(layer);
            if (!CanDispenseMealNow) return;
            if (potFull == null)
                potFull = MaterialPool.MatFrom("Things/Building/CM_Pot/CM_Pot_Full", ShaderDatabase.Transparent);
            Printer_Plane.PrintPlane(layer, DrawPos, DrawSize, potFull);
        }

        public StorageSettings GetStoreSettings() { EnsureStorageSettings(); return storageSettings; }
        public StorageSettings GetParentStoreSettings() => def.building.fixedStorageSettings;
        public void Notify_SettingsChanged() { }
    }
}


