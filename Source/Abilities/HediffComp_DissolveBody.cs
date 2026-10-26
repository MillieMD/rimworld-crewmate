using Verse;

namespace RimWorld
{
    class HediffComp_DissolveBody : HediffComp
    {
        public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
        {
            Log.Message($"Died. Destroying {Pawn.Name}'s corpse");
            Pawn.Corpse.Destroy();
        }

    }
}