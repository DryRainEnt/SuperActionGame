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

		public SerializedDictionary<string, object> Data { get; set; }

		public int TotalDuration => (Actants.Count > 0 ? Actants.Max(actant => actant.EndFrame) : 0);

		private float _innerTimer;

		public int CurrentFrame
		{
			get => Mathf.FloorToInt(_innerTimer * 30f);
			set => _innerTimer = value / 30f;
		}
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
		
		public void ResetState(Actor actor = null, SerializedDictionary<string, object> data = null)
		{
			Data = data ?? new SerializedDictionary<string, object>();
			_innerTimer = 0f;
			_previousFrame = CurrentFrame;

			foreach (var act in Actants)
			{
				act.State = ActantState.NotStarted;
				act.Init(actor);
			}
		}
		
		public void UpdateState(Actor actor, float dt)
		{
			CurrentActantName = "NullActant";

			foreach (var act in Actants)
			{
				var progress = Mathf.Clamp01((_innerTimer - act.StartFrame / Constants.DefaultActionFrameRate)
				                             / ((float)act.Duration / Constants.DefaultActionFrameRate));
				
				UpdateActant(act, actor, progress);
			}
			
			_previousFrame = CurrentFrame;
			_innerTimer += dt;
		}
		
		public bool UpdateActant(SingleActant act, Actor actor, float progress)
		{
			if (act.State == ActantState.Finished || act.StartFrame > CurrentFrame)
				return false;
			
			var isFirst = act.EndFrame > CurrentFrame
			              && act.StartFrame <= CurrentFrame
			              && act.State == ActantState.NotStarted;

			if (isFirst) act.State = ActantState.Running;
			if (act.EndFrame < CurrentFrame) act.State = ActantState.Finished;
			if (act.StartFrame > CurrentFrame) act.State = ActantState.NotStarted;

			act.Act(actor, progress, isFirst);

			return true;
		}
		
		public void OnGUIFrame(Rect position, float scale)
		{
			foreach (var act in GetActantByFrame(CurrentFrame).Select(idx => Actants[idx]))
			{
				var progress = Mathf.Clamp01((_innerTimer - act.StartFrame / Constants.DefaultActionFrameRate)
				                             / ((float)act.Duration / Constants.DefaultActionFrameRate));
				
				act.OnGUI(position, scale, progress);
			}
		}

		public void OnDrawGizmos()
		{
			foreach (var act in GetActantByFrame(CurrentFrame).Select(idx => Actants[idx]))
			{
				act.OnDrawGizmos();
			}
		}
	}
}
