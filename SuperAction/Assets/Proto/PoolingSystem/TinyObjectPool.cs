using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Proto.PoolingSystem
{
    /// <summary>
    /// TinyObjectPool의 원형
    /// </summary>
    public abstract class TinyObjectPool
    {
        /// <summary>
        /// 생성된 모든 풀의 리스트
        /// </summary>
        protected static List<TinyObjectPool> TinyPools = new List<TinyObjectPool>();

        public abstract int ExistCount { get; }
    
        public abstract int PoolCount { get; }
    
        public static void ClearAll()
        {
            for (int i = 0; i < TinyPools.Count; i++)
            {
                TinyPools[i].Clear();
            }
        }
    
        public abstract void Clear();
    }

    /// <summary>
    /// 자주 생성되는 클래스가 메모리를 잡아먹지 않도록 풀링해서 쓴다.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TinyObjectPool<T> : TinyObjectPool where T : class, new()
    {
        private Stack<T> pool = new Stack<T>();

        private int existCount;

        public override int ExistCount => existCount;
        public override int PoolCount => pool.Count;

        public TinyObjectPool()
        {
            TinyPools.Add(this);
            existCount = 0;
        }

        public T GetOrCreate()
        {
            if (PoolCount > 0)
                return pool.Pop();
            else
            {
                existCount++;
                return new T();
            }
        }

        public void Dispose(T obj)
        {
            if (pool.Contains(obj))
                return;
        
            pool.Push(obj);
        }
    
        public override void Clear()
        {
            existCount -= pool.Count;
            pool.Clear();
        }
    }
}