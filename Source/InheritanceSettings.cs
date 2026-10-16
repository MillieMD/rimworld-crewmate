using Verse;

namespace Crewmate{
    
    // Usage: 
    // <GeneDef>
    //    ...
    //    <li class="Crewmate.LobbyGene">
    //        <isLobbyGene>true<isLobbyGene>
    //    </li>
    // </GeneDef>
    public class InheritanceSettings : DefModExtension
    {
        /// <summary>
        /// Forces 100% inheritance rate of gene is an active gene of either parent, otherwise do not inherit at all
        /// </summary>
        public bool forceActiveInheritance = false;

    }
}