using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SimpleActionFramework.Core
{
	[System.Serializable]
	public class ActionState : ScriptableObject
	{
		public List<SingleActant> Actants;

		public ushort TotalDuration => Actants.Max(actant => actant.EndFrame);
	}
}
