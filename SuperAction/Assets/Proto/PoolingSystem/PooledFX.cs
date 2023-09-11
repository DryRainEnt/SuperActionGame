using UnityEngine;

namespace Proto.PoolingSystem
{
    public class PooledFX : MonoBehaviour, IPooledObject
    {
        protected string _name;

        protected float _duration;

        public string Name
        {
            get => _name; 
            set => _name = value;
        }

        private void Update()
        {
            if (_duration > 0f)
            {
                _duration -= Time.deltaTime;
            }
            else
            {
                _duration = 9999f;
                Dispose();
            }
        }

        public void OnPooled()
        {
        }

        public void Dispose()
        {
            ObjectPoolController.Dispose(this);
        }
        
        public void Initialize(float duration)
        {
            _duration = duration;
        }
    }
}