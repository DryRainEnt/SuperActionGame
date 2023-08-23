using UnityEngine;
using SimpleActionFramework.Core;

[System.Serializable]
public class GetInputActant : SingleActant
{
	public string StateKey;
	public string[] InputCommand;
	
	public override void Act(Actor actor, float progress, bool isFirstFrame = false)
	{
	 	base.Act(actor, progress, isFirstFrame);
	 	// Put your code here
	    
	    actor.ActionStateMachine.CurrentState.CurrentActantName = "GetInputActant";
	 	
	    Debug.Log($"Set State: {StateKey}");
	    
	    // Check if the input command is satisfied
	    if (false)
		    actor.ActionStateMachine.SetState(StateKey);
	}
}
