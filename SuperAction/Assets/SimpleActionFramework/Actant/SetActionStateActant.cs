using UnityEngine;
using SimpleActionFramework.Core;

[System.Serializable]
public class SetActionStateActant : SingleActant
{
	public string StateKey;
	
	public override void Act(ActionStateMachine machine, float progress, bool isFirstFrame = false)
	{
	 	base.Act(machine, progress, isFirstFrame);
	    
	    machine.CurrentState.CurrentActantName = "SetStateActant";
	 	
	    Debug.Log($"Set State: {StateKey}");
	    
	    machine.SetState(StateKey);
	}
}
