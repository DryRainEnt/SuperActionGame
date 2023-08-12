using UnityEngine;
using SimpleActionFramework.Core;

[System.Serializable]
public class SetFrameActant : SingleActant
{
	public string FrameKey;
	
	public override void Act(ActionStateMachine machine, float progress, bool isFirstFrame = false)
	{
	 	base.Act(machine, progress, isFirstFrame);
	 	// Put your code here
	    
	    machine.CurrentState.CurrentActantName = "SetFrameActant";
	 	
	    Debug.Log($"Set Frame: {FrameKey}");
	    
	    // Set the frame
	    machine.Actor.SetFrame(machine.Actor.FrameDataSet.FrameData[FrameKey]);
	}
}
