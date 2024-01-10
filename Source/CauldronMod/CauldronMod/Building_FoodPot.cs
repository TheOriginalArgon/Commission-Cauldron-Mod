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
    public class Building_FoodPot : Building
    {
        private float nutritionCount;

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

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref nutritionCount, "nutritionCount", 0, false);
        }

        public void AddNutrition(float count)
        {
            float num = Mathf.Min(count, 25 - nutritionCount);
            if (num <= 0)
            {
                return;
            }
            nutritionCount += num;
        }

        public void AddNutrition(Thing food)
        {
            float num = Mathf.Min(food.stackCount * food.GetStatValue(StatDefOf.Nutrition), 25 - nutritionCount);
            if (num > 0)
            {
                AddNutrition(num);
                food.SplitOff(food.stackCount).Destroy(DestroyMode.Vanish);
            }
        }

        public override string GetInspectString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(base.GetInspectString());
            sb.AppendLine("Nutrition: " + nutritionCount.ToString() + "/25");
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
            Thing meal = ThingMaker.MakeThing(ThingDefOf.MealSimple, null); // DEBUG: Needs to have chances of spawning different meals.
            // Here should be code to select the ingredients for the meal once we add code to register those on input.
            return meal;
        }

    }
}
