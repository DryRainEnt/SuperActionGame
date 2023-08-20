using System.Collections.Generic;
using SimpleActionFramework.Core;
using UnityEngine;

namespace SimpleActionFramework.Actant
{
	[System.Serializable]
	public class SetActionStateActant : SingleActant
	{
		public List<string> ConditionKeys = new List<string>();
		public List<string> ConditionValues = new List<string>();
		public string StateKey;
	
		public override void Act(ActionStateMachine machine, float progress, bool isFirstFrame = false)
		{
			base.Act(machine, progress, isFirstFrame);
	    
			machine.CurrentState.CurrentActantName = "SetStateActant";
	 	
			Debug.Log($"Set State: {StateKey}");
	    
			machine.SetState(StateKey);
		}
	}
}
