﻿using System.Collections;
using System.Collections.Generic;
using SimpleActionFramework.Core;
using UnityEngine;

namespace CMF
{
	//This character movement input class is an example of how to get input from a keyboard to control the character;
    public class CharacterKeyboardInput : CharacterInput
    {
		public string horizontalInputAxis = "Horizontal";
		public string verticalInputAxis = "Vertical";
		public KeyCode jumpKey = KeyCode.K;

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
			if(useRawInput)
				return Input.GetAxisRaw(verticalInputAxis);
			else
				return Input.GetAxis(verticalInputAxis);
		}

		public override bool IsJumpKeyPressed()
		{
			var isKey = GlobalInputController.Instance.GetInput("button2");
			return isKey;
		}

		public override void InputCheck(Actor actor)
		{
			Game.Instance.playerLifeCount = actor.LifeCount;
			Game.Instance.playerKillCount = actor.KillCount;
			
			inputAxis.Value = Vector2.zero;
			foreach (var key in ActionKeys)
			{
				InputCheck(actor, key);
			}
		}

		public void InputCheck(Actor actor, string key)
		{
			var aKey = key switch
			{
				"forward" => actor.IsLeft ? "left" : "right",
				"backward" => actor.IsLeft ? "right" : "left",
				_ => key
			};
            
			var iAxis = inputAxis.Value;
			var cAxis = commandAxis.Value;
			
			switch (aKey)
			{
				case "button1":
					cAxis.x = GlobalInputController.Instance.GetInput(aKey) ? 1 : 0;
					break;
				case "button2":
					cAxis.y = GlobalInputController.Instance.GetInput(aKey) ? 1 : 0;
					break;
				case "right":
					iAxis.x += GlobalInputController.Instance.GetInput(aKey) ? 1 : 0;
					break;
				case "left":
					iAxis.x += GlobalInputController.Instance.GetInput(aKey) ? -1 : 0;
					break;
				case "up":
					iAxis.y += GlobalInputController.Instance.GetInput(aKey) ? 0 : -1;
					break;
				case "down":
					iAxis.y += GlobalInputController.Instance.GetInput(aKey) ? 0 : 1;
					break;
				default:
					break;
			}
			
			commandAxis.Value = cAxis;
			inputAxis.Value = iAxis;
			
			if (GlobalInputController.Instance.GetPressed(aKey))
			{
				actor.RecordedInputs.Add(new InputRecord(Utils.BuildString(key, "+"), Time.realtimeSinceStartup));
				actor.CurrentInputs[key] = true;
			}
			if (GlobalInputController.Instance.GetReleased(aKey))
			{
				actor.RecordedInputs.Add(new InputRecord(Utils.BuildString(key, "-"), Time.realtimeSinceStartup));
				actor.CurrentInputs[key] = false;
			}

			if (actor.RecordedInputs.Count > 100)
			{
				Debug.LogWarning("Something went wrong!!");
				actor.RecordedInputs.Clear();
			}
		}
    }
}
