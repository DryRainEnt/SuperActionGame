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

		public override void Act(Actor actor, float progress, bool isFirstFrame = false)
		{
			base.Act(actor, progress, isFirstFrame);
			var deltaProgress = InnerProgress - PrevProgress;
			
			if (isFirstFrame)
			{
				_transform = actor.transform;
			}

			_transform.Rotate(0, 0, RotationAngle * deltaProgress);
		}
	}
}
