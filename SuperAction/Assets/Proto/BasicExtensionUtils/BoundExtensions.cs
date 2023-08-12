using UnityEngine;

namespace Proto.BasicExtensionUtils
{

    public static class BoundExtensions
    {
        public static float GetArea(this Bounds bounds)
        {
            return bounds.size.x.Abs() * bounds.size.y.Abs();
        }

        public static Bounds GetOverlappingBound(Bounds A, Bounds B)
        {
            Vector3 AA = new Vector3(Mathf.Max(A.min.x, B.min.x), Mathf.Max(A.min.y, B.min.y));
            Vector3 BB = new Vector3(Mathf.Min(A.max.x, B.max.x), Mathf.Min(A.max.y, B.max.y));

            Bounds bounds = new Bounds();

            if (AA.x >= BB.x || AA.y >= BB.y)
                return bounds;

            bounds.SetMinMax(AA, BB);
            return bounds;
        }

        public static Vector2 GetMinDirectionPoint(Bounds bound, Vector2 move)
        {
            return new Vector2();
        }
    }
}