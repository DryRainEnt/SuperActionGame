using UnityEngine;

namespace SimpleActionFramework.Core
{
    public enum InterpolationType
    {
        Linear,
        EaseInQuad,
        EaseOutQuad,
        EaseInOutQuad,
        EaseInCubic,
        EaseOutCubic,
        EaseInOutCubic,
        Constant,
    }
    
    /// <summary>
    /// Interpolates values 
    /// </summary>
    public static class Interpolations
    {
        public static float Interpolate(this InterpolationType type, float t, float a = 0f, float b = 1f)
        {
            return type switch
            {
                InterpolationType.Constant => Constant(t, a, b),
                InterpolationType.Linear => Linear(t, a, b),
                InterpolationType.EaseInQuad => EaseInQuad(t, a, b),
                InterpolationType.EaseOutQuad => EaseOutQuad(t, a, b),
                InterpolationType.EaseInOutQuad => EaseInOutQuad(t, a, b),
                InterpolationType.EaseInCubic => EaseInCubic(t, a, b),
                InterpolationType.EaseOutCubic => EaseOutCubic(t, a, b),
                InterpolationType.EaseInOutCubic => EaseInOutCubic(t, a, b),
                _ => Linear(t, a, b)
            };
        }

        private static float Constant(float t, float a = 0f, float b = 1f)
        {
            return 1;
        }

        private static float Linear(float t, float a = 0f, float b = 1f)
        {
            return (b - a) * t + a;
        }
        
        private static float EaseInQuad(float t, float a = 0f, float b = 1f)
        {
            return (b - a) * t * t + a;
        }
        
        private static float EaseOutQuad(float t, float a = 0f, float b = 1f)
        {
            return -(b - a) * t * (t - 2) + a;
        }
        
        private static float EaseInOutQuad(float t, float a = 0f, float b = 1f)
        {
            if ((t /= 0.5f) < 1) return (b - a) / 2 * t * t + a;
            return -(b - a) / 2 * (--t * (t - 2) - 1) + a;
        }
        
        private static float EaseInCubic(float t, float a = 0f, float b = 1f)
        {
            return (b - a) * t * t * t + a;
        }
        
        private static float EaseOutCubic(float t, float a = 0f, float b = 1f)
        {
            return (b - a) * ((t -= 1) * t * t + 1) + a;
        }
        
        private static float EaseInOutCubic(float t, float a = 0f, float b = 1f)
        {
            if ((t /= 0.5f) < 1) return (b - a) / 2 * t * t * t + a;
            return (b - a) / 2 * ((t -= 2) * t * t + 2) + a;
        }
    }
}
