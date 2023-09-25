using CMF;
using SimpleActionFramework.Core;
using UnityEngine;

namespace Resources.Scripts
{
	public class CharacterArtificialInput : CharacterInput
	{
		public Vector2 movementDirection = Vector2.zero;

		public string horizontalInputAxis = "Horizontal";
		public string verticalInputAxis = "Vertical";
		public KeyCode jumpKey = KeyCode.Space;

		//If this is enabled, Unity's internal input smoothing is bypassed;
		public bool useRawInput = true;

		public override float GetHorizontalMovementInput()
		{
			if(useRawInput)
				return Input.GetAxisRaw(horizontalInputAxis);
			else
				return Input.GetAxis(horizontalInputAxis);
		}

		public override float GetVerticalMovementInput()
		{
			return 0;
		}

		// 점프 인풋을 발생시키는 위치
		public override bool IsJumpKeyPressed()
		{
			return Input.GetKey(jumpKey);
		}
        
		// 커맨드 인풋을 발생시키는 위치
		public override void InputCheck(Actor actor, string key)
		{
			var aKey = key switch
			{
				"forward" => actor.IsLeft ? "left" : "right",
				"backward" => actor.IsLeft ? "right" : "left",
				_ => key
			};
            
			if (GlobalInputController.Instance.GetPressed(aKey))
			{
				actor.RecordedInputs.Add(new InputRecord(Utils.BuildString(key, "+"), Time.realtimeSinceStartup));
			}
			if (GlobalInputController.Instance.GetReleased(aKey))
			{
				actor.RecordedInputs.Add(new InputRecord(Utils.BuildString(key, "-"), Time.realtimeSinceStartup));
			}

			if (actor.RecordedInputs.Count > 100)
			{
				Debug.LogWarning("Something went wrong!!");
				actor.RecordedInputs.Clear();
			}
		}
	}
}