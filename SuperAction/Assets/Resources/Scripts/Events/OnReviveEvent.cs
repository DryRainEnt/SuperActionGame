using Proto.EventSystem;
using Proto.PoolingSystem;
using SimpleActionFramework.Core;
using SimpleActionFramework.Implements;
using UnityEngine;

namespace Resources.Scripts.Events
{
	public class OnReviveEvent : IEvent
	{
		public int ActorIndex;
		public int DeathCount;
    
		private static TinyObjectPool<OnReviveEvent> pool
			= new TinyObjectPool<OnReviveEvent>();

		public override string ToString()
		{
			return $"OnReviveEvent for Actor {ActorIndex}: \n DeathCount: {DeathCount - 1} -> {DeathCount}";
		}

		public static OnReviveEvent Create(int idx, int prev)
		{
			var e = pool.GetOrCreate();
            
			e.ActorIndex = idx;
			e.DeathCount = prev;
        
			return e;
		}

		public override void Dispose()
		{
			pool.Dispose(this);
		}
	}
}