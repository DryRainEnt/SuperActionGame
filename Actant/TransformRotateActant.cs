using SimpleActionFramework.Core;
using UnityEngine;

namespace SimpleActionFramework.Actant
{
	[System.Serializable]
	public class TransformRotateActant : SingleActant
	{
		public float RotationAngle;
		public InterpolationType InterpolationType;
	
		private float _startRotation;
		private Transform _transform;

		public override void Act(ActionStateMachine machine, float progress, bool isFirstFrame = false)
		{
			var prevProgress = InnerProgress;
			InnerProgress = InterpolationType.Interpolate(progress);
			var deltaProgress = InnerProgress - prevProgress;
			
			if (isFirstFrame)
			{
				_transform = machine.Actor.transform;
			}

			_transform.Rotate(0, 0, RotationAngle * deltaProgress);
			
			machine.CurrentState.CurrentActantName = "TransformRotateActant";
		}
	}
}
