namespace Crewmate
{
    class IntUtils
    {
        /// <summary>
        /// Clone of Verse InRange method
        /// </summary>
        public static bool ValueInRange(int val, int min, int max)
        {
            if (val >= min) return val <= max;
            
            return false;
        }
    }
}