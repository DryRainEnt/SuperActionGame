using System.Collections.Generic;
using UnityEngine;

namespace Proto.BasicExtensionUtils
{
    public static class GenericExtensions
    {
        #region DictionaryUtils

        public static object GetDefault(this Dictionary<string, object> data, string key, object defaultValue)
        {
            if (!data.ContainsKey(key))
                return defaultValue;
            return data[key].GetType() != defaultValue.GetType() ? defaultValue : data[key];
        }
        
        #endregion
    }
}
