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

		private float _innerTimer;
		public ushort CurrentFrame => (ushort)(_innerTimer * 30);
		private ushort _previousFrame = 0;
		
		public bool IsFinished => _innerTimer >= TotalDuration;

		public string ReservedState;
		
		public void ResetState()
		{
			_innerTimer = 0f;
		}
		
		public void UpdateState(float dt)
		{
			_previousFrame = CurrentFrame;
			_innerTimer += dt;

			foreach (var act in Actants
				         .Where(act => act.StartFrame <= CurrentFrame
				                       && act.EndFrame > CurrentFrame)
				         .Where(act => !act.UsedOnce
				                       || act.StartFrame > _previousFrame))
			{
				act.Act(dt);
			}
		}
	}
}
