using System;
using UnityEngine.Serialization;

namespace SimpleActionFramework.Utility
{
	[Serializable]
	public struct CombinedIdKey
	{
		public int machineId;
		public int actionId;
		public int actantId;
		
		public CombinedIdKey(int machineId, int actionId, int actantId)
		{
			this.machineId = machineId;
			this.actionId = actionId;
			this.actantId = actantId;
		}
	}
}