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

		public override void Act(Actor actor, float progress, bool isFirstFrame = false)
		{
			base.Act(actor, progress, isFirstFrame);
			var deltaProgress = InnerProgress - PrevProgress;

			if (isFirstFrame)
			{
				_transform = actor.transform;
			}

			if (IsRelative)
			{
				var localDirection = actor.transform.TransformDirection(MoveDirection);
				_transform.position += localDirection * deltaProgress;
			}
			else
			{
				_transform.position += MoveDirection * deltaProgress;	
			}
		}
	}
}
