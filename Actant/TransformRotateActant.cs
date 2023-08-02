using SimpleActionFramework.Core;
using UnityEngine;

namespace SimpleActionFramework.Actant
{
	[System.Serializable]
	public class TransformRotateActant : SingleActant
	{
		public float RotationAngle;
	
		private float _startRotation;

		public override void Act(ActionStateMachine machine, float progress, bool isFirstFrame = false)
		{
			base.Act(machine, progress, isFirstFrame);
			// Put your code here
			if (isFirstFrame)
				_startRotation = machine.Character.transform.eulerAngles.z;

			machine.Character.transform.rotation
				= Quaternion.Euler(0, 0, _startRotation + RotationAngle * progress);
		}
	}
}
