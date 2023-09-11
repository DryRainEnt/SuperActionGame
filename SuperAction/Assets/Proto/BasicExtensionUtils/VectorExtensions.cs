using UnityEngine;

namespace Proto.BasicExtensionUtils
{

    public static class VectorExtensions
    {
        public static Vector2 ToVector2(this Vector3 vec)
        {
            return new Vector2(vec.x, vec.y);
        }

        public static Vector3 ToVector3(this Vector2 vec, float depth = 0f)
        {
            return new Vector3(vec.x, vec.y, depth);
        }

        public static Vector2 GetXFlat(this Vector2 vec, float y = 0)
        {
            return new Vector2(vec.x, y);
        }

        public static Vector2 GetYFlat(this Vector2 vec, float x = 0)
        {
            return new Vector2(x, vec.y);
        }

        public static Vector3 GetXFlat(this Vector3 vec, float y = 0)
        {
            return new Vector3(vec.x, y, vec.z);
        }

        public static Vector3 GetYFlat(this Vector3 vec, float x = 0)
        {
            return new Vector3(x, vec.y, vec.z);
        }

        public static Vector3 GetOnes(this Vector3 vec)
        {
            return new Vector3(Mathf.Sign(vec.x), Mathf.Sign(vec.y), vec.z);
        }


        public static Vector2 doFlipX(this Vector2 vec, bool flip)
        {
            return flip ? new Vector2(-vec.x, vec.y) : vec;
        }

        public static Vector2 FlipX(this Vector2 vec)
        {
            return new Vector2(-vec.x, vec.y);
        }

        public static Vector2 FlipY(this Vector2 vec)
        {
            return new Vector2(vec.x, -vec.y);
        }

        public static Vector3 FlipX(this Vector3 vec)
        {
            return new Vector3(-vec.x, vec.y, vec.z);
        }

        public static Vector3 FlipY(this Vector3 vec)
        {
            return new Vector3(vec.x, -vec.y, vec.z);
        }

        public static Vector3 AlignVector(this Vector3 vec)
        {
            return new Vector3(Mathf.Round(vec.x), Mathf.Round(vec.y), vec.z);
        }

        public static Vector3 AlignXVector(this Vector3 vec)
        {
            return new Vector3(Mathf.Round(vec.x), vec.y, vec.z);
        }

        public static Vector3 AlignYVector(this Vector3 vec)
        {
            return new Vector3(vec.x, Mathf.Round(vec.y), vec.z);
        }

        public static Vector3 PixelizeVector(this Vector3 vec)
        {
            return new Vector3(vec.x.PixelizedFloat(), vec.y.PixelizedFloat(), vec.z);
        }

        public static Vector2 PixelizeVector(this Vector2 vec)
        {
            return new Vector2(vec.x.PixelizedFloat(), vec.y.PixelizedFloat());
        }

        public static Vector3 GetFilteredVector(this Vector3 vec)
        {
            return new Vector3(vec.x.GetFilteredFloat(), vec.y.GetFilteredFloat(), vec.z);
        }

        public static Vector2 GetFilteredVector(this Vector2 vec)
        {
            return new Vector2(vec.x.GetFilteredFloat(), vec.y.GetFilteredFloat());
        }

        public static Vector2 Abs(this Vector2 vec)
        {
            return new Vector2(vec.x.Abs(), vec.y.Abs());
        }

        public static Vector3 Abs(this Vector3 vec)
        {
            return new Vector3(vec.x.Abs(), vec.y.Abs(), vec.z);
        }

    }
}