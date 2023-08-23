using Proto.EventSystem;
using Proto.PoolingSystem;
using SimpleActionFramework.Core;
using SimpleActionFramework.Implements;
using UnityEngine;

namespace Resources.Scripts.Events
{
	public class OnAttackHitEvent : IEvent
	{
		public HitMask giverMask;
		public HitMask takerMask;
		public DamageInfo info;
    
		private static TinyObjectPool<OnAttackHitEvent> pool
			= new TinyObjectPool<OnAttackHitEvent>();

		public override string ToString()
		{
			return $"OnAttackHitEvent : \n giver : {giverMask} \n taker : {takerMask} \n" +
			       info;
		}

		public static OnAttackHitEvent Create(HitMask sender, HitMask getter, DamageInfo info)
		{
			var e = pool.GetOrCreate();

			e.giverMask = sender;
			e.takerMask = getter; 
			e.info = info;
        
			return e;
		}

		public static OnAttackHitEvent Create(HitData data)
		{
			var e = pool.GetOrCreate();

			e.giverMask = data.GiverMask;
			e.takerMask = data.ReceiverMask; 
			e.info = data.DamageInfo;
        
			return e;
		}

		public override void Dispose()
		{
			pool.Dispose(this);
		}
	}
}