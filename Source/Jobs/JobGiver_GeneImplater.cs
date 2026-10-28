using Verse;
using Verse.AI;

namespace RimWorld
{

    public class JobGiver_GeneImplanter : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            Crewmate.Crewmate.Debug("JobGiver_GeneImplanter::TryGiveJob");

            if (!(pawn.MentalState is MentalState_GeneImplanter mentalState) || !mentalState.IsTargetStillValidAndReachable())
            {
                Crewmate.Crewmate.Debug("Target no longer valid or reachable");
                return null;
            }

            Job job = JobMaker.MakeJob(Crewmate.JobDefOf.ForceImplantXenogerm, mentalState.target);
            job.canBashDoors = true;

            return job;
        }
    }
}
