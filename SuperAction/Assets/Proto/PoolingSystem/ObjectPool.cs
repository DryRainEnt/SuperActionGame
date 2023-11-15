using System.Collections.Generic;
using UnityEngine;

namespace Proto.PoolingSystem
{
    public class ObjectPool : MonoBehaviour
    {
        private string _name;

        public string Name
        {
            get => _name;
            set => _name = value;
        }

        private GameObject _prefab;

        public GameObject Prefab
        {
            get => _prefab;
            set => _prefab = value;
        }

        private Stack<IPooledObject> _pool;

        public Stack<IPooledObject> Pool => _pool;

        public ObjectPool Initialize(string poolName, GameObject asset)
        {
            _name = poolName;
            _prefab = asset;
            _pool = new Stack<IPooledObject>();

            return this;
        }

        private IPooledObject GetOrCreate()
        {
            if (_pool.Count > 0)
            {
                //Debug.Log(_pool.Peek().Name + " pulled out!");
                return _pool.Pop();
            }
            
            var obj = Instantiate(_prefab).GetComponent<IPooledObject>();

            return obj;
        }

        public IPooledObject Instantiate(PoolParameters param)
        {
            var obj = GetOrCreate();
            obj.Name = _name;
            obj.gameObject.SetActive(true);
            if (param.Parent is not null)
                obj.gameObject.transform.SetParent(param.Parent);
            obj.gameObject.transform.position = param.Position;
            
            obj.OnPooled();
            //Debug.Log(obj.Name + " Instantiated!");
            
            return obj;
        }

        public void Dispose(IPooledObject obj)
        {
            obj.gameObject.transform.SetParent(transform);
            obj.gameObject.SetActive(false);
            obj.gameObject.transform.position = new Vector2(999, 999);
            _pool.Push(obj);
            
            //Debug.Log(obj.Name + " Disposed!");
        }
    }
}