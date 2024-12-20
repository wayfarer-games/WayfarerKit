using UnityEngine;

namespace WayfarerKit.Extensions
{
    public static class MathExtensions
    {
        public static float GetPercentage(float left, float right, float value)
        {
            if (value <= left)
                return 0;
            
            if (value >= right)
                return 1;
            
            return (value - left) / (right - left);
        }
    }
}