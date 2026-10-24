using HarmonyLib;
using Verse;

namespace CauldronMod
{
    public class CauldronMod : Mod
    {
       
        public CauldronMod(ModContentPack content) : base(content)
        {
            Harmony harmony = new Harmony("Argon.CauldronMod");
            harmony.PatchAll();
        }
       
    }
}
