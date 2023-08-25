using UnityEngine;
using SimpleActionFramework.Core;

[System.Serializable]
public class SetVelocityActant : SingleActant
{
	public Vector2 velocity;
	public override void Act(Actor actor, float progress, bool isFirstFrame = false)
	{
	 	base.Act(actor, progress, isFirstFrame);
	 	// Put your code here
	    
	    actor.SetVelocity(velocity * InterpolationType.Interpolate(progress));
	}
}
