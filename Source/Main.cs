using Verse;
using HarmonyLib;
using System.Diagnostics;

namespace Crewmate
{
    [StaticConstructorOnStartup]
    public static class Crewmate
    {
        static Crewmate()
        {
            var harmony = new Harmony("serprentino.amongus");
            harmony.PatchAll();
            Debug("CrewmatePatches initialised");
        }

        /// <summary>
        /// Print message to RimWorld debug console. Only works if the DEBUG flag is set. 
        /// </summary>
        [Conditional("DEBUG")]
        public static void Debug(string message)
        {
            Log.Message(message);
        }
    }
}