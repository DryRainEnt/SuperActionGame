using Proto.BasicExtensionUtils;
using SimpleActionFramework.Core;
using UnityEngine;

namespace SimpleActionFramework.Actant
{
	[System.Serializable]
	public class SetVelocityActant : SingleActant
	{
		public Vector2 Velocity;
		public bool RelativeToActor = true;
		
		public override void Act(Actor actor, float progress, bool isFirstFrame = false)
		{
			base.Act(actor, progress, isFirstFrame);
			// Put your code here
	    
			actor.SetVelocity(((RelativeToActor && actor.IsLeft) ? Velocity.FlipX() : Velocity) * InnerProgress);
		}
	}
}
