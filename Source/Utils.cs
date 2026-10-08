namespace Crewmate
{
    class IntUtils
    {
        public static bool ValueInRange(int val, int min, int max)
        {

            if (val >= min)
            {
                return val <= max;
            }

            return false;
        }

    }
}