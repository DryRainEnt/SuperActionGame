using Proto.EventSystem;
using Proto.PoolingSystem;
using SimpleActionFramework.Core;
using SimpleActionFramework.Implements;
using UnityEngine;

namespace Resources.Scripts.Events
{
	public class OnHealthUpdatedEvent : IEvent
	{
		public int ActorIndex;
		public float PreviousHP;
		public float CurrentHP;
    
		private static TinyObjectPool<OnHealthUpdatedEvent> pool
			= new TinyObjectPool<OnHealthUpdatedEvent>();

		public override string ToString()
		{
			return $"OnHealthUpdatedEvent for Actor {ActorIndex}: \n HP: {PreviousHP} -> {CurrentHP}";
		}

		public static OnHealthUpdatedEvent Create(int idx, float prev, float curr)
		{
			var e = pool.GetOrCreate();
            
			e.ActorIndex = idx;
			e.PreviousHP = prev;
			e.CurrentHP = curr;
        
			return e;
		}

		public override void Dispose()
		{
			pool.Dispose(this);
		}
	}
}