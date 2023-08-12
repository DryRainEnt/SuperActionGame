using UnityEngine;

namespace Proto.BasicExtensionUtils
{
    public static class IntExtensions
    {
        public static int Clamp(this int value, int max, int min = 0)
        {
            var v = (value >= max) ? max : value;
            return (value <= min) ? min : v;
        }

        public static int Sign(this int value)
        {
            return value < 0 ? -1 : 1;
        }
    }
}
