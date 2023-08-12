using UnityEngine;

namespace Proto.PoolingSystem
{
    public struct PoolParameters
    {
        private Vector2 _position;
        public Vector2 Position => _position;
        
        private Vector2 _direction;
        public Vector2 Direction => _direction;

        private Transform _parent;
        public Transform Parent => _parent;

        public PoolParameters(Vector2 pos, Transform parent = null)
        {
            _position = pos;
            _direction = Vector2.zero;
            _parent = parent;
        }
        
        public PoolParameters(Vector2 pos)
        {
            _position = pos;
            _direction = Vector2.zero;
            _parent = ObjectPoolController.Self.DefaultParent;
        }
    } 
    public interface IPooledObject
    {
        string Name { get; set; }
        GameObject gameObject { get; }

        void OnPooled();
        void Dispose();
    }

}