using Proto.EventSystem;
using Proto.PoolingSystem;
using SimpleActionFramework.Core;

namespace Resources.Scripts.Events
{
	public class OnAttackGuardEvent : IEvent
	{
		public HitMask giverMask;
		public HitMask takerMask;
		public DamageInfo info;
    
		private static TinyObjectPool<OnAttackGuardEvent> pool
			= new TinyObjectPool<OnAttackGuardEvent>();

		public override string ToString()
		{
			return $"OnAttackGuardEvent : \n giver : {giverMask} \n taker : {takerMask} \n" +
			       info;
		}

		public static OnAttackGuardEvent Create(HitMask sender, HitMask getter, DamageInfo info)
		{
			var e = pool.GetOrCreate();

			e.giverMask = sender;
			e.takerMask = getter; 
			e.info = info;
        
			return e;
		}

		public override void Dispose()
		{
			pool.Dispose(this);
		}
	}
}