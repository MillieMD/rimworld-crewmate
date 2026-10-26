using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Crewmate;
using Unity.Burst;
using Verse;

namespace RimWorld
{
    /// <summary>
    /// Clone of CompAbilityEffect_Teleport - except in PreCastAction where the sound effect from Props is used, to allow the Vent sound effect to play
    /// </summary>
    public class CompAbilityEffect_PhantomSmoke : CompAbilityEffect_Smokepop
    {
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            GenExplosion.DoExplosion(target.Cell, parent.pawn.MapHeld, Props.smokeRadius, DamageDefOf.Smoke, null, -1, -1f, null, null, null, null, null, 0f, 1, GasType.BlindSmoke, null, 255, false, null, 0, 1, 0, false, null, null, null, true, 1, 0, false);
        }
    }
}