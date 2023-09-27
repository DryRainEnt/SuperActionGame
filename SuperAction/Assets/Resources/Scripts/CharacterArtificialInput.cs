using System;
using CMF;
using Proto.BasicExtensionUtils;
using SimpleActionFramework.Core;
using Sirenix.Utilities;
using UnityEngine;
using Constants = Proto.BasicExtensionUtils.Constants;

namespace Resources.Scripts
{
	public class CharacterArtificialInput : CharacterInput
	{
		public Vector2 movementDirection = Vector2.zero;

		public Vector2 prevInputAxis;
		public Vector2 inputAxis;
		
		public Vector2 prevCommandAxis;
		public Vector2 commandAxis;

		private bool _isJumpKeyPressed = false;

		private float _innerTimer = 0;
		
		private AIState _aiState;

		private bool _useAI = false;
		public bool UseAI => _useAI;

		private void OnEnable()
		{
			_aiState = new AIState(transform);
		}

		public override float GetHorizontalMovementInput()
		{
			return inputAxis.x;
		}

		public override float GetVerticalMovementInput()
		{
			return inputAxis.y;
		}

		// 점프 인풋을 발생시키는 위치
		public override bool IsJumpKeyPressed()
		{
			return _isJumpKeyPressed;
		}
        
		// 커맨드 인풋을 발생시키는 위치
		public override void InputCheck(Actor actor, string key)
		{
			_innerTimer += Time.deltaTime;
			
			if (GlobalInputController.Instance.GetPressed("debug"))
			{
				_useAI = !_useAI;
			}

			if (!_useAI)
			{
				inputAxis = Vector2.zero;
				commandAxis = Vector2.zero;
				return;
			}
			
			prevInputAxis = inputAxis;
			prevCommandAxis = commandAxis;
			
			_aiState.Execute(ref inputAxis, ref commandAxis);
            
			
			if (inputAxis.x > 0 && prevInputAxis.x.Abs() <= Constants.Epsilon)
			{
				actor.RecordedInputs.Add(new InputRecord("right+", Time.realtimeSinceStartup));
			}
			if (prevInputAxis.x > 0 && inputAxis.x.Abs() <= Constants.Epsilon)
			{
				actor.RecordedInputs.Add(new InputRecord("right-", Time.realtimeSinceStartup));
			}
			if (inputAxis.x < 0 && prevInputAxis.x.Abs() <= Constants.Epsilon)
			{
				actor.RecordedInputs.Add(new InputRecord("left+", Time.realtimeSinceStartup));
			}
			if (prevInputAxis.x < 0 && inputAxis.x.Abs() <= Constants.Epsilon)
			{
				actor.RecordedInputs.Add(new InputRecord("left-", Time.realtimeSinceStartup));
			}
            
			
			if (inputAxis.y > 0 && prevInputAxis.y.Abs() <= Constants.Epsilon)
			{
				actor.RecordedInputs.Add(new InputRecord("up+", Time.realtimeSinceStartup));
			}
			if (prevInputAxis.y > 0 && inputAxis.y.Abs() <= Constants.Epsilon)
			{
				actor.RecordedInputs.Add(new InputRecord("up-", Time.realtimeSinceStartup));
			}
			if (inputAxis.y < 0 && prevInputAxis.y.Abs() <= Constants.Epsilon)
			{
				actor.RecordedInputs.Add(new InputRecord("down+", Time.realtimeSinceStartup));
			}
			if (prevInputAxis.y < 0 && inputAxis.y.Abs() <= Constants.Epsilon)
			{
				actor.RecordedInputs.Add(new InputRecord("down-", Time.realtimeSinceStartup));
			}
			
			if (commandAxis.x > 0 && prevCommandAxis.x.Abs() <= Constants.Epsilon)
			{
				actor.RecordedInputs.Add(new InputRecord("button1+", Time.realtimeSinceStartup));
			}
			if (prevCommandAxis.x > 0 && commandAxis.x.Abs() <= Constants.Epsilon)
			{
				actor.RecordedInputs.Add(new InputRecord("button1-", Time.realtimeSinceStartup));
			}
			
			if (commandAxis.y > 0 && prevCommandAxis.y.Abs() <= Constants.Epsilon)
			{
				_isJumpKeyPressed = true;
				actor.RecordedInputs.Add(new InputRecord("jump+", Time.realtimeSinceStartup));
			}
			if (prevCommandAxis.y > 0 && commandAxis.y.Abs() <= Constants.Epsilon)
			{
				_isJumpKeyPressed = false;
				actor.RecordedInputs.Add(new InputRecord("jump-", Time.realtimeSinceStartup));
			}
		}
	}
	
	
	
	public enum AIStatePreset
	{
		Parry,		// Attack
		Combo1,		// UpAttack
		Combo2,		// Jump
		Combo3,		// JumpAttack
		Combo4,		// JumpAttackUp
		Combo5,		// JumpAttackDown
		Fork,		// ForwardAttack
		Approach,	// Approach
		RunAway,	// RunAway
		Counter,	// DownAttack
		Guard,		// Guard
		Avoid,		// Dodge
		Wait,		// Wait
	}

	public class AIState
	{
		private Transform _parent;

		private AIStatePreset _currentState;

		private AIStatePreset CurrentState
		{
			get => _currentState;
			set
			{
				// Debug.Log($"[{_innerTimer:0.0000}]state reset: {_currentState} => {value} | {Distance}\n" +
				//           $"Input: {_lastInput}\n" + $"Command: {_lastCommand}\n");
				_innerTimer = 0f;
				_currentState = value;
			}
		}

		private float _innerTimer;
		private Vector2 DirectionRaw => (Game.Instance.Player.Position - (Vector2)_parent.position);
		private Vector2 Direction => DirectionRaw.Clamp(-Vector2.one, Vector2.one);
		private float Distance => DirectionRaw.magnitude;

		private Vector2 _lastInput;
		private Vector2 _lastCommand;
		
		private float RandomToken => UnityEngine.Random.Range(0f, 1f);

		public AIState(Transform parent)
		{
			_parent = parent;
		}
		
		public void Execute(ref Vector2 inputAxis, ref Vector2 commandAxis)
		{
			_innerTimer += Time.deltaTime;
			
			_lastInput = inputAxis;
			_lastCommand = commandAxis;
			
			switch (CurrentState)
			{
				case AIStatePreset.Parry:
					commandAxis.x = 1;
					if (_innerTimer > 0.3f)
					{
						inputAxis.x = 0;
						inputAxis.y = 0;
						commandAxis.x = 0;
						commandAxis.y = 0;
					}
					if (_innerTimer > 0.5f)
						CurrentState = AIStatePreset.Wait;
					break;
				case AIStatePreset.Combo1:
					inputAxis.x = 0;
					inputAxis.y = 1;
					
					if (_innerTimer > 0.1f)
					{
						commandAxis.x = 1;
						commandAxis.y = 0;
					}
					
					if (_innerTimer > 0.4f)
					{
						inputAxis.x = 0;
						inputAxis.y = 0;
						commandAxis.x = 0;
						commandAxis.y = 0;
					}
					if (_innerTimer > 0.5f)
						CurrentState = AIStatePreset.Combo2;
					break;
				case AIStatePreset.Combo2:
					inputAxis.x = 0;
					inputAxis.y = 0;
					commandAxis.x = 0;
					commandAxis.y = 1;

					if (Direction.x.Abs() > 6)
						CurrentState = AIStatePreset.Approach;
                    
					if (_innerTimer > 0.2f)
					{
						inputAxis.x = 0;
						inputAxis.y = 0;
						commandAxis.x = 0;
						commandAxis.y = 0;
					}
					if (_innerTimer > 0.24f)
						CurrentState = AIStatePreset.Combo3;
					break;
				case AIStatePreset.Combo3:
					inputAxis.x = 0;
					inputAxis.y = 0;
					commandAxis.x = 1;
					commandAxis.y = 0;

					if (_innerTimer > 0.1f)
					{
						inputAxis.x = 0;
						inputAxis.y = 0;
						commandAxis.x = 0;
						commandAxis.y = 0;
					}
					if (_innerTimer > 0.16f)
						CurrentState = AIStatePreset.Combo4;
					break;
				case AIStatePreset.Combo4:
					inputAxis.x = 0;
					inputAxis.y = 1;
					if (_innerTimer > 0.06f)
					{
						commandAxis.x = 1;
						commandAxis.y = 0;
					}

					if (_innerTimer > 0.4f)
					{
						inputAxis.x = 0;
						inputAxis.y = 0;
						commandAxis.x = 0;
						commandAxis.y = 0;
					}
					if (_innerTimer > 0.5f)
						CurrentState = AIStatePreset.Combo5;
					break;
				case AIStatePreset.Combo5:
					inputAxis.x = 0;
					inputAxis.y = -1;
					if (_innerTimer > 0.1f)
					{
						commandAxis.x = 1;
						commandAxis.y = 0;
					}

					if (_innerTimer > 0.6f)
					{
						inputAxis.x = 0;
						inputAxis.y = 0;
						commandAxis.x = 0;
						commandAxis.y = 0;
						CurrentState = AIStatePreset.Avoid;
					}
					break;
				case AIStatePreset.Fork:
					inputAxis.x = Direction.x;
					inputAxis.y = 0;
					commandAxis.x = 1;
					commandAxis.y = 0;

					if (_innerTimer > 0.6f)
					{
						inputAxis.x = 0;
						inputAxis.y = 0;
						commandAxis.x = 0;
						commandAxis.y = 0;
						CurrentState = Distance < 4f ? AIStatePreset.Combo1 : AIStatePreset.Avoid;
					}
					break;
				case AIStatePreset.Approach:
					inputAxis.x = Direction.x;
					inputAxis.y = 0;
					commandAxis.x = 0;
					commandAxis.y = 0;
					
					if (Distance < 6f)
					{
						inputAxis.x = 0;
						inputAxis.y = 0;
						commandAxis.x = 0;
						commandAxis.y = 0;
						CurrentState = AIStatePreset.Fork;
					}
					break;
				case AIStatePreset.RunAway:
					inputAxis.x = -Direction.x;
					inputAxis.y = 0;
					commandAxis.x = 0;
					commandAxis.y = 0;
					
					if (Distance > 8f)
					{
						inputAxis.x = 0;
						inputAxis.y = 0;
						commandAxis.x = 0;
						commandAxis.y = 0;
						CurrentState = AIStatePreset.Approach;
					}
					else if (Distance > 6f)
					{
						inputAxis.x = 0;
						inputAxis.y = 0;
						commandAxis.x = 0;
						commandAxis.y = 0;
						CurrentState = RandomToken < 0.7f ? AIStatePreset.Fork : AIStatePreset.Counter;
					}
					else if (Distance < 4f && _innerTimer > 1f)
					{
						inputAxis.x = 0;
						inputAxis.y = 0;
						commandAxis.x = 0;
						commandAxis.y = 0;
						CurrentState = RandomToken < 0.7f ? AIStatePreset.Avoid : AIStatePreset.Counter;
					}
					break;
				case AIStatePreset.Counter:
					inputAxis.x = 0;
					inputAxis.y = -1;
					commandAxis.x = 1;
					commandAxis.y = 0;
					if (_innerTimer > 0.5f)
					{
						inputAxis.x = 0;
						inputAxis.y = 0;
						commandAxis.x = 0;
						commandAxis.y = 0;
						CurrentState = Distance < 1.5f ? AIStatePreset.Combo2 : AIStatePreset.RunAway;
					}
					break;
				case AIStatePreset.Guard:
					inputAxis.x = 0;
					inputAxis.y = -1;
					
					if (Distance > 8f)
					{
						inputAxis.x = 0;
						inputAxis.y = 0;
						commandAxis.x = 0;
						commandAxis.y = 0;
						CurrentState = AIStatePreset.Approach;
					}
					else if (Distance > 6f)
					{
						inputAxis.x = 0;
						inputAxis.y = 0;
						commandAxis.x = 0;
						commandAxis.y = 0;
						CurrentState = RandomToken < 0.7f ? AIStatePreset.Fork : AIStatePreset.Parry;
					}
					else if (Distance < 2f && _innerTimer > 1f)
					{
						inputAxis.x = 0;
						inputAxis.y = 0;
						commandAxis.x = 0;
						commandAxis.y = 0;
						CurrentState = RandomToken < 0.7f ? AIStatePreset.Avoid : AIStatePreset.Counter;
					}
					break;
				case AIStatePreset.Avoid:
					inputAxis.x = -Direction.x;
					inputAxis.y = -1;
					commandAxis.x = 0;
					commandAxis.y = 1;
					
					if (_innerTimer > 0.3f)
					{
						inputAxis.x = 0;
						inputAxis.y = 0;
						commandAxis.x = 0;
						commandAxis.y = 0;
						CurrentState = Distance < 1.5f ? AIStatePreset.Guard : AIStatePreset.RunAway;
					}
					break;
				case AIStatePreset.Wait:
					if (_innerTimer < 1f)
						break;
					
					if (Distance is > 6f and < 8f)
						CurrentState = AIStatePreset.Fork;
					else if (Distance > 8f)
						CurrentState = AIStatePreset.Approach;
					else if (Distance < 3f)
						CurrentState = AIStatePreset.Parry;
					else
						CurrentState = AIStatePreset.Combo1;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}