using UnityEngine;
using SimpleActionFramework.Core;

namespace SimpleActionFramework.Actant
{
	[System.Serializable]
	public class SetGravityFactorActant : SingleActant
	{
		public float GravityFactor = 1f;
		
	 	public override void Act(Actor actor, float progress, bool isFirstFrame = false)
	 	{
	 	 	base.Act(actor, progress, isFirstFrame);
	 	 	// Put your code here
	 	}

	    public override void OnStart(Actor actor)
	    {
		    actor.SetGravityFactor(GravityFactor);
	    }

	    public override void OnFinished(Actor actor)
	    {
		    actor.SetGravityFactor();
	    }
	}
}
