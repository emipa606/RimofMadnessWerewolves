using RimWorld;
using Verse;
using Verse.AI;

namespace Werewolf
{
    public class JobGiver_WerewolfHunt : ThinkNode_JobGiver
    {
        private const int MinMeleeChaseTicks = 900;

        private const int MaxMeleeChaseTicks = 1800;

        protected override Job TryGiveJob(Pawn pawn)
        {
            if (pawn.TryGetAttackVerb(null) == null)
            {
                return null;
            }
            Pawn pawn2 = FindPawnTarget(pawn);
            if (pawn2 != null && pawn.CanReach(pawn2, PathEndMode.Touch, Danger.Deadly, false, TraverseMode.ByPawn))
            {
                return MeleeAttackJob(pawn, pawn2);
            }

            Building building = FindTurretTarget(pawn);
            if (building != null)
            {
                return MeleeAttackJob(pawn, building);
            }
            if (pawn2 != null)
            {
                using (PawnPath pawnPath = pawn.Map.pathFinder.FindPath(pawn.Position, pawn2.Position, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.PassAllDestroyableThings, false), PathEndMode.OnCell))
                {
                    if (!pawnPath.Found)
                    {
                        return null;
                    }
                    Thing thing = pawnPath.FirstBlockingBuilding(out IntVec3 cellBeforeBlocker, pawn);
                    if (thing != null)
                    {
                        //Job job = DigUtility.PassBlockerJob(pawn, thing, cellBeforeBlocker, true);
                        //if (job != null)
                        //{
                        return MeleeAttackJob(pawn, thing);
                        //}
                    }
                    IntVec3 loc = pawnPath.LastCellBeforeBlockerOrFinalCell(pawn.MapHeld);
                    IntVec3 randomCell = CellFinder.RandomRegionNear(loc.GetRegion(pawn.Map, RegionType.Set_Passable), 9, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), null, null, RegionType.Set_Passable).RandomCell;
                    return randomCell == pawn.Position ? new Job(JobDefOf.Wait, 30, false) : new Job(JobDefOf.Goto, randomCell);
                }
            }
            Building buildingDoor = FindDoorTarget(pawn);
            return buildingDoor != null ? MeleeAttackJob(pawn, buildingDoor) : null;
        }

        private Job MeleeAttackJob(Pawn pawn, Thing target)
        {
            return new Job(JobDefOf.AttackMelee, target)
            {
                maxNumMeleeAttacks = 1,
                expiryInterval = Rand.Range(MinMeleeChaseTicks, MaxMeleeChaseTicks),
                attackDoorIfTargetLost = true,
                killIncappedTarget = true
            };
        }

        private Pawn FindPawnTarget(Pawn pawn)
        {
            return (Pawn)AttackTargetFinder.BestAttackTarget(pawn, TargetScanFlags.NeedReachable, (Thing x) => x is Pawn p && !p.Dead && x.def.race.intelligence >= Intelligence.ToolUser, 0f, 9999f, default, 3.40282347E+38f, true);
        }

        private Building FindTurretTarget(Pawn pawn)
        {
            return (Building)AttackTargetFinder.BestAttackTarget(pawn, TargetScanFlags.NeedLOSToPawns | TargetScanFlags.NeedLOSToNonPawns | TargetScanFlags.NeedReachable | TargetScanFlags.NeedThreat, (Thing t) => t is Building, 0f, 70f, default, 3.40282347E+38f, false);
        }


        private Building_Door FindDoorTarget(Pawn pawn)
        {
            return (Building_Door)AttackTargetFinder.BestAttackTarget(pawn, TargetScanFlags.NeedReachable, (Thing t) => t is Building_Door, 0f, 70f, default, 3.40282347E+38f, false);
        }
    }
}
