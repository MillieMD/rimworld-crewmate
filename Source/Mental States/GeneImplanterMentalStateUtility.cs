using System.Collections.Generic;
using System.Text;

namespace Verse.AI
{
    public static class GeneImplanterMentalStateUtility
    {
        public static Pawn FindPawnToImplant(Pawn pawn)
        {
            if (!pawn.Spawned)
            {
                return null;
            }

            IReadOnlyList<Pawn> allPawns = pawn.Map.mapPawns.AllPawnsSpawned;

            List<Pawn> potentialTargets = new List<Pawn>();

            foreach (Pawn candidate in allPawns)
            {
                Crewmate.Crewmate.Debug($"Candidate: {candidate.Name}");

                if ((candidate.Faction == pawn.Faction || (candidate.IsPrisoner && candidate.HostFaction == pawn.Faction)) // Owned by my tile
                    && candidate.RaceProps.Humanlike // AND is human
                    && candidate != pawn  // AND are not the same person
                    && pawn.CanReach(candidate, PathEndMode.Touch, Danger.Deadly) // AND reachable
                    && (candidate.CurJob == null || !candidate.CurJob.exitMapOnArrival // AND not going to leave the map
                    && candidate.genes?.Xenotype != pawn.genes?.Xenotype)) // AND not the same xenotype
                {
                    potentialTargets.Add(candidate);
                    Crewmate.Crewmate.Debug($"Selected: {candidate}");
                }
            }

            if(!potentialTargets.Any())
            {
                Crewmate.Crewmate.Debug("No targets found. Returning null");
                return null;
            }

            foreach(Pawn target in potentialTargets)
            {
                Crewmate.Crewmate.Debug($"Potential Target: {target.Name}");
            }

            return potentialTargets.RandomElement();
        }
    }
}