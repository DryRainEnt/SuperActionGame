using SimpleActionFramework.Core;
using UnityEngine;

namespace SimpleActionFramework.Actant
{
	[System.Serializable]
	public class TransformRotateActant : SingleActant
	{
		public float RotationAngle;
	
		private float _startRotation;
		private Transform _transform;

		public override void Act(ActionStateMachine machine, float progress, bool isFirstFrame = false)
		{
			base.Act(machine, progress, isFirstFrame);
			
			if (isFirstFrame)
			{
				_transform = machine.Character.transform;
				_startRotation = _transform.eulerAngles.z;
			}

			machine.Character.transform.rotation
				= Quaternion.Euler(0, 0, _startRotation + RotationAngle * InnerProgress);
			
			machine.CurrentState.CurrentActantName = "TransformRotateActant";
		}
	}
}
