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
		
		[SerializeReference]
		public List<SingleActant> actantWrapper = new List<SingleActant>();
		[SerializeField]
		public ActantWrapper actantWrapperB = new ActantWrapper();

		public SerializedDictionary<string, object> Data { get; set; }

		public int TotalDuration => (Actants.Count > 0 ? Actants.Max(actant => actant.EndFrame) : 0);

		private float _innerTimer;
		public int CurrentFrame => Mathf.FloorToInt(_innerTimer * 30f);
		private int _previousFrame = 0;
		
		public bool IsFinished => _innerTimer >= TotalDuration;

		public string ReservedState;
		public string CurrentActantName = "NullActant";

		public int[] GetActantByFrame(int frame)
		{
			return Actants.Select((actant, idx) => new { idx, actant })
				.Where(pair => pair.actant.StartFrame <= frame && frame <= pair.actant.EndFrame)
				.Select(pair => pair.idx).ToArray();
		}
		
		public void ResetState(ActionStateMachine machine = null, SerializedDictionary<string, object> data = null)
		{
			Data = data ?? new SerializedDictionary<string, object>();
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

	[Serializable]
	public class ActantWrapper : UnityEngine.Object
	{
		public SingleActant actant;
	}
}
