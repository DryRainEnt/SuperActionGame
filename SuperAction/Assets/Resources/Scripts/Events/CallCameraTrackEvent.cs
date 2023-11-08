using Proto.EventSystem;
using Proto.PoolingSystem;
using SimpleActionFramework.Core;
using SimpleActionFramework.Implements;
using UnityEngine;

namespace Resources.Scripts.Events
{
	public class CallCameraTrackEvent : IEvent
	{
		public Transform target;
    
		private static TinyObjectPool<CallCameraTrackEvent> pool
			= new TinyObjectPool<CallCameraTrackEvent>();

		public override string ToString()
		{
			return $"Camera Track Event: {target}";
		}

		public static CallCameraTrackEvent Create(Transform target)
		{
			var e = pool.GetOrCreate();
            
			e.target = target;
        
			return e;
		}

		public override void Dispose()
		{
			pool.Dispose(this);
		}
	}
}