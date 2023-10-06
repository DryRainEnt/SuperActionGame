using UnityEngine;
using SimpleActionFramework.Core;

namespace SimpleActionFramework.Actant
{
	[System.Serializable]
	public class ToggleInput : SingleActant
	{
		public bool useCharacterInput;
		
	 	public override void Act(Actor actor, float progress, bool isFirstFrame = false)
	 	{
	 	 	base.Act(actor, progress, isFirstFrame);
	 	 	// Put your code here
		    
		    actor.ToggleCharacterInput(useCharacterInput);
	 	}
	}
}
