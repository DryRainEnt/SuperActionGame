using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SimpleActionFramework.Core
{
	[System.Serializable]
	public class ActionState : ScriptableObject
	{
		public List<SingleActant> Actants;

		public Character Character { get; set; }
		public SerializedDictionary<string, string> Data { get; set; }

		public ushort TotalDuration => Actants.Max(actant => actant.EndFrame);

		private float _innerTimer;
		public ushort CurrentFrame => (ushort)(_innerTimer * 30);
		private ushort _previousFrame = 0;
		
		public bool IsFinished => _innerTimer >= TotalDuration;

		public string ReservedState;
		
		public void ResetState(Character character = null, SerializedDictionary<string, string> data = null)
		{
			Character = character;
			Data = data ?? new SerializedDictionary<string, string>();
			_innerTimer = 0f;
		}
		
		public void UpdateState(ActionStateMachine machine, float dt)
		{
			_previousFrame = CurrentFrame;
			_innerTimer += dt;

			foreach (var act in Actants
				         .Where(act => act.StartFrame <= CurrentFrame
				                       && act.EndFrame > CurrentFrame)
				         .Where(act => !act.UsedOnce
				                       || act.StartFrame > _previousFrame))
			{
				var progress = Mathf.Clamp01((_innerTimer - act.StartFrame / 30f) / (float)act.Duration);
				var isFirst = _previousFrame < act.StartFrame && act.StartFrame <= CurrentFrame;
				
				act.Act(machine, progress, isFirst);
			}
		}
	}
}
