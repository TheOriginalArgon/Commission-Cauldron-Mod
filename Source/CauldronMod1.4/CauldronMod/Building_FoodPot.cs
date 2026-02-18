using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace CauldronMod
{
    [StaticConstructorOnStartup]
    public class Building_FoodPot : Building, IStoreSettingsParent
    {
        private float nutritionCount;

        private Dictionary<ThingDef, float> ingredients = new Dictionary<ThingDef, float>();

        private static List<ThingDef> thingDefs;
        private static List<float> values;

        public ThingDef DispensableDef => ThingDefOf.MealSimple; //This is placeholder.

        private static Material potFull = MaterialPool.MatFrom("Things/Building/CM_Pot/CM_Pot_Full", ShaderDatabase.Transparent);

        public bool CanDispenseMealNow
        {
            get
            {
                return nutritionCount >= 0.5;
            }
        }

        public float NutritionSpaceLeft
        {
            get
            {
                return 25 - nutritionCount;
            }
        }

        public bool Empty
        {
            get
            {
                return nutritionCount <= 0;
            }
        }

        public bool StorageTabVisible => true;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref nutritionCount, "nutritionCount", 0, false);
            Scribe_Collections.Look(ref ingredients, "ingredients", LookMode.Def, LookMode.Value, ref thingDefs, ref values);
        }

        protected override void ReceiveCompSignal(string signal)
        {
            if (signal == "Ruined")
            {
                nutritionCount = 0f;
                ingredients.Clear();
                Messages.Message("CM_MessageRuined".Translate().CapitalizeFirst(), new TargetInfo(PositionHeld, Map, false), MessageTypeDefOf.NegativeEvent, true);
                DirtyMapMesh(Map);
            }
        }

        public void AddNutrition(float count)
        {
            float num = Mathf.Min(count, 25 - nutritionCount);
            if (num <= 0f)
            {
                return;
            }
            nutritionCount += num;
        }

        public void AddNutrition(Thing food)
        {
            float num = Mathf.Min(food.stackCount * food.GetStatValue(StatDefOf.Nutrition), 25 - nutritionCount);
            if (num > 0f)
            {
                AddNutrition(num);
                food.SplitOff(food.stackCount).Destroy(DestroyMode.Vanish);
                if (!ingredients.ContainsKey(food.def))
                {
                    ingredients.Add(food.def, num);
                    //Log.Message($"Added {food.def.label} to {def.defName}, with initial nutrition value {num}");
                }
                else
                {
                    ingredients[food.def] += num;
                    //Log.Message($"Added {num} nutrition to {food.def.label} contents of {def.defName}");
                }
            }
            if (CanDispenseMealNow)
            {
                DirtyMapMesh(Map);
            }
        }

        public override string GetInspectString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(base.GetInspectString());
            sb.AppendInNewLine("Nutrition: " + nutritionCount.ToString() + "/25");
            return sb.ToString().TrimEndNewlines();
        }

        // Here goes a method to extract a meal with the chances and substract 0.5 nutrition.
        public Thing TryDispenseMeal()
        {
            if (!CanDispenseMealNow)
            {
                return null;
            }
            nutritionCount -= 0.5f;
            float num = Rand.Range(0f, 1f);
            Thing meal;
            if (num >= 0f && num < 0.5f)
            {
                meal = ThingMaker.MakeThing(ThingDefOf.MealNutrientPaste, null);
            }
            else if (num >= 0.5f && num < 0.8f)
            {
                meal = ThingMaker.MakeThing(ThingDefOf.MealSimple, null);
            }
            else if (num >= 0.8f && num < 0.95f)
            {
                meal = ThingMaker.MakeThing(ThingDefOf.MealFine, null);
            }
            else
            {
                meal = ThingMaker.MakeThing(DefDatabase<ThingDef>.GetNamed("MealLavish"), null);
            }
            // Here should be code to select the ingredients for the meal once we add code to register those on input.
            ThingDef[] ings = ingredients.Keys.ToArray();
            //foreach (ThingDef thing in ings)
            //{
            //    Log.Warning($"Dictionary contains keys: {thing.defName}");
            //}
            ThingDef ing = ings[Rand.Range(0, ings.Length - 1)];
            //Log.Error($"{ing.defName} IS THERE!");
            meal.TryGetComp<CompIngredients>().RegisterIngredient(ing);
            ingredients[ing] -= 0.5f;
            string lab = ing.label; // DEBUG: REMOVE
            if (ingredients[ing] <= 0f)
            {
                //Log.Warning($"Removed {ingredients[ing]} nutrition of {ing.label} from dictionary.");
                ingredients.Remove(ing);
            }
            //Log.Warning($"Dispensed {meal.def.label} with ingredients {lab}. Dictionary holds {ingredients.Count} keyvalue pairs");
            return meal;
        }

        public override void Print(SectionLayer layer)
        {
            base.Print(layer);
            if (CanDispenseMealNow)
            {
                //Graphics.DrawMesh(MeshPool.plane10, Matrix4x4.TRS(DrawPos, Quaternion.identity, DrawSize), potFull, 0);
                Printer_Plane.PrintPlane(layer, DrawPos, DrawSize, potFull);
            }
        }

        public StorageSettings GetStoreSettings()
        {
            return def.building.defaultStorageSettings;
        }

        public StorageSettings GetParentStoreSettings()
        {
            return def.building.fixedStorageSettings;
        }

        public void Notify_SettingsChanged()
        {
        }
    }
}
