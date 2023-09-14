using System.Collections.Generic;
using UnityEngine;

namespace Proto.PoolingSystem
{
    public class ObjectPoolController : MonoBehaviour
    {
        private static ObjectPoolController self;
        public static ObjectPoolController Self => self ? self : (self = FindObjectOfType<ObjectPoolController>().Initialize());
        private Dictionary<string, ObjectPool> _poolList;

        private Transform _defaultParent;
        public Transform DefaultParent => _defaultParent;

        private ObjectPoolController Initialize()
        {
            _poolList = new Dictionary<string, ObjectPool>();
            _defaultParent = transform.Find("PoolParentContainer") ?? new GameObject("PoolParentContainer").transform;;
            _defaultParent.SetParent(transform);

            return this;
        }

        public static ObjectPool GetOrCreate(string poolName, string bundle)
        {
            if (Self._poolList.TryGetValue(poolName, out var create))
                return create;
            
            var prefab = UnityEngine.Resources.Load<GameObject>(Utils.BuildString("Prefabs/", bundle, "/", poolName));

            if (prefab != null)
            {
                var poolObj = new GameObject(prefab.name);
                poolObj.transform.SetParent(Self.transform);
                var pool = poolObj.AddComponent<ObjectPool>().Initialize(prefab.name, prefab);
                Self._poolList.Add(prefab.name, pool);

                return pool;
            }

            return null;
        }


        public static IPooledObject InstantiateObject(string poolName, PoolParameters param)
        {
            return Self._poolList[poolName].Instantiate(param);
        }

        public static void Dispose(IPooledObject obj)
        {
            Self._poolList[obj.Name].Dispose(obj);
        }

        public static void DisposeAllActivePool()
        {
            var list = Self._defaultParent.GetComponentsInChildren<IPooledObject>();
            foreach (var pooled in list)
            {
                pooled.Dispose();
            }
        }
    }

}