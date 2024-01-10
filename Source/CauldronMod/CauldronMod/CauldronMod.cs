using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using HarmonyLib;

namespace CauldronMod
{
    [StaticConstructorOnStartup]
    public static class CauldronMod
    {
        static CauldronMod()
        {
            Harmony harmony = new Harmony("Argon.CauldronMod");
            harmony.PatchAll();
        }
    }
}
