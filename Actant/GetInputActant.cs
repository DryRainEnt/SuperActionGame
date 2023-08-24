using UnityEngine;
using SimpleActionFramework.Core;

[System.Serializable]
public class GetInputActant : SingleActant
{
	public string StateKey;
	public ConditionState[] ConditionStates;
	
	public override void Act(Actor actor, float progress, bool isFirstFrame = false)
	{
	 	base.Act(actor, progress, isFirstFrame);

	    if (ConditionStates.Length == 0)
	    {
		    actor.ActionStateMachine.SetState(StateKey);
		    return;
	    }

	    var machine = actor.ActionStateMachine;
	    var state = ConditionStates[0].ConditionCheck(machine);
	    
	    // Check if the conditions are satisfied
	    for (var index = 1; index < ConditionStates.Length; index++)
	    {
		    var condition = ConditionStates[index];
		    if (condition.JointType == JointType.And)
			    state &= condition.ConditionCheck(machine);
		    if (condition.JointType == JointType.Or)
			    state |= condition.ConditionCheck(machine);
		    if (condition.JointType == JointType.Xor)
			    state ^= condition.ConditionCheck(machine);
	    }
	    
	    if (state)
		    actor.ActionStateMachine.SetState(StateKey);
	}
}
