using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using HarmonyLib;

namespace Crewmate
{

    [StaticConstructorOnStartup]
    // [HarmonyPatch(typeof(PregnancyUtility), nameof(PregnancyUtility.GetInheritedGenes))]
    [HarmonyPatch(typeof(PregnancyUtility), "GetInheritedGenes", new Type[] { typeof(Pawn), typeof(Pawn), typeof(bool) }, new ArgumentType[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Out })]
    class CrewmateInheritancePatch
    {

        static CrewmateInheritancePatch()
        {
            var harmony = new Harmony("serprentino.amongus");
            Log.Message("CrewmateInheritancePatch initialised");
            harmony.PatchAll();
        }

        public static bool Prefix(Pawn father, Pawn mother, out bool success, ref List<GeneDef> __result)
        {

            Log.Message("PREFIX: PregnancyUtility.GetInheritedGenes beginning...");

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

            // TODO tmpGeneShuffled is currently full list of mother and father genes
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


            Log.Message($"{tmpGenes.Count()} genes passed on");
            Log.Message("Full gene list:");
            foreach (GeneDef gene in tmpGenes)
            {
                Log.Message(gene.defName);
            }

            Log.Message("PREFIX: PregnancyUtility.GetInheritedGenes finishing...");

            __result = tmpGenes;
            return false;
        }
    }

}

/*
public static List<GeneDef> GetInheritedGenes(Pawn father, Pawn mother, out bool success)
{
    tmpGenes.Clear();
    tmpGenesShuffled.Clear();
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

    // tmpGeneShuffled -> full list of mother and father genes

    foreach (KeyValuePair<GeneDef, float> geneChance in tmpGeneChances)
    {

        GeneDef gene = geneChance.key;
        bool isLobbyGene = gene.isLobbyGene;
        bool isActive = false;

        // if the gene is one of ours -> make geneChance 100%
        if (gene && isActive)
        {

            tmpGeneChances[gene] = 1.0f;
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
        if (GeneTuning.BiostatRange.Includes(val))
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
    if (!tmpGenes.Contains(GeneDefOf.Inbred) && Rand.Value < InbredChanceFromParents(mother, father, out var _))
    {
        tmpGenes.Add(GeneDefOf.Inbred);
    }
    tmpGeneChances.Clear();
    tmpGenesShuffled.Clear();
    return tmpGenes;
}*/