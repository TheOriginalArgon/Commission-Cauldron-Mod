using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using HarmonyLib;
using RimWorld;

namespace CauldronMod
{
    [StaticConstructorOnStartup]
    public static class CauldronMod
    {
        static CauldronMod()
        {
            Harmony harmony = new Harmony("Argon.CauldronMod");
            harmony.PatchAll();
            //harmony.Patch(typeof(FoodUtility).GetNestedTypes(AccessTools.all).SelectMany(AccessTools.GetDeclaredMethods).First(mi => mi.Name.Contains("foodValidator")), postfix: new HarmonyMethod(typeof(HarmonyPatch_Delegate), "Prefix_Delegate"));
        }
    }
}
