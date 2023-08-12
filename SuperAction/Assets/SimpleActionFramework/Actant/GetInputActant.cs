using UnityEngine;
using SimpleActionFramework.Core;

[System.Serializable]
public class GetInputActant : SingleActant
{
	public string StateKey;
	public string[] InputCommand;
	
	public override void Act(ActionStateMachine machine, float progress, bool isFirstFrame = false)
	{
	 	base.Act(machine, progress, isFirstFrame);
	 	// Put your code here
	    
	    machine.CurrentState.CurrentActantName = "GetInputActant";
	 	
	    Debug.Log($"Set State: {StateKey}");
	    
	    // Check if the input command is satisfied
	    if (false)
			machine.SetState(StateKey);
	}
}
