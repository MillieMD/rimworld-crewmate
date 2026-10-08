using Verse;

namespace Crewmate{

    // <GeneDef>
    //    <li class="Crewmate.LobbyGene">
    //        <isLobbyGene>true<isLobbyGene>
    //    </li>
    // </GeneDef>
    public class InheritanceSettings : DefModExtension
    {
        // Forces 100% inheritance rate if gene is an active gene of either parent, otherwise do not inherit at all
        public bool forceActiveInheritance = false;

    }
}