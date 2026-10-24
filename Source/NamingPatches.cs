using System;
using System.Collections.Generic;
using Verse;
using Verse.Grammar;
using RimWorld;
using HarmonyLib;

namespace Crewmate
{
    /// <summary>
    /// Patches to the PawnGenerator.GeneratePawn method to enable Crewmates to have names based on their colours
    /// </summary>
    [HarmonyPatch]
    class NamingPatches
    {
        /// <summary>
        /// Rename a generated pawn (not babies) if they have the crewmate xenotype.
        /// Required as generated pawns are given a name before their genes are activated.
        /// </summary>
        [HarmonyPatch(typeof(PawnGenerator), "GeneratePawn", new Type[] { typeof(PawnGenerationRequest) })]
        public static void Postfix(ref Pawn __result)
        {

            XenotypeDef xenotype = __result.genes?.Xenotype;

            if (!ModsConfig.BiotechActive || xenotype == null)
            {
                Crewmate.Debug("Biotech inactive or xenotype is null. Skipping POSTFIX of PawnGenerator.GeneratePawn");
                return;
            }

            if (xenotype.defName != "Crewmate")
            {
                Crewmate.Debug($"Pawn is not a crewmate ({xenotype.defName}). Skipping POSTFIX of PawnGenerator.GeneratePawn");
                return;
            }

            __result.Name = generateCrewmateName(__result);
        }

        /// <summary>
        /// Prefix pawn name generation to inject Crewmate colour naming rules.
        /// </summary>
        [HarmonyPatch(typeof(PawnBioAndNameGenerator), nameof(PawnBioAndNameGenerator.GeneratePawnName))]
        public static bool Prefix(ref Pawn pawn, ref Name __result)
        {
            
            XenotypeDef xenotype = pawn.genes?.Xenotype;

            if (!ModsConfig.BiotechActive || xenotype == null)
            {
                Crewmate.Debug("Biotech inactive or xenotype is null. Skipping PREFIX of PawnGenerator.GeneratePawn");
                return true;
            }

            if (xenotype.defName != "Crewmate")
            {
                Crewmate.Debug($"Pawn is not a Crewmate ({xenotype.defName}). Skipping PREFIX of PawnGenerator.GeneratePawn");
                return true;
            }

            __result = generateCrewmateName(pawn);
            return false;
        }

        /// <summary>
        /// Generate a name for a Crewmate Pawn using the Strings for their colour
        /// </summary>
        private static Name generateCrewmateName(Pawn pawn)
        {
            XenotypeDef xenotype = pawn.genes.Xenotype;

            RulePackDef nameMaker = xenotype.GetNameMaker(pawn.gender);
            bool appendNumberIfNameUsed = false;
            string rootKeyword = null;
            string testPawnNameSymbol = null;
            bool forceNoNick = false;

            List<Rule> extraRules = getPawnColourRules(pawn.genes); 

            NameTriple name = NameTriple.FromString(NameGenerator.GenerateName(
                nameMaker,
                x => true,
                appendNumberIfNameUsed,
                rootKeyword,
                testPawnNameSymbol,
                extraRules),
                forceNoNick);

            // Force nickname to be the same as the first name
            return new NameTriple(name.First, name.First, name.Last);
        }

        /// <summary>
        /// Generate the Rule_File that corresponds to a Pawn's Crewmate_Skin_ genes.
        /// </summary>
        /// <returns> List of a Rules with relevant ColourName.txt strings loaded. Empty List if no Crewmate_Skin_ gene present</returns>
        private static List<Rule> getPawnColourRules(Pawn_GeneTracker genes)
        {
            List<Rule> extraRules = new List<Rule>();

            foreach (Gene gene in genes.Endogenes)
            {

                if (gene.def.defName.Contains("Crewmate_Skin_"))
                {
                    string colourName = gene.def.defName.Split("Crewmate_Skin_")[1];

                    // Colour variations use base colour file
                    if (colourName == "Saturated_Yellow") colourName = "Yellow";
                    if (colourName == "Fortegreen") colourName = "Green";

                    Rule_File rule = new Rule_File
                    {
                        keyword = "PAWN_skincolour",
                        path = $"Names/OrganisedColours/{colourName}Names"
                    };

                    // Forces rule to read strings from file, otherwise the rule is unresolvable
                    rule.Init();

                    extraRules.Add(rule);

                    Crewmate.Debug($"Added rule: {rule.keyword}->{rule.path}");
                    Crewmate.Debug(rule.ToString());
                }
            }

            return extraRules;
        }
    }
}