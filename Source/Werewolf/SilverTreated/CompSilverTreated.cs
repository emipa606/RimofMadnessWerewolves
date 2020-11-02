using RimWorld;
using Verse;

namespace Werewolf
{
    public class CompSilverTreated : ThingComp
    {
        public bool treated = false;

        public override string TransformLabel(string label)
        {
            return treated ? (string)(label + "ROM_SilverAmmo".Translate()) : base.TransformLabel(label);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref treated, "treated", false);
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                if (!treated && this?.parent?.Stuff == ThingDefOf.Silver)
                {
                    treated = true;
                }
            }
        }

    }
}
