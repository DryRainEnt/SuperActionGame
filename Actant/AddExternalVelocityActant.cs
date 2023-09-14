using Proto.BasicExtensionUtils;
using UnityEngine;
using SimpleActionFramework.Core;

namespace SimpleActionFramework.Actant
{
	[System.Serializable]
	public class AddExternalVelocityActant : SingleActant
	{
		public Vector2 Velocity;
		public bool RelativeToActor = true;
		
	 	public override void Act(Actor actor, float progress, bool isFirstFrame = false)
	 	{
	 	 	base.Act(actor, progress, isFirstFrame);
	 	 	// Put your code here
	    
		    actor.AddExternalVelocity(((RelativeToActor && actor.IsLeft) ? Velocity.FlipX() : Velocity) * InnerProgress);
	 	}
	}
}
