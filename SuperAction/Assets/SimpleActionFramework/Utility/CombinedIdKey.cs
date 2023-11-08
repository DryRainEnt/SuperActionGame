using System;
using UnityEngine;

namespace SimpleActionFramework.Utility
{
	[Serializable]
	public struct CombinedIdKey
	{
		public int machineId;
		public int actionId;
		public int actantId;
		public int timeStamp;
		
		public CombinedIdKey(int machineId, int actionId, int actantId)
		{
			this.machineId = machineId;
			this.actionId = actionId;
			this.actantId = actantId;
			this.timeStamp = Time.frameCount;
		}
		
		public bool IsSame(CombinedIdKey key)
		{
			return machineId == key.machineId && 
			       actionId == key.actionId && 
			       actantId == key.actantId &&
			       timeStamp == key.timeStamp;
		}
		
		public bool IsSameAction(CombinedIdKey key)
		{
			return machineId == key.machineId && 
			       actionId == key.actionId &&
			       timeStamp == key.timeStamp;
		}
		
		public bool IsSameMove(CombinedIdKey key)
		{
			return
				actionId == key.actionId &&
				actantId == key.actantId;
		}
	}
}