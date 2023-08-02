using UnityEngine;
using SimpleActionFramework.Core;

[System.Serializable]
public class TransformMoveActant : SingleActant
{
	public Vector3 MoveDirection;
	
	private Vector3 _startPosition;
	
	public override void Act(ActionStateMachine machine, float progress, bool isFirstFrame = false)
	{
		base.Act(machine, progress, isFirstFrame);
		// Put your code here
		if (isFirstFrame)
			_startPosition = machine.Character.transform.position;

		machine.Character.transform.position
			= Vector3.Lerp(_startPosition, _startPosition + MoveDirection, progress);
	}
}
