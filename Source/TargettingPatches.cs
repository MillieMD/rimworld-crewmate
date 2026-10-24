using Verse;
using RimWorld;
using HarmonyLib;

namespace Crewmate
{
    /// <summary>
    /// Enable mpostor ability targetting
    /// </summary>
    [HarmonyPatch]
    class TargettingPatches
    {
        /// <summary>
        /// Override targetting logic so &lt;canTargetSelf&gt; takes priority over all other parameters - allowing a pawn to target themselves, even if they cannot target other Pawns
        /// </summary>
        [HarmonyPatch(typeof(TargetingParameters), nameof(TargetingParameters.CanTarget))]
        public static bool Prefix(TargetingParameters __instance, TargetInfo targ, ITargetingSource source, ref bool __result)
        {
            if(__instance.canTargetSelf && (source.Caster == targ.Thing))
            {
                Crewmate.Debug("Can target self");
                __result = true;
                return false;
            }

            return true; 
        }
    }
}