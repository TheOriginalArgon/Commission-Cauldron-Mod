using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CauldronMod
{
    public class CompProperties_Cauldron : CompProperties
    {
        public CompProperties_Cauldron()
        {
            compClass = typeof(CompCauldron);
        }

        public float ticksToRuin;
        public float ticksToStir;
    }

    public class CompCauldron : ThingComp
    {
        public CompProperties_Cauldron Props => (CompProperties_Cauldron)props;

        protected float ticksToStir;

        protected float ruinedPercent;

        public bool NeedsStirringNow => ticksToStir >= Props.ticksToStir;

        public void Reset() { ruinedPercent = 0f; ticksToStir = 0f; }

        public void ResetStir() {  ticksToStir = 0f; }

        public override string CompInspectStringExtra()
        {
            if (NeedsStirringNow || !parent.TryGetComp<CompRefuelable>().HasFuel)
            {
                StringBuilder sb = new StringBuilder();
                if (ruinedPercent > 0f)
                {
                    sb.Append(base.CompInspectStringExtra());
                    sb.AppendInNewLine("Runining: " + ruinedPercent.ToStringPercent());
                    return sb.ToString();
                }
            }
            return null;
        }

        public override void CompTick()
        {
            DoTicks(1);
        }

        public override void CompTickRare()
        {
            DoTicks(250);
        }

        private void DoTicks(int ticks)
        {
            if (!(parent as Building_FoodPot).Empty)
            {
                if (ruinedPercent < 1f)
                {
                    if (NeedsStirringNow)
                    {
                        ruinedPercent += 0.00001f * ticks;
                    }
                    else if (!parent.TryGetComp<CompRefuelable>().HasFuel)
                    {
                        ruinedPercent += 0.00005f * ticks;
                    }
                    else
                    {
                        if (ruinedPercent > 0f)
                        {
                            ruinedPercent = 0f;
                        }
                        ticksToStir += 0.5f * ticks;
                    }
                    if (ruinedPercent < 0f)
                    {
                        ruinedPercent = 0f;
                    }
                }
                else if (ruinedPercent >= 1f)
                {
                    ruinedPercent = 1f;
                    parent.BroadcastCompSignal("Ruined");
                    Reset();
                }
                //Log.Message("RuinPct: " + ruinedPercent);
                //Log.Message("StirPct: " + ticksToStir);
            }
        }

        public override void PostExposeData()
        {
            Scribe_Values.Look(ref ruinedPercent, "ruinedPercent", 0f, false);
            Scribe_Values.Look(ref ticksToStir, "ticksToStir", 0f, false);
        }
    }
}
