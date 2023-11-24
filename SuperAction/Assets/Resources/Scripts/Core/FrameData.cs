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
		
		public void Modify(int frame, FrameData data)
		{
			var index = FindIndex(frame);
			if (index == -1)
			{
				Append(data);
				return;
			}

			Frames[index] = data;
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
		
		public int FindIndex(int frame)
		{
			return Frames.FindIndex(f => f.Frame == frame);
		}
		
		public FrameData Last()
		{
			return Frames.Last();
		}

		public FrameData this[int frame]
		{
			get
			{
				var idx = FindIndex(frame);
				return Frames[(idx < 0) ? Frames.Count - 1 : idx];
			}
			set => Modify(frame, value);
		}
	}
	
	[Serializable]
	public struct FrameData
	{
		public int Frame;

		public int[] ActorIds;

		public float[] ActorDataSet;

		public float Validation;

		public float[] Intention;
		
		public float Aggressiveness => Intention[0] * 1f + Intention[1] * 0.8f + Intention[2] * -0.3f + Intention[3] * -0.6f + Intention[4] * -0.9f;

		public void AddValidation(float v)
		{
			Validation += v;
		}

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
				
				ActorDataSet[i * 6 + 0] = actor.HP / 100f;
				ActorDataSet[i * 6 + 1] = actor.CurrentState.GetHashCode() / (float)int.MaxValue;
				ActorDataSet[i * 6 + 2] = actor.Position.x / 16f;
				ActorDataSet[i * 6 + 3] = actor.Position.y / 16f;
				ActorDataSet[i * 6 + 4] = actor.Velocity.x / 16f;
				ActorDataSet[i * 6 + 5] = actor.Velocity.y / 16f;

				i++;
			}

			Validation = 0;
			
			NetworkManager.Instance.NeuralNetwork.ForwardA(out Intention, ActorDataSet);
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

			Validation = copy.Validation;
			
			copy.Intention.CopyTo(Intention = new float[copy.Intention.Length], 0);
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
			
			Validation = copy.Validation;
			copy.Intention.CopyTo(Intention = new float[copy.Intention.Length], 0);

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