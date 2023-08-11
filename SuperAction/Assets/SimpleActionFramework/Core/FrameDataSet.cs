using System.Collections.Generic;
using UnityEngine;

namespace SimpleActionFramework.Core
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "NewFrameData", menuName = "Simple Action Framework/Create New Frame Data", order = 1)]
    public class FrameDataSet : ScriptableObject
    {
        public SerializedDictionary<string, FrameData> FrameData = new SerializedDictionary<string, FrameData>();
        
        public void Add(string key, FrameData data)
        {
            FrameData.Add(new KeyValuePair<string, FrameData>(key, data));
        }
    }
}
