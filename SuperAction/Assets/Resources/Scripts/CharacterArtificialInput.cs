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

		private bool _isJumpKeyPressed = false;

		private float _innerTimer = 0;
		
		private AIState _aiState;
		public AIState AiState => _aiState ??= new AIState(GetComponent<Actor>());

		private bool _useAI = true;
		public bool UseAI => _useAI;

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
			var aKey = key switch
			{
				"forward" => actor.IsLeft ? "left" : "right",
				"backward" => actor.IsLeft ? "right" : "left",
				_ => key
			};

			if (!_useAI)
			{
				inputAxis = Vector2.zero;
				commandAxis = Vector2.zero;
				return;
			}
			
			if (key == "button1")
			{
				_innerTimer += Time.deltaTime;

				prevInputAxis = inputAxis;
				prevCommandAxis = commandAxis;

				AiState.Execute(ref inputAxis, ref commandAxis);
			}

			var plus = Utils.BuildString(aKey, "+");
			var minus = Utils.BuildString(aKey, "-");
			int condition = 0;
			
			switch (aKey)
			{
                case "button1":
	                if (commandAxis.x > 0 && prevCommandAxis.x.Abs() <= Constants.Epsilon)
		                condition = 1;
	                if (prevCommandAxis.x > 0 && commandAxis.x.Abs() <= Constants.Epsilon)
		                condition = -1;
	                break;
                case "button2": 
	                if (commandAxis.y > 0 && prevCommandAxis.y.Abs() <= Constants.Epsilon)
	                {
		                condition = 1;
		                _isJumpKeyPressed = true;
	                }
	                if (prevCommandAxis.y > 0 && commandAxis.y.Abs() <= Constants.Epsilon)
	                {
		                condition = -1;
		                _isJumpKeyPressed = false;
	                }
	                break;
                case "button3": break;
                case "button4": break;
                case "right": 
	                if (inputAxis.x > 0 && prevInputAxis.x.Abs() <= Constants.Epsilon)
		                condition = 1;
	                if (prevInputAxis.x > 0 && inputAxis.x.Abs() <= Constants.Epsilon)
		                condition = -1;
	                break;
                case "left":
	                if (inputAxis.x < 0 && prevInputAxis.x.Abs() <= Constants.Epsilon)
		                condition = 1;
	                if (prevInputAxis.x < 0 && inputAxis.x.Abs() <= Constants.Epsilon)
		                condition = -1;
	                break;
                case "up":
	                if (inputAxis.y > 0 && prevInputAxis.y.Abs() <= Constants.Epsilon)
		                condition = 1;
	                if (prevInputAxis.y > 0 && inputAxis.y.Abs() <= Constants.Epsilon)
		                condition = -1;
	                break;
                case "down": 
	                if (inputAxis.y < 0 && prevInputAxis.y.Abs() <= Constants.Epsilon)
		                condition = 1;
	                if (prevInputAxis.y < 0 && inputAxis.y.Abs() <= Constants.Epsilon)
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
		private Actor _parent;
		private Actor _enemy;

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
		private Vector2 DirectionRaw => _enemy.Position - _parent.Position;
		private Vector2 Direction => DirectionRaw.Clamp(-Vector2.one, Vector2.one);
		private float Distance => DirectionRaw.magnitude;

		private Vector2 _lastInput;
		private Vector2 _lastCommand;
		
		private float RandomToken => UnityEngine.Random.Range(0f, 1f);

		public AIState(Actor parent)
		{
			_parent = parent;
			_enemy = Game.Instance.GetEnemy(_parent.ActorIndex);
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
					
					if (_innerTimer > 0.3f)
					{
						inputAxis.x = 0;
						inputAxis.y = 0;
						commandAxis.x = 0;
						commandAxis.y = 0;
					}
					if (_innerTimer > 0.4f)
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