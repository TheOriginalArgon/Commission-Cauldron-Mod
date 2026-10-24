using RimWorld;
using UnityEngine;
using Verse;

namespace CauldronMod
{
    public class CompProperties_Cauldron : CompProperties
    {
        public CompProperties_Cauldron() { compClass = typeof(CompCauldron); }
        public float ticksToRuin = 15000f;
        public float ticksToStir = 10000f;
    }

    public class CompCauldron : ThingComp
    {
        public CompProperties_Cauldron Props => (CompProperties_Cauldron)props;
        protected float ticksToStir;
        protected float ruinedPercent;
        public bool NeedsStirringNow => parent is Building_FoodPot pot && !pot.Empty && ticksToStir >= Props.ticksToStir;
        public void Reset() { ruinedPercent = 0f; ticksToStir = 0f; }
        public void ResetStir() { ticksToStir = 0f; }

        public override string CompInspectStringExtra()
        {
            string result = NeedsStirringNow ? "CM_NeedsStirring".Translate().ToString() : null;
            if (ruinedPercent > 0f)
                result = (result.NullOrEmpty() ? "" : result + "\n") + "CM_Ruining".Translate(ruinedPercent.ToStringPercent());
            return result;
        }

        public override void CompTick() => DoTicks(1);
        public override void CompTickRare() => DoTicks(250);

        private void DoTicks(int ticks)
        {
            if (!(parent is Building_FoodPot pot) || pot.Empty) { Reset(); return; }
            bool hasFuel = parent.GetComp<CompRefuelable>()?.HasFuel == true;
            // Both neglect conditions use the duration declared in XML. Stirring
            // cannot clear spoilage caused by a fire that is still unlit.
            if (NeedsStirringNow || !hasFuel)
                ruinedPercent += ticks / Mathf.Max(1f, Props.ticksToRuin);
            else
            {
                ruinedPercent = 0f;
                ticksToStir += ticks;
            }
            if (ruinedPercent >= 1f)
            {
                parent.BroadcastCompSignal("Ruined");
                Reset();
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref ruinedPercent, "ruinedPercent", 0f);
            Scribe_Values.Look(ref ticksToStir, "ticksToStir", 0f);
        }
    }
}
