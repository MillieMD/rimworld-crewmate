using System.Collections.Generic;
using Verse;
using HarmonyLib;

namespace Crewmate
{
    public class ViolenceOverride : DefModExtension
    {
        public bool overrideIncapableViolence = false;

    }

    [HarmonyPatch]
    public class ViolencePatch
    {
        [HarmonyPatch(typeof(Pawn), "WorkTagIsDisabled")]
        public static bool Prefix(Pawn __instance, WorkTags w, ref bool __result)
        {

            if(!ModsConfig.BiotechActive) return true; // Just do the normal method

            if(w != WorkTags.Violent) return true; // Irrelevant, go do normal method

            bool overrideIncapableViolence = false;

            if(__instance.genes == null) return true;

            foreach(Gene gene in __instance.genes?.GenesListForReading)
            {
                if(!gene.def.HasModExtension<ViolenceOverride>()) continue;

                overrideIncapableViolence = gene.def.GetModExtension<ViolenceOverride>().overrideIncapableViolence;
            }

            bool incapableViolence = (__instance.CombinedDisabledWorkTags & w) != 0; // False if capable of violence

            __result = incapableViolence && !overrideIncapableViolence;
            return false;
        }
    }
}