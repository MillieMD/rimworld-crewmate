using Multiplayer.API;
using Verse;

namespace RimWorld
{
    public class CompAbilityEffect_PhantomSmoke : CompAbilityEffect_Smokepop
    {
        [SyncMethod]
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            GenExplosion.DoExplosion(target.Cell, parent.pawn.MapHeld, Props.smokeRadius, DamageDefOf.Smoke, null, -1, -1f, null, null, null, null, null, 0f, 1, GasType.BlindSmoke, null, 255, false, null, 0, 1, 0, false, null, null, null, true, 1, 0, false);
        }
    }
}