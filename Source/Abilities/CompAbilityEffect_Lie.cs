using System;
using System.Collections.Generic;
using Multiplayer.API;
using Verse;

namespace RimWorld
{
    public class CompAbilityEffect_Lie : CompAbilityEffect
    {
        const float AFFECTED_CHANCE = 0.4f;
        const int MIN_POS = 10;
        const int MAX_POS = 30;
        const double BACKFIRE_CHANCE = 0.1f;
        const int BACKFIRE_VALUE = -25;

        [SyncMethod]
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);

            FactionManager factionManager = Find.FactionManager;

            IEnumerable<Faction> factions = factionManager.AllFactionsVisible;
            Faction player_faction = factionManager.OfPlayer;
            int goodWillValue;

            foreach (Faction faction in factions)
            {
                if (Rand.Value > AFFECTED_CHANCE){
                    continue;
                }

                goodWillValue =  Rand.Value < BACKFIRE_CHANCE ? BACKFIRE_VALUE : Rand.RangeInclusive(MIN_POS, MAX_POS);
                faction.TryAffectGoodwillWith(player_faction, goodWillValue);
            }
        }
    }
}
