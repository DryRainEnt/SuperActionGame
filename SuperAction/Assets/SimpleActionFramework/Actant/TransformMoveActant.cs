using SimpleActionFramework.Core;
using UnityEngine;

namespace SimpleActionFramework.Actant
{
	[System.Serializable]
	public class TransformMoveActant : SingleActant
	{
		public Vector3 MoveDirection;
		public bool IsRelative;
	
		private Transform _transform;

		public override void Act(ActionStateMachine machine, float progress, bool isFirstFrame = false)
		{
			base.Act(machine, progress, isFirstFrame);
			var deltaProgress = InnerProgress - PrevProgress;

			if (isFirstFrame)
			{
				_transform = machine.Actor.transform;
			}

			if (IsRelative)
			{
				var localDirection = machine.Actor.transform.TransformDirection(MoveDirection);
				_transform.position += localDirection * deltaProgress;
			}
			else
			{
				_transform.position += MoveDirection * deltaProgress;	
			}
			
			
			machine.CurrentState.CurrentActantName = "TransformMoveActant";
		}
	}
}
