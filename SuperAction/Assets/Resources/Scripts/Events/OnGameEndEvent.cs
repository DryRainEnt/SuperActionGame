using Proto.EventSystem;
using Proto.PoolingSystem;
using SimpleActionFramework.Core;
using SimpleActionFramework.Implements;
using UnityEngine;

namespace Resources.Scripts.Events
{
	public class OnGameEndEvent : IEvent
	{
		public int WinnerIndex;
    
		private static TinyObjectPool<OnGameEndEvent> pool
			= new TinyObjectPool<OnGameEndEvent>();

		public override string ToString()
		{
			return $"OnReviveEvent for Actor {WinnerIndex}";
		}

		public static OnGameEndEvent Create(int idx)
		{
			var e = pool.GetOrCreate();
            
			e.WinnerIndex = idx;
        
			return e;
		}

		public override void Dispose()
		{
			pool.Dispose(this);
		}
	}
}