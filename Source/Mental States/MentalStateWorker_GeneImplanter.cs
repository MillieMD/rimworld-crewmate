namespace Verse.AI
{
    public class MentalStateWorker_GeneImplanter : MentalStateWorker
    {
        public override bool StateCanOccur(Pawn pawn)
        {
            if (!base.StateCanOccur(pawn))
            {
                return false;
            }
            return GeneImplanterMentalStateUtility.FindPawnToImplant(pawn) != null;
        }
    }
}
