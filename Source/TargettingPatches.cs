using Verse;
using RimWorld;
using HarmonyLib;

namespace Crewmate
{
    /// <summary>
    /// Patches for Impostor ability targetting
    /// </summary>
    [HarmonyPatch]
    class TargettingPatches
    {
        /// <summary>
        /// Patch to override targetting logic so that <canTargetSelf> takes priority over all other parameters - allowing a pawn to target themselves, but nothing else.
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