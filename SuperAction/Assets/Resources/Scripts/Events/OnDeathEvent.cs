using Proto.EventSystem;
using Proto.PoolingSystem;
using SimpleActionFramework.Core;
using SimpleActionFramework.Implements;
using UnityEngine;

namespace Resources.Scripts.Events
{
	public class OnDeathEvent : IEvent
	{
		public int ActorIndex;
		public int NewDeathCount;
    
		private static TinyObjectPool<OnDeathEvent> pool
			= new TinyObjectPool<OnDeathEvent>();

		public override string ToString()
		{
			return $"OnDeathEvent for Actor {ActorIndex}: \n DeathCount: {NewDeathCount - 1} -> {NewDeathCount}";
		}

		public static OnDeathEvent Create(int idx, int prev)
		{
			var e = pool.GetOrCreate();
            
			e.ActorIndex = idx;
			e.NewDeathCount = prev;
        
			return e;
		}

		public override void Dispose()
		{
			pool.Dispose(this);
		}
	}
}