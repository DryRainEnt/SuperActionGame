using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Internal;

public static class Utils
{
    private static StringBuilder _stringBuilderInner = new StringBuilder();
    public static Vector2 UnitProduct(Vector2 A, Vector2 B, float depth = 0)
    {
        return new Vector2(A.x * B.x, A.y * B.y);
    }

    public static Vector3 UnitProduct(Vector3 A, Vector3 B, float depth = 0)
    {
        return new Vector3(A.x * B.x, A.y * B.y, depth);
    }

    public static string BuildString(params string[] str)
    {
        _stringBuilderInner.Clear();
        for (int i = 0; i < str.Length; i++)
        {
            _stringBuilderInner.Append(str[i]);
        }

        return _stringBuilderInner.ToString();
    }

    public static string BuildString(string separator, params object[] list)
    {
        _stringBuilderInner.Clear();
        _stringBuilderInner.Append(list[0]);
        for (int i = 1; i < list.Length; i++)
        {
            _stringBuilderInner.Append(separator);
            _stringBuilderInner.Append(list[i]);
        }

        return _stringBuilderInner.ToString();
    }

    public static string BuildString(char separator, params string[] list)
    {
        _stringBuilderInner.Clear();
        _stringBuilderInner.Append(list[0]);
        for (int i = 1; i < list.Length; i++)
        {
            _stringBuilderInner.Append(separator);
            _stringBuilderInner.Append(list[i]);
        }

        return _stringBuilderInner.ToString();
    }

    public static int AssembleLayerBits(params int[] layers)
    {
        var result = 0;
        for (int i = 0; i < layers.Length; i++)
        {
            result |= 1 << layers[i];
        }

        return result;
    }

    #region DictionaryUtils

    public static object GetDefault(this Dictionary<string, object> data, string key, object defaultValue)
    {
        if (!data.ContainsKey(key))
            return defaultValue;
        else if (data[key].GetType() != defaultValue.GetType())
            return defaultValue;

        return data[key];
    }
    

    #endregion
}