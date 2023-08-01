using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SimpleActionFramework.Core
{
    [System.Serializable]
    public class SerializedDictionary<TKey, TValue> : ISerializationCallbackReceiver, IDictionary<TKey, TValue>
    {
        [SerializeField]
        private List<TKey> keys = new List<TKey>();
        [SerializeField]
        private List<TValue> values = new List<TValue>();
    
        private Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();
    

        void IDictionary<TKey, TValue>.Add(TKey key, TValue value)
        {
            if (keys.Contains(key))
                return;
        
            keys.Add(key);
            values.Add(value);
            dictionary.Add(key, value);
        }

        public bool ContainsKey(TKey key)
        {
            return keys.Contains(key);
        }
    
        public bool ContainsValue(TValue value)
        {
            return value is not null && values.Contains(value);
        }
        
        public TKey GetKey(TValue value)
        {
            return keys[values.IndexOf(value)];
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            ((IDictionary<TKey, TValue>)dictionary).Add(item);
        }

        public void Clear()
        {
            keys.Clear();
            values.Clear();
            dictionary.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return ContainsKey(item.Key) && ContainsValue(item.Value);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            ((IDictionary<TKey, TValue>)dictionary).CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return ((IDictionary<TKey, TValue>)dictionary).Remove(item);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return dictionary.TryGetValue(key, out value);
        }

        public int Count => keys?.Count ?? 0;
        public bool IsReadOnly { get; }

        public bool Update(TKey key, TValue value)
        {
            if (!keys.Contains(key)) return false;
        
            values[keys.FindIndex(x => x.Equals(key))] = value;
            return true;
        }
    
        public bool Remove(TKey key)
        {
            if (!keys.Contains(key)) return false;
        
            values.RemoveAt(keys.FindIndex(x => x.Equals(key)));
            keys.Remove(key);
            return true;
        }
    
        public TValue this[TKey key]
        {
            get => dictionary[key];
            set => dictionary[key] = value;
        }

        public ICollection<TKey> Keys => dictionary.Keys;
        public ICollection<TValue> Values => dictionary.Values;

        // Unity는 이 메서드를 사용하여 직렬화를 수행합니다.
        public void OnBeforeSerialize()
        {
            keys.Clear();
            values.Clear();

            foreach (KeyValuePair<TKey, TValue> pair in this)
            {
                keys.Add(pair.Key);
                values.Add(pair.Value);
            }
        }

        // Unity는 이 메서드를 사용하여 역직렬화를 수행합니다.
        public void OnAfterDeserialize()
        {
            dictionary.Clear();

            for (int i = 0; i != System.Math.Min(keys.Count, values.Count); i++)
                dictionary[keys[i]] = values[i];
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return dictionary.GetEnumerator();
        }

        public IEnumerator GetEnumerator()
        {
            return dictionary.GetEnumerator();
        }
    }
}
