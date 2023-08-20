using SimpleActionFramework.Core;
using UnityEngine;

namespace SimpleActionFramework.Actant
{
	[System.Serializable]
	public class SetSpriteActant : SingleActant
	{
		public Sprite sprite;
	
		public override void Act(ActionStateMachine machine, float progress, bool isFirstFrame = false)
		{
			base.Act(machine, progress, isFirstFrame);
			// Put your code here
	    
			machine.CurrentState.CurrentActantName = "SetFrameActant";
	 	
			Debug.Log($"Set Sprite : {sprite.name}");
	    
			// Set the frame
			machine.Actor.SetSprite(sprite);
		}
	}
}
