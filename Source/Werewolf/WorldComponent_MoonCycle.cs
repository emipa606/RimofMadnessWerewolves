using System;
using Verse;
using RimWorld;
using System.Collections.Generic;
using RimWorld.Planet;
using System.Linq;

namespace Werewolf
{
    public class WorldComponent_MoonCycle : WorldComponent
    {
        public List<Moon> moons;
        GameCondition gcMoonCycle = null;
        public int ticksUntilFullMoon = -1;
        public int ticksPerMoonCycle = -1;

        public WorldComponent_MoonCycle(World world) : base(world)
        {
            if (moons.NullOrEmpty())
            {
                GenerateMoons(world);
            }
        }

        public void AdvanceOneDay()
        {
            if (!moons.NullOrEmpty())
                foreach (Moon moon in moons) moon.AdvanceOneDay();
        }

        public void AdvanceOneQuadrum()
        {
            if (!moons.NullOrEmpty())
                foreach (Moon moon in moons) moon.AdvanceOneQuadrum();
        }

        public void DebugTriggerNextFullMoon()
        {
            Moon soonestMoon = null;
            if (!moons.NullOrEmpty() && gcMoonCycle != null)
            {
                soonestMoon = moons.MinBy(x => x.DaysUntilFull);
                for (int i = 0; i < soonestMoon.DaysUntilFull; i++)
                    AdvanceOneDay();
                soonestMoon.FullMoonIncident();
            }
        }

        public void DebugRegenerateMoons(World world)
        {
            moons = null;
            GenerateMoons(world);
            Messages.Message("DEBUG :: Moons Regenerated", MessageTypeDefOf.TaskCompletion);
        }

        public void GenerateMoons(World world)
        {
            if (moons == null) moons = new List<Moon>();

            //1-2% chance there are more than 3 moons.
            int numMoons = 1;
            float val = Rand.Value;
            if (val > 0.98) numMoons = Rand.Range(4, 6);
            else if (val > 0.7) numMoons = 3;
            else if (val > 0.4) numMoons = 2;
            for (int i = 0; i < numMoons; i++)
            {
                int uniqueID = 1;
                if (moons.Any<Moon>())
                    uniqueID = moons.Max((Moon o) => o.UniqueID) + 1;

                moons.Add(new Moon(uniqueID, world, Rand.Range(350000 * (i + 1), 600000 * (i + 1))));
            }
        }

        public Dictionary<Pawn, int> recentWerewolves = new Dictionary<Pawn, int>();


        public override void WorldComponentTick()
        {
            base.WorldComponentTick();
            if (!moons.NullOrEmpty())
            {
                foreach (Moon moon in moons)
                {
                    moon.Tick();
                }
            }
            if (gcMoonCycle == null)
            {
                gcMoonCycle = GameConditionMaker.MakeConditionPermanent(WWDefOf.ROM_MoonCycle);
                Find.World.gameConditionManager.RegisterCondition(gcMoonCycle);
            }

            if (recentWerewolves.Any())
                recentWerewolves.RemoveAll(x => x.Key.Dead || x.Key.DestroyedOrNull());
            if (recentWerewolves.Any())
            {
                var recentVampiresKeys = new List<Pawn>(recentWerewolves.Keys);
                foreach (var key in recentVampiresKeys)
                {
                    recentWerewolves[key] += 1;
                    if (recentWerewolves[key] > 100)
                    {
                        recentWerewolves.Remove(key);
                        if (!key.Spawned || key.Faction == Faction.OfPlayerSilentFail) continue;
                        Find.LetterStack.ReceiveLetter("ROM_WerewolfEncounterLabel".Translate(),
                                "ROM_WerewolfEncounterDesc".Translate(key.LabelShort), LetterDefOf.ThreatSmall, key, null);
                    }
                }
            }
        }

        public int DaysUntilFullMoon
        {
            get
            {
                return 0;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref ticksUntilFullMoon, "ticksUntilFullMoon", -1, false);
            Scribe_References.Look<GameCondition>(ref gcMoonCycle, "gcMoonCycle");
            if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs && gcMoonCycle == null)
            {
                gcMoonCycle = Find.World.GameConditionManager.GetActiveCondition<GameCondition_MoonCycle>();
                Log.Warning($"{this}.gcMoonCycle wasn't a proper reference in save file, likely due to outdated ROM Werewolf; " +
                    $"attempting to default to first active GameCondition_MoonCycle: {gcMoonCycle?.GetUniqueLoadID() ?? "null"}");
                if (gcMoonCycle != null)
                {
                    if (gcMoonCycle.uniqueID == -1)
                    {
                        var uniqueID = Find.UniqueIDsManager.GetNextGameConditionID(); ;
                        Log.Warning($"{this}.gcMoonCycle.uniqueID is unassigned (-1); setting it to {uniqueID}");
                        gcMoonCycle.uniqueID = uniqueID;
                    }
                    if (gcMoonCycle.def != WWDefOf.ROM_MoonCycle)
                    {
                        Log.Warning($"{this}.gcMoonCycle.def is {gcMoonCycle.def.ToStringSafe()}; " +
                            $"setting it to {WWDefOf.ROM_MoonCycle}");
                        gcMoonCycle.def = WWDefOf.ROM_MoonCycle;
                    }
                }
            }
            Scribe_Collections.Look<Moon>(ref moons, "moons", LookMode.Deep, new object[0]);
        }
    }
}
