using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
    /// <summary>
    /// Clone of CompAbilityEffect_Teleport - except in PreCastAction where the sound effect from Props is used, to allow the Vent sound effect to play
    /// </summary>
    public class CompAbilityEffect_Lie : CompAbilityEffect
    {
        const float AFFECTED_CHANCE = 0.4f;
        const int MIN_POS = 10;
        const int MAX_POS = 30;
        const double BACKFIRE_CHANCE = 0.1f;
        const int BACKFIRE_VALUE = -25;

        static Random random = new Random();
        static FactionManager factionManager = Find.FactionManager;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);

            IEnumerable<Faction> factions = factionManager.AllFactionsVisible;
            Faction player_faction = factionManager.OfPlayer;
            int goodWillValue;

            foreach (Faction faction in factions)
            {
                if (random.NextDouble() > AFFECTED_CHANCE){
                    continue;
                }

                goodWillValue = random.NextDouble() < BACKFIRE_CHANCE ? BACKFIRE_VALUE : random.Next(MIN_POS, MAX_POS);

                faction.TryAffectGoodwillWith(player_faction, goodWillValue);
            }
        }
    }
}
