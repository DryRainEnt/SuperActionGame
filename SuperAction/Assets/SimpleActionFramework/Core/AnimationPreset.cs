using UnityEngine;

namespace SimpleActionFramework.Core
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "NewAnimPreset", menuName = "Simple Action Framework/Create New Animation Preset", order = 5)]
    public class AnimationPreset : ScriptableObject
    {
        public SerializedDictionary<string, int> AnimSequence = new SerializedDictionary<string, int>();
    }
}
