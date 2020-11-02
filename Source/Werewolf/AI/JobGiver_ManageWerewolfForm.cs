using RimWorld;
using Verse;
using Verse.AI;

namespace Werewolf
{
    public class JobGiver_AttackAndTransform : JobGiver_AIFightEnemy
    {
        protected override bool TryFindShootingPosition(Pawn pawn, out IntVec3 dest)
        {
            _ = !pawn.IsColonist;
            Verb verb = pawn.TryGetAttackVerb(null);
            if (verb == null)
            {
                dest = IntVec3.Invalid;
                return false;
            }
            return CastPositionFinder.TryFindCastPosition(new CastPositionRequest
            {
                caster = pawn,
                target = pawn.mindState.enemyTarget,
                verb = verb,
                maxRangeFromTarget = verb.verbProps.range,
                wantCoverFromTarget = verb.verbProps.range > 5f
            }, out dest);
        }


        protected override Job TryGiveJob(Pawn pawn)
        {
            UpdateEnemyTarget(pawn);
            Thing enemyTarget = pawn.mindState.enemyTarget;
            if (enemyTarget == null)
            {
                return null;
            }

            _ = !pawn.IsColonist;
            Verb verb = pawn.TryGetAttackVerb(null);
            if (verb == null)
            {
                return null;
            }

            if (pawn.GetComp<CompWerewolf>() is CompWerewolf w && w.IsWerewolf)
            {
                if (!w.IsTransformed && w.IsBlooded)
                {
                    w.TransformInto(w.HighestLevelForm, false);
                }
            }

            if (verb.verbProps.IsMeleeAttack)
            {
                return MeleeAttackJob(enemyTarget);
            }
            var flag = CoverUtility.CalculateOverallBlockChance(pawn.Position, enemyTarget.Position, pawn.Map) > 0.01f;
            var flag2 = pawn.Position.Standable(pawn.Map);
            var flag3 = verb.CanHitTarget(enemyTarget);
            var flag4 = (pawn.Position - enemyTarget.Position).LengthHorizontalSquared < 25;
            if ((flag && flag2 && flag3) || (flag4 && flag3))
            {
                return new Job(JobDefOf.Wait_Combat, ExpiryInterval_ShooterSucceeded.RandomInRange, true);
            }
            if (!TryFindShootingPosition(pawn, out IntVec3 intVec))
            {
                return null;
            }
            if (intVec == pawn.Position)
            {
                return new Job(JobDefOf.Wait_Combat, ExpiryInterval_ShooterSucceeded.RandomInRange, true);
            }
            var newJob = new Job(JobDefOf.Goto, intVec)
            {
                expiryInterval = ExpiryInterval_ShooterSucceeded.RandomInRange,
                checkOverrideOnExpire = true
            };
            pawn.Map.pawnDestinationReservationManager.Reserve(pawn, newJob, intVec);
            return newJob;


        }

    }
}
