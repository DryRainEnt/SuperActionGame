﻿using System;
using System.Collections.Generic;
using System.IO;
using CMF;
using Proto.BasicExtensionUtils;
using Resources.Scripts.Core;
using SimpleActionFramework.Core;
using Sirenix.Utilities;
using UnityEngine;
using Constants = Proto.BasicExtensionUtils.Constants;

namespace Resources.Scripts
{
	public class NeuralNetworkInput : CharacterInput
	{
		public CustomNeuralNetwork NeuralNetwork => NetworkManager.Instance.NeuralNetwork;
		
		public Vector2 movementDirection = Vector2.zero;

		private bool _isJumpKeyPressed = false;

		private float _innerTimer = 0;

		private bool _useAI = false;
		public bool UseAI => _useAI;

		private void OnEnable()
		{
			Game.Instance.Learner = GetComponent<Actor>();
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
			
			var aKey = key switch
			{
				"forward" => actor.IsLeft ? "left" : "right",
				"backward" => actor.IsLeft ? "right" : "left",
				_ => key
			};

			if (GlobalInputController.Instance.GetPressed("debug"))
			{
				_useAI = !_useAI;
				actor.Color = UseAI ? Color.white : Color.grey;
			}

			if (!_useAI)
			{
				inputAxis = Vector2.zero;
				commandAxis = Vector2.zero;
				return;
			}
			
			prevInputAxis = inputAxis;
			prevCommandAxis = commandAxis;
			
			intention = NeuralNetwork.ForwardA(new FrameData(Time.frameCount).ActorDataSet);
			var action = NN_ActionMap.FindMostSimilarObject(intention);
            var input = NN_ActionMap.ActionInputPairs.Find(x => x.ActionName == action.ActionName).InputLayer;
            
            inputAxis = new Vector2(input.x, input.y);
            commandAxis = new Vector2(input.z, input.w);

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
		                condition = 1;
	                if (prevCommandAxis.y > 0 && commandAxis.y.Abs() <= Constants.Epsilon)
		                condition = -1;
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

	public static class NN_ActionMap
	{
		// intend : [Attack, Approach, Avoid, Guard, Wait]
		public static List<NN_ActionIntendPair> ActionIntendPairs = new ()
		{
			new NN_ActionIntendPair("Idle", new []{0f, 0f, 0f, 0f, 1f}),
			new NN_ActionIntendPair("Approach", new []{0f, 1f, -0.5f, -0.5f, 0f}),
			new NN_ActionIntendPair("Retreat", new []{-1f, 0f, 0.5f, 0f, 0f}),
			new NN_ActionIntendPair("Dodge", new []{-1f, -0.5f, 1f, 0f, 0f}),
			new NN_ActionIntendPair("Guard", new []{-0.5f, 0f, -0.2f, 1f, 0.5f}),
			new NN_ActionIntendPair("Jump", new []{0.2f, 0.1f, 0.3f, -0.7f, -0.5f}),
			new NN_ActionIntendPair("5A", new []{0.3f, 0f, -0.2f, 0.4f, -0.2f}),
			new NN_ActionIntendPair("6A", new []{0.6f, 0.7f, -0.7f, -0.4f, -0.6f}),
			new NN_ActionIntendPair("2A", new []{0.2f, 0f, -0.3f, 0.3f, -0.5f}),
			new NN_ActionIntendPair("8A", new []{0.8f, 0.1f, -1f, -0.8f, -0.7f}),
			new NN_ActionIntendPair("J5A", new []{0.5f, 0.2f, 0f, -0.3f, -0.3f}),
			new NN_ActionIntendPair("J6A", new []{0.4f, 0.6f, 0f, -0.2f, -0.5f}),
			new NN_ActionIntendPair("J2A", new []{0.7f, 0.4f, -0.7f, -0.5f, -0.6f}),
			new NN_ActionIntendPair("J8A", new []{0.9f, 0.1f, -0.4f, -0.8f, -0.9f}),
		};

		public static List<NN_ActionInputPair> ActionInputPairs = new()
		{
			new NN_ActionInputPair("Idle", new Vector4(0f, 0f, 0f, 0f)),
			new NN_ActionInputPair("Approach", new Vector4(1f, 0f, 0f, 0f)),
			new NN_ActionInputPair("Retreat", new Vector4(-1f, 0f, 0f, 0f)),
			new NN_ActionInputPair("Dodge", new Vector4(0f, -1f, 0f, 1f)),
			new NN_ActionInputPair("Guard", new Vector4(0f, -1f, 0f, 0f)),
			new NN_ActionInputPair("Jump", new Vector4(0f, 0f, 0f, 1f)),
			new NN_ActionInputPair("5A", new Vector4(0f, 0f, 1f, 0f)),
			new NN_ActionInputPair("6A", new Vector4(1f, 0f, 1f, 0f)),
			new NN_ActionInputPair("2A", new Vector4(0f, -1f, 1f, 0f)),
			new NN_ActionInputPair("8A", new Vector4(0f, 1f, 1f, 0f)),
			new NN_ActionInputPair("J5A", new Vector4(0f, 0f, 1f, 0f)),
			new NN_ActionInputPair("J6A", new Vector4(1f, 0f, 1f, 0f)),
			new NN_ActionInputPair("J2A", new Vector4(0f, -1f, 1f, 0f)),
			new NN_ActionInputPair("J8A", new Vector4(0f, 1f, 1f, 0f)),
		};
		
		public static NN_ActionIntendPair FindMostSimilarObject(float[] targetFactors)
		{
			NN_ActionIntendPair mostSimilarObject = null;
			float maxSimilarity = float.MinValue;

			foreach (NN_ActionIntendPair obj in ActionIntendPairs)
			{
				float dotProduct = 0;
				float magnitudeA = 0;
				float magnitudeB = 0;
            
				for (int i = 0; i < targetFactors.Length; i++)
				{
					dotProduct += targetFactors[i] * obj.IntendLayer[i];
					magnitudeA += Mathf.Pow(targetFactors[i], 2);
					magnitudeB += Mathf.Pow(obj.IntendLayer[i], 2);
				}

				float similarity = dotProduct / (Mathf.Sqrt(magnitudeA) * Mathf.Sqrt(magnitudeB));

				if (similarity > maxSimilarity)
				{
					maxSimilarity = similarity;
					mostSimilarObject = obj;
				}
			}
        
			return mostSimilarObject ?? ActionIntendPairs[0];
		}

	}
	
	public class NN_ActionIntendPair
	{
		public string ActionName;
		public float[] IntendLayer;
		
		public NN_ActionIntendPair(string actionName, float[] intendLayer)
		{
			ActionName = actionName;
			IntendLayer = intendLayer;
		}
	}
	
	public class NN_ActionInputPair
	{
		public string ActionName;
		public Vector4 InputLayer;
		
		public NN_ActionInputPair(string actionName, Vector4 input)
		{
			ActionName = actionName;
			InputLayer = input;
		}
	}
}