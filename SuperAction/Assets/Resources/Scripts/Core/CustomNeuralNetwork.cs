using System;
using UnityEngine;
using Newtonsoft.Json;

namespace Resources.Scripts.Core
{
	[Serializable]
	public struct CustomNeuralNetwork
	{
		public int level;
		
		public float[] net_a_fc1_bias; // [60, 12]
		public float[,] net_a_fc1_weight; // [60, 12]
		public float[] net_a_fc2_bias; // [24, 60]
		public float[,] net_a_fc2_weight; // [24, 60]
		public float[] net_a_fc3_bias; // [5, 24]
		public float[,] net_a_fc3_weight; // [5, 24]
		
		public float[] net_b_fc1_bias; // [25, 5]
		public float[,] net_b_fc1_weight; // [25, 5]
		public float[] net_b_fc2_bias; // [1, 25]
		public float[,] net_b_fc2_weight; // [1, 25]
        
		public string ToJson()
		{
			return JsonConvert.SerializeObject(this, Formatting.Indented);
		}
		
		public static CustomNeuralNetwork FromJson(string json)
		{
			Debug.Log($"Load Json: {json}");
			CustomNeuralNetwork newCnn = JsonConvert.DeserializeObject<CustomNeuralNetwork>(json);
			Debug.Log($"level: {newCnn.level}");
			
			return newCnn;
		}
	}
}