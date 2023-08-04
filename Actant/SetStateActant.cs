using UnityEngine;
using SimpleActionFramework.Core;

[System.Serializable]
public class SetStateActant : SingleActant
{
	public string StateKey;
	
	public override void Act(ActionStateMachine machine, float progress, bool isFirstFrame = false)
	{
	 	base.Act(machine, progress, isFirstFrame);
	 	
	    Debug.Log($"Set State: {StateKey}");
	    
	    machine.SetState(StateKey);
	    
	    machine.CurrentState.CurrentActantName = "SetStateActant";
	}
}
