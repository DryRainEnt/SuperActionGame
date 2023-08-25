using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SimpleActionFramework.Core;

[System.Serializable]
public class LoopPointActant : SingleActant
{
	public List<ConditionState> ConditionStates = new List<ConditionState>();
	
	public override void Act(Actor actor, float progress, bool isFirstFrame = false)
	{
	 	base.Act(actor, progress, isFirstFrame);
	 	// Put your code here

	    if (progress >= 1f)
	    {
		    if (ConditionStates.Count == 0)
		    {
			    actor.ActionStateMachine.RewindState(StartFrame);
			    return;
		    }

		    var state = ConditionStates[0].ConditionCheck(actor.ActionStateMachine);
	    
		    // Check if the conditions are satisfied
		    for (var index = 1; index < ConditionStates.Count; index++)
		    {
			    var condition = ConditionStates[index];
			    if (condition.JointType == JointType.And)
				    state &= condition.ConditionCheck(actor.ActionStateMachine);
			    if (condition.JointType == JointType.Or)
				    state |= condition.ConditionCheck(actor.ActionStateMachine);
			    if (condition.JointType == JointType.Xor)
				    state ^= condition.ConditionCheck(actor.ActionStateMachine);
		    }
	    
		    if (state)
			    actor.ActionStateMachine.RewindState(StartFrame);
	    }
	}

	public override void OnFinished()
	{
	}
}
