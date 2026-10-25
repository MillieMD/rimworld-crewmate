using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
    /// <summary>
    /// Clone of CompAbilityEffect_Teleport - except in PreCastAction where the sound effect from Props is used, to allow the Vent sound effect to play
    /// </summary>
    public class CompAbilityEffect_Vent : CompAbilityEffect_Teleport
    {
        public override IEnumerable<PreCastAction> GetPreCastActions()
        {
            Log.Message("get vent pre cast");
            yield return new PreCastAction
            {
                action = delegate (LocalTargetInfo t, LocalTargetInfo d)
                {

                    Log.Message("called precast action for vent");

                    if (!parent.def.HasAreaOfEffect)
                    {
                        Pawn pawn = t.Pawn;
                        if (pawn != null)
                        {
                            FleckCreationData dataAttachedOverlay = FleckMaker.GetDataAttachedOverlay(pawn, FleckDefOf.PsycastSkipFlashEntry, new Vector3(-0.5f, 0f, -0.5f));
                            dataAttachedOverlay.link.detachAfterTicks = 5;
                            pawn.Map.flecks.CreateFleck(dataAttachedOverlay);
                        }
                        else
                        {
                            FleckMaker.Static(t.CenterVector3, parent.pawn.Map, FleckDefOf.PsycastSkipFlashEntry);
                        }
                        FleckMaker.Static(d.Cell, parent.pawn.Map, FleckDefOf.PsycastSkipInnerExit);
                    }
                    
                    if (Props.destination != AbilityEffectDestination.RandomInRange)
                    {
                        FleckMaker.Static(d.Cell, parent.pawn.Map, FleckDefOf.PsycastSkipOuterRingExit);
                    }
                },
                ticksAwayFromCast = 5
            };
        }

        private AcceptanceReport CanSkipTarget(LocalTargetInfo target)
        {
            if (target.Thing is Pawn pawn)
            {
                if (pawn.BodySize > Props.maxBodySize)
                {
                    return "CannotSkipTargetTooLarge".Translate();
                }
                if (pawn.kindDef.skipResistant)
                {
                    return "CannotSkipTargetPsychicResistant".Translate();
                }
            }
            return true;
        }
    }
}
