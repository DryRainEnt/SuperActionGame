using System.Collections.Generic;
using UnityEngine;

namespace Proto.PoolingSystem
{
    public class ObjectPoolController : MonoBehaviour
    {
        public static ObjectPoolController Self;
        private Dictionary<string, ObjectPool> _poolList;

        private Transform _defaultParent;
        public Transform DefaultParent => _defaultParent;

        private void Awake()
        {
            Self = this;
            
            _poolList = new Dictionary<string, ObjectPool>();
            _defaultParent = transform.Find("PoolParentContainer") ?? new GameObject("PoolParentContainer").transform;;
            _defaultParent.SetParent(transform);
        }

        public ObjectPool GetOrCreate(string poolName, string bundle)
        {
            if (_poolList.ContainsKey(poolName))
                return _poolList[poolName];
            
            var prefab = UnityEngine.Resources.Load<GameObject>(Utils.BuildString("Prefabs/", bundle, "/", poolName));

            if (prefab != null)
            {
                var poolObj = new GameObject(prefab.name);
                poolObj.transform.SetParent(transform);
                var pool = poolObj.AddComponent<ObjectPool>().Initialize(prefab.name, prefab);
                _poolList.Add(prefab.name, pool);

                return pool;
            }

            return null;
        }


        public IPooledObject Instantiate(string poolName, PoolParameters param)
        {
            return _poolList[poolName].Instantiate(param);
        }

        public void Dispose(IPooledObject obj)
        {
            _poolList[obj.Name].Dispose(obj);
        }

        public void DisposeAllActivePool()
        {
            var list = _defaultParent.GetComponentsInChildren<IPooledObject>();
            foreach (var pooled in list)
            {
                pooled.Dispose();
            }
        }
    }

}