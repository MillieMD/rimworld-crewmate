using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.Grammar;
using RimWorld;
using HarmonyLib;

namespace Crewmate
{
    [StaticConstructorOnStartup]
    [HarmonyPatch]
    class CrewmatePatches
    {
        const bool LOGGING_ENABLED = false;

        static CrewmatePatches()
        {
            var harmony = new Harmony("serprentino.amongus");
            Log.Message("CrewmatePatches initialised");
            harmony.PatchAll();
        }

        /// <summary>
        /// Force inheritance of a gene where it is active on either the mother or the father, if forceActiveInheritance is enabled
        /// </summary>
        [HarmonyPatch(typeof(PregnancyUtility), "GetInheritedGenes", new Type[] { typeof(Pawn), typeof(Pawn), typeof(bool) }, new ArgumentType[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Out })]
        public static bool Prefix(Pawn father, Pawn mother, out bool success, ref List<GeneDef> __result)
        {
            if (LOGGING_ENABLED) Log.Message("PREFIX: PregnancyUtility.GetInheritedGenes beginning...");

            List<GeneDef> tmpGenes = new List<GeneDef>();
            List<GeneDef> tmpGenesShuffled = new List<GeneDef>();
            Dictionary<GeneDef, float> tmpGeneChances = new Dictionary<GeneDef, float>();

            if (father?.genes != null)
            {
                // Add all father non-melanin && non archite genes with 0.5 chance to inherit
                foreach (Gene endogene in father.genes.Endogenes)
                {
                    if (endogene.def.endogeneCategory != EndogeneCategory.Melanin && endogene.def.biostatArc <= 0)
                    {
                        tmpGeneChances.SetOrAdd(endogene.def, 0.5f);
                        if (!tmpGenesShuffled.Contains(endogene.def))
                        {
                            tmpGenesShuffled.Add(endogene.def);
                        }
                    }
                }
            }
            if (mother?.genes != null)
            {
                foreach (Gene endogene2 in mother.genes.Endogenes)
                {

                    if (endogene2.def.endogeneCategory != EndogeneCategory.Melanin && endogene2.def.biostatArc <= 0)
                    {
                        // add gene to gene shuffled if not already present
                        if (!tmpGenesShuffled.Contains(endogene2.def))
                        {
                            tmpGenesShuffled.Add(endogene2.def);
                        }

                        // if gene has already been inherited from father - 100% inherit chance, else 50%
                        if (tmpGeneChances.ContainsKey(endogene2.def))
                        {
                            tmpGeneChances[endogene2.def] = 1f;
                        }
                        else
                        {
                            tmpGeneChances.Add(endogene2.def, 0.5f);
                        }
                    }
                }
            }

            // tmpGeneChances is currently full list of mother and father genes
            int geneCount = tmpGeneChances.Count;
            Dictionary<GeneDef, float>.KeyCollection keys = tmpGeneChances.Keys;

            for (int i = 0; i < geneCount; i++)
            {

                GeneDef gene = keys.ElementAt(i);

                if (!gene.HasModExtension<InheritanceSettings>())
                {
                    continue;
                }

                bool forced = gene.GetModExtension<InheritanceSettings>().forceActiveInheritance;

                if (!forced)
                {
                    continue;
                }

                bool isActive = mother.genes.HasActiveGene(gene) || father.genes.HasActiveGene(gene);

                if (isActive)
                {
                    tmpGeneChances.SetOrAdd(gene, 1f);
                }
                else
                {
                    tmpGeneChances.SetOrAdd(gene, 0f);
                }

            }

            if (LOGGING_ENABLED)
            {
                foreach (KeyValuePair<GeneDef, float> pair in tmpGeneChances)
                {
                    Log.Message($"Gene: {pair.Key.defName} has a {pair.Value} chance of being added");
                }
            }

            int num = 0;
            // attempt 50 times to create a gene set that the pawn can afford (complexity/met)
            do
            {
                tmpGenes.Clear();
                tmpGenesShuffled.Shuffle();
                foreach (GeneDef item in tmpGenesShuffled)
                {
                    // gene wins coins toss using tempgenechances
                    if (!tmpGenes.Contains(item) && Rand.Chance(tmpGeneChances[item]))
                    {
                        tmpGenes.Add(item);
                    }
                }
                tmpGenes.RemoveAll((GeneDef x) => x.prerequisite != null && !tmpGenes.Contains(x.prerequisite));
                int val = tmpGenes.NonOverriddenGenes(xenogene: false).Sum((GeneDef x) => x.biostatMet);

                bool inBiostatRange = IntUtils.ValueInRange(val, GeneTuning.BiostatRange.min, GeneTuning.BiostatRange.max);

                if (inBiostatRange)
                {
                    break;
                }
                num++;
            }
            while (num < 50);
            success = num < 50;
            if (PawnSkinColors.SkinColorsFromParents(father, mother).TryRandomElement(out var result))
            {
                tmpGenes.Add(result);
            }
            if (!tmpGenes.Any((GeneDef x) => x.endogeneCategory == EndogeneCategory.HairColor))
            {
                GeneDef geneDef = father?.genes?.GetFirstEndogeneByCategory(EndogeneCategory.HairColor);
                GeneDef geneDef2 = mother?.genes?.GetFirstEndogeneByCategory(EndogeneCategory.HairColor);
                GeneDef result2;
                if (geneDef != null && geneDef2 == null)
                {
                    tmpGenes.Add(geneDef);
                }
                else if (geneDef2 != null && geneDef == null)
                {
                    tmpGenes.Add(geneDef2);
                }
                else if (geneDef != null && geneDef2 != null)
                {
                    tmpGenes.Add(Rand.Bool ? geneDef2 : geneDef);
                }
                else if (DefDatabase<GeneDef>.AllDefs.Where((GeneDef x) => x.endogeneCategory == EndogeneCategory.HairColor).TryRandomElementByWeight((GeneDef x) => x.selectionWeight, out result2))
                {
                    tmpGenes.Add(result2);
                }
            }
            if (!tmpGenes.Contains(GeneDefOf.Inbred) && Rand.Value < PregnancyUtility.InbredChanceFromParents(mother, father, out var _))
            {
                tmpGenes.Add(GeneDefOf.Inbred);
            }

            if (LOGGING_ENABLED)
            {
                Log.Message($"{tmpGenes.Count()} genes passed on");
                Log.Message("Inherited gene list:");
                foreach (GeneDef gene in tmpGenes)
                {
                    Log.Message(gene.defName);
                }

                Log.Message("PREFIX: PregnancyUtility.GetInheritedGenes finishing...");
            }

            __result = tmpGenes;
            return false;
        }

        /// <summary>
        /// Rename a generated pawn (not babies) if they have the crewmate xenotype.
        /// </summary>
        [HarmonyPatch(typeof(PawnGenerator), "GeneratePawn", new Type[] { typeof(PawnGenerationRequest) })]
        public static void Postfix(ref Pawn __result)
        {

            XenotypeDef xenotype = __result.genes?.Xenotype;

            if (!ModsConfig.BiotechActive || xenotype == null)
            {
                if(LOGGING_ENABLED) Log.Message("Biotech inactive or xenotype is null. Skipping POSTFIX of PawnGenerator.GeneratePawn");
                return;
            }

            if (xenotype.defName != "Crewmate")
            {
                if(LOGGING_ENABLED) Log.Message("Pawn is not a crewmate. Skipping POSTFIX of PawnGenerator.GeneratePawn");
                return;
            }

            // Force nick name to be the same as the first name
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
                if(LOGGING_ENABLED) Log.Message("Biotech inactive or xenotype is null. Skipping POSTFIX of PawnGenerator.GeneratePawn");
                return true;
            }

            if (xenotype.defName != "Crewmate")
            {
                if(LOGGING_ENABLED) Log.Message("Pawn is not a crewmate. Skipping POSTFIX of PawnGenerator.GeneratePawn");
                if(LOGGING_ENABLED) Log.Message($"Pawn xenotype: {xenotype.defName}");
                return true;
            }

            __result = generateCrewmateName(pawn);
            return false;
        }

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

            // Force nick name to be the same as the first name
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

                    if(LOGGING_ENABLED) Log.Message($"Added rule: {rule.keyword}->{rule.path}");
                    if(LOGGING_ENABLED) Log.Message(rule.ToString());

                }
            }

            return extraRules;
        }
    }
}