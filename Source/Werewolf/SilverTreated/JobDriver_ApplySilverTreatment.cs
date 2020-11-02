using RimWorld;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Werewolf
{
    public class JobDriver_ApplySilverTreatment : JobDriver
    {
        private float workLeft;

        private float totalNeededWork;

        protected Thing Target => job.targetA.Thing;

        protected Building Building => (Building)Target.GetInnerIfMinified();

        protected int TotalNeededWork
        {
            get
            {
                Building building = Building;
                var value = Mathf.RoundToInt(building.GetStatValue(StatDefOf.WorkToBuild, true));
                return Mathf.Clamp(value, 20, 3000);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<float>(ref workLeft, "workLeft", 0f, false);
            Scribe_Values.Look<float>(ref totalNeededWork, "totalNeededWork", 0f, false);
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null) && pawn.Reserve(job.targetB, job, 1, -1, null) &&
                pawn.Reserve(job.targetC, job, 1, -1, null);
        }

        [DebuggerHidden]
        protected override IEnumerable<Toil> MakeNewToils()
        {
            TargetIndex weapon = TargetIndex.A;
            TargetIndex silver = TargetIndex.B;
            TargetIndex machineTable = TargetIndex.C;

            var silverThings = new List<Thing>();

            //Unforbid
            yield return new Toil
            {
                initAction = delegate
                {
                    if (TargetA.Thing is Thing t && t.IsForbidden(Faction.OfPlayer))
                    {
                        t.SetForbidden(false);
                    }
                }
            };
            yield return Toils_Reserve.Reserve(weapon, 1, -1, null);
            Toil tempToil = Toils_Goto.GotoThing(weapon, PathEndMode.ClosestTouch).
                FailOnDespawnedNullOrForbidden(weapon).
                FailOnSomeonePhysicallyInteracting(weapon);
            yield return tempToil;
            yield return Toils_Haul.StartCarryThing(weapon);
            yield return Toils_Haul.CarryHauledThingToCell(machineTable);
            yield return Toils_Haul.PlaceHauledThingInCell(machineTable, tempToil, false);
            this.FailOnForbidden(silver);
            yield return Toils_Reserve.Reserve(silver, 1, -1, null);
            Toil reserveSilver = Toils_Reserve.Reserve(silver, 1, -1, null);
            yield return reserveSilver;
            yield return Toils_Goto.GotoThing(silver, PathEndMode.ClosestTouch).
                FailOnDespawnedNullOrForbidden(silver).FailOnSomeonePhysicallyInteracting(silver);
            yield return Toils_Haul.StartCarryThing(silver, false, true).
                FailOnDestroyedNullOrForbidden(silver);
            yield return new Toil
            {
                initAction = delegate
                {
                    silverThings.Add(TargetB.Thing);
                }, defaultCompleteMode = ToilCompleteMode.Instant
            };
            yield return Toils_Haul.CheckForGetOpportunityDuplicate(reserveSilver, silver, TargetIndex.None, true, null);
            yield return Toils_Goto.GotoThing(machineTable, PathEndMode.Touch);
            yield return Toils_General.Wait(240).
                FailOnDestroyedNullOrForbidden(weapon).FailOnCannotTouch(weapon, PathEndMode.Touch).
                FailOnDestroyedNullOrForbidden(machineTable).
                FailOnDestroyedNullOrForbidden(silver).
                WithProgressBarToilDelay(silver, false, -0.5f);
            yield return new Toil
            {
                initAction = delegate
                {
                    Log.Message("Finished");
                    //this.FinishedRemoving();
                    //this.Map.designationManager.RemoveAllDesignationsOn(this.Target, false);
                    SilverTreatedUtility.ApplySilverTreatment(TargetA.Thing as ThingWithComps, silverThings);
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
        }
        
        protected virtual void TickAction()
        {
        }
    }
}
