using System;
using CMF;
using Proto.BasicExtensionUtils;
using SimpleActionFramework.Core;
using Sirenix.Utilities;
using UnityEngine;
using Constants = Proto.BasicExtensionUtils.Constants;

namespace Resources.Scripts
{
	public class NetworkPlayerInput : CharacterInput
	{
		public Vector2 movementDirection = Vector2.zero;

		private bool _isJumpKeyPressed = false;

		private float _innerTimer = 0;

		public override float GetHorizontalMovementInput()
		{
			return inputAxis.Value.x;
		}

		public override float GetVerticalMovementInput()
		{
			return inputAxis.Value.y;
		}

		// 점프 인풋을 발생시키는 위치
		public override bool IsJumpKeyPressed()
		{
			return _isJumpKeyPressed;
		}
        

		public override void InputCheck(Actor actor)
		{
			foreach (var key in ActionKeys)
			{
				InputCheck(actor, key);
			}
		}

		// 커맨드 인풋을 발생시키는 위치
		public void InputCheck(Actor actor, string key)
		{
			var aKey = key switch
			{
				"forward" => actor.IsLeft ? "left" : "right",
				"backward" => actor.IsLeft ? "right" : "left",
				_ => key
			};
			
			if (key == "button1")
			{
				_innerTimer += Time.deltaTime;

				prevInputAxis = inputAxis;
				prevCommandAxis = commandAxis;
			}

			var plus = Utils.BuildString(aKey, "+");
			var minus = Utils.BuildString(aKey, "-");
			int condition = 0;
			
			switch (aKey)
			{
                case "button1":
	                if (commandAxis.Value.x > 0 && prevCommandAxis.Value.x.Abs() <= Constants.Epsilon)
		                condition = 1;
	                if (prevCommandAxis.Value.x > 0 && commandAxis.Value.x.Abs() <= Constants.Epsilon)
		                condition = -1;
	                break;
                case "button2": 
	                if (commandAxis.Value.y > 0 && prevCommandAxis.Value.y.Abs() <= Constants.Epsilon)
	                {
		                condition = 1;
		                _isJumpKeyPressed = true;
	                }
	                if (prevCommandAxis.Value.y > 0 && commandAxis.Value.y.Abs() <= Constants.Epsilon)
	                {
		                condition = -1;
		                _isJumpKeyPressed = false;
	                }
	                break;
                case "button3": break;
                case "button4": break;
                case "right": 
	                if (inputAxis.Value.x > 0 && prevInputAxis.Value.x.Abs() <= Constants.Epsilon)
		                condition = 1;
	                if (prevInputAxis.Value.x > 0 && inputAxis.Value.x.Abs() <= Constants.Epsilon)
		                condition = -1;
	                break;
                case "left":
	                if (inputAxis.Value.x < 0 && prevInputAxis.Value.x.Abs() <= Constants.Epsilon)
		                condition = 1;
	                if (prevInputAxis.Value.x < 0 && inputAxis.Value.x.Abs() <= Constants.Epsilon)
		                condition = -1;
	                break;
                case "up":
	                if (inputAxis.Value.y > 0 && prevInputAxis.Value.y.Abs() <= Constants.Epsilon)
		                condition = 1;
	                if (prevInputAxis.Value.y > 0 && inputAxis.Value.y.Abs() <= Constants.Epsilon)
		                condition = -1;
	                break;
                case "down": 
	                if (inputAxis.Value.y < 0 && prevInputAxis.Value.y.Abs() <= Constants.Epsilon)
		                condition = 1;
	                if (prevInputAxis.Value.y < 0 && inputAxis.Value.y.Abs() <= Constants.Epsilon)
		                condition = -1;
	                break;
			}
			
			switch (condition)
			{
				case > 0:
					actor.RecordedInputs.Add(new InputRecord(plus, Time.realtimeSinceStartup));
					actor.CurrentInputs[key] = true;
					break;
				case < 0:
					actor.RecordedInputs.Add(new InputRecord(minus, Time.realtimeSinceStartup));
					actor.CurrentInputs[key] = false;
					break;
			}
		}
	}
}