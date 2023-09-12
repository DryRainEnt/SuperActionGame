using System.Collections.Generic;
using System.Linq;
using SimpleActionFramework.Actant;
using UnityEngine;

namespace SimpleActionFramework.Core
{
	[System.Serializable]
	[CreateAssetMenu(fileName = "NewActionState", menuName = "Simple Action Framework/Create New Action State", order = 2)]
	public class ActionState : ScriptableObject
	{
		[SerializeReference]
		public List<SingleActant> Actants = new();
		
		[SerializeReference]
		public List<SingleActant> actantWrapper = new();

		public int TotalDuration => (Actants.Count > 0 ? Actants.Max(actant => actant.EndFrame) : 0);
		
		public int GetId => GetInstanceID();
		
		public int connectedStateCount;
		
		private List<string> _connectedStates = new();
		public List<string> ConnectedStates {
			get
			{
				if (_connectedStates.Count > 0)
					return _connectedStates;

				foreach (var act in Actants)
				{
					if (act is SetActionStateActant set)
					{
						_connectedStates.Add(set.StateKey);
					}
				}
				return _connectedStates;
			}
		}


		private void OnEnable()
		{
			for (var index = 0; index < Actants.Count; index++)
			{
				var actant = Actants[index];
				actant.Idx = index;
			}
			
			_connectedStates.Clear();
			connectedStateCount = ConnectedStates.Count;
		}

		public int[] GetActantByFrame(int frame)
		{
			return Actants.Select((actant, idx) => new { idx, actant })
				.Where(pair => pair.actant.StartFrame <= frame && frame < pair.actant.EndFrame)
				.Select(pair => pair.idx).ToArray();
		}
        
		public void OnGUIFrame(Rect position, float scale, int frame)
		{
			foreach (var act in GetActantByFrame(frame).Select(idx => Actants[idx]))
			{
				var progress = Mathf.Clamp01((frame - act.StartFrame)
				                             / ((float)act.Duration == 0f ? 1f : act.Duration));
				
				act.OnGUI(position, scale, progress);
			}
		}
        
		public void DrawGizmos(int frame)
		{
			foreach (var act in GetActantByFrame(frame).Select(idx => Actants[idx]))
			{
				act.OnDrawGizmos();
			}
		}
	}
}
