﻿using CMF;
using UnityEngine;

namespace Resources.Scripts
{
	public class CharacterArtificialInput : CharacterInput
	{
		public Vector2 movementDirection = Vector2.zero;

		public override float GetHorizontalMovementInput()
		{
			return movementDirection.x;
		}

		public override float GetVerticalMovementInput()
		{
			return movementDirection.y;
		}

		public override bool IsJumpKeyPressed()
		{
			//TODO: Implement jump key
			return false;
		}
	}
}