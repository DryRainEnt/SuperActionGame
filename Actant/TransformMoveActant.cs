using SimpleActionFramework.Core;
using UnityEngine;

namespace SimpleActionFramework.Actant
{
	[System.Serializable]
	public class TransformMoveActant : SingleActant
	{
		public Vector3 MoveDirection;
		public bool IsRelative;
	
		private Vector3 _startPosition;
		private Transform _transform;

		public override void Act(ActionStateMachine machine, float progress, bool isFirstFrame = false)
		{
			base.Act(machine, progress, isFirstFrame);

			if (isFirstFrame)
			{
				_transform = machine.Character.transform;
				_startPosition = _transform.position;
			}

			if (IsRelative)
			{
				var localDirection = machine.Character.transform.TransformDirection(MoveDirection);
				machine.Character.transform.position
					= Vector3.Lerp(_startPosition, _startPosition + localDirection, InnerProgress);
			}
			else
			{
				machine.Character.transform.position
					= Vector3.Lerp(_startPosition, _startPosition + MoveDirection, InnerProgress);	
			}
			
			
			machine.CurrentState.CurrentActantName = "TransformMoveActant";
		}
	}
}
