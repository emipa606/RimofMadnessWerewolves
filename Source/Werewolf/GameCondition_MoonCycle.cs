using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Text;

namespace Werewolf
{
    public class GameCondition_MoonCycle : GameCondition
    {
        private WorldComponent_MoonCycle wcMoonCycle = null;
        public WorldComponent_MoonCycle WCMoonCycle
        {
            get
            {
                if (wcMoonCycle == null)
                {
                    wcMoonCycle = Find.World?.GetComponent<WorldComponent_MoonCycle>();
                }
                return wcMoonCycle;
            }
        }

        public int SoonestFullMoonInDays
        {
            get
            {
                var result = -1;
                if (WCMoonCycle.moons is List<Moon> moons && !moons.NullOrEmpty())
                {
                    for (var i = 0; i < moons.Count; i++)
                    {
                        if (result == -1)
                        {
                            result = moons[i].DaysUntilFull;
                        }

                        if (moons[i].DaysUntilFull < result)
                        {
                            result = moons[i].DaysUntilFull;
                        }
                    }
                }
                return result;
            }
        }
        
        public override string Label
        {
           get
           {
                var result = SoonestFullMoonInDays > 0
                    ? (string)"ROM_MoonCycle_UntilNextFullMoon".Translate(SoonestFullMoonInDays)
                    : (string)"ROM_MoonCycle_FullMoonImminentArgless".Translate();
                return result;
            }
        }

        public override void End()
        {
            gameConditionManager.ActiveConditions.Remove(this);
        }

        public override string TooltipString
        {
            get
            {
                string result = "ROM_MoonCycle_CurrentPhaseDesc".Translate(WCMoonCycle.world.info.name);
                var s = new StringBuilder();
                s.AppendLine(result);
                s.AppendLine();
                if (WCMoonCycle.moons is List<Moon> MoonList && !MoonList.NullOrEmpty())
                {
                    s.AppendLine("ROM_MoonCycle_Moons".Translate(WCMoonCycle.world.info.name));
                    s.AppendLine("------");

                    foreach (Moon m in MoonList)
                    {
                        var daysLeft = m.DaysUntilFull;
                        if (daysLeft > 0)
                        {
                            s.AppendLine("  " + "ROM_MoonCycle_CurrentPhase".Translate(new object[] { m.Name, m.DaysUntilFull }));
                        }
                        else
                        {
                            s.AppendLine("  " + "ROM_MoonCycle_FullMoonImminent".Translate(m.Name));
                        }
                    }
                }
                return s.ToString().TrimEndNewlines();
            }
        }
    }
}
