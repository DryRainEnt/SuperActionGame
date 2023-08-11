using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SimpleActionFramework.Core
{
	[System.Serializable]
	[CreateAssetMenu(fileName = "NewActionState", menuName = "Simple Action Framework/Create New Action State", order = 2)]
	public class ActionState : ScriptableObject
	{
		[SerializeReference]
		public List<SingleActant> Actants = new List<SingleActant>();

		public SerializedDictionary<string, string> Data { get; set; }

		public ushort TotalDuration => (ushort)(Actants.Count > 0 ? Actants.Max(actant => actant.EndFrame) : 0);

		private float _innerTimer;
		public ushort CurrentFrame => (ushort)Mathf.FloorToInt(_innerTimer * 30f);
		private ushort _previousFrame = 0;
		
		public bool IsFinished => _innerTimer >= TotalDuration;

		public string ReservedState;
		public string CurrentActantName = "NullActant";

		public void ResetState(ActionStateMachine machine = null, SerializedDictionary<string, string> data = null)
		{
			Data = data ?? new SerializedDictionary<string, string>();
			_innerTimer = 0f;
			_previousFrame = CurrentFrame;

			foreach (var act in Actants)
			{
				act.State = ActantState.NotStarted;
				act.Init(machine);
			}
		}
		
		public void UpdateState(ActionStateMachine machine, float dt)
		{
			CurrentActantName = "NullActant";

			foreach (var act in Actants)
			{
				if (act.State == ActantState.Finished || act.StartFrame > CurrentFrame)
					continue;
				
				var progress = Mathf.Clamp01((_innerTimer - act.StartFrame / Constants.DefaultActionFrameRate)
				                             / ((float)act.Duration / Constants.DefaultActionFrameRate));
				var isFirst = _previousFrame <= act.StartFrame
				              && act.StartFrame <= CurrentFrame
				              && act.State == ActantState.NotStarted;

				if (isFirst) act.State = ActantState.Running;
				if (act.EndFrame <= _previousFrame) act.State = ActantState.Finished;

				act.Act(machine, progress, isFirst);
			}
			
			_previousFrame = CurrentFrame;
			_innerTimer += dt;
		}
	}
}
