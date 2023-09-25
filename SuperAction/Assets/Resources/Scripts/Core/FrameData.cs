using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Resources.Scripts.Core
{
	[Serializable]
	public struct FrameDataChunk
	{
		public int ChunkId;
		[SerializeField]
		public List<FrameData> Frames;

		public FrameDataChunk(int id)
		{
			ChunkId = id;
			Frames = new List<FrameData>();
		}
		
		public string ToJson()
		{
			return JsonUtility.ToJson(this, true);
		}
		
		public FrameDataChunk FromJson(string json)
		{
			return JsonUtility.FromJson<FrameDataChunk>(json);
		}

		public void Append(FrameData data)
		{
			Frames.Add(data);
		}

		public void Clear()
		{
			Frames.Clear();
			ChunkId++;
		}
	}
	
	[Serializable]
	public struct FrameData
	{
		public int Frame;

		public int[] ActorIds;

		public float[] ActorDataSet;

		public FrameData(int frame)
		{
			Frame = frame;
			var count = Game.Instance.RegisteredActors.Count;
			ActorIds = new int[count];
			ActorDataSet = new float[count * 6];

			int i = 0;
			foreach (var (id, actor) in Game.Instance.RegisteredActors)
			{
				ActorIds[i] = id;
				
				ActorDataSet[i * 6 + 0] = actor.HP;
				ActorDataSet[i * 6 + 1] = actor.CurrentState.GetHashCode();
				ActorDataSet[i * 6 + 2] = actor.Position.x;
				ActorDataSet[i * 6 + 3] = actor.Position.y;
				ActorDataSet[i * 6 + 4] = actor.Velocity.x;
				ActorDataSet[i * 6 + 5] = actor.Velocity.y;

				i++;
			}
		}
		
		public FrameData(FrameData copy)
		{
			Frame = copy.Frame;
			
			ActorIds = new int[copy.ActorIds.Length];
			for (int i = 0; i < copy.ActorIds.Length; i++)
			{
				ActorIds[i] = copy.ActorIds[i];
			}
			
			ActorDataSet = new float[copy.ActorDataSet.Length];
			for (int i = 0; i < copy.ActorDataSet.Length; i++)
			{
				ActorDataSet[i] = copy.ActorDataSet[i];
			}
		}
		
		public FrameData CopyFrom(FrameData copy)
		{
			Frame = copy.Frame;
			
			ActorIds = new int[copy.ActorIds.Length];
			for (int i = 0; i < copy.ActorIds.Length; i++)
			{
				ActorIds[i] = copy.ActorIds[i];
			}
			
			ActorDataSet = new float[copy.ActorDataSet.Length];
			for (int i = 0; i < copy.ActorDataSet.Length; i++)
			{
				ActorDataSet[i] = copy.ActorDataSet[i];
			}

			return this;
		}
		
		public string ToJson()
		{
			return JsonUtility.ToJson(this);
		}
		
		public FrameData FromJson(string json)
		{
			return CopyFrom(JsonUtility.FromJson<FrameData>(json));
		}
	}
}