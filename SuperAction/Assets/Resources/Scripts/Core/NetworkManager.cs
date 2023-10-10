using System;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Proto.EventSystem;
using UnityEngine;

namespace Resources.Scripts.Core
{
	public class NetworkManager : MonoBehaviour, IEventListener
	{
		public string FileName = "ALPS.py";
        public string RelativeFilePath => Utils.BuildString('/', Application.streamingAssetsPath, FileName);

        public string WeightsPath(int level) => Utils.BuildString('/', Application.streamingAssetsPath, "Weights", $"weights{level}.json");
        
        public CustomNeuralNetwork NeuralNetwork;

        private bool hasMessage = false;
        private string message = "";
        
        public async Task Initiate()
		{
	        await Task.Run(RunPython);
		}

        private async void RunPython()
        {
	        NeuralNetwork = new CustomNeuralNetwork();
	        NeuralNetwork.FromJson(WeightsPath(1));
	        
			ProcessStartInfo start = new ProcessStartInfo();
			start.FileName = "python";
			start.Arguments = $"{RelativeFilePath}";
			start.UseShellExecute = false;
			start.RedirectStandardInput = true;
			start.RedirectStandardOutput = true;

			using Process process = Process.Start(start);
			while (process is { HasExited: false })
			{
				using (StreamReader reader = process.StandardOutput)
				{
					string result = await reader.ReadToEndAsync();

					UnityEngine.Debug.Log(result);
					if (int.TryParse(result, out var level))
					{
						NeuralNetwork.FromJson(WeightsPath(level));
					}
				}

				if (hasMessage)
				{
					await process.StandardInput.WriteLineAsync(message);
					message = String.Empty;
					hasMessage = false;
				}
			}
        }

        public bool OnEvent(IEvent e)
        {
	        return false;
        }
	}
	
	public static class CustomNeuralNetworkExtension
	{
		public static float ReLU(float x)
		{
			return Mathf.Max(0, x);
		}
    
		public static float Tanh(float x)
		{
			float eToX = Mathf.Exp(x);
			float eToNegX = Mathf.Exp(-x);
			return (eToX - eToNegX) / (eToX + eToNegX);
		}

		public static float[] ForwardA(this CustomNeuralNetwork nn, float[] input)
		{
			// 첫 번째 레이어
			float[] hidden1 = new float[60];
			for (int i = 0; i < 60; i++)
			{
				for (int j = 0; j < 12; j++)
				{
					hidden1[i] += input[j] * nn.net_a_fc1_weight[i * 12 + j];
				}
				hidden1[i] = ReLU(hidden1[i] + nn.net_a_fc1_bias[i]);
			}
        
			// 두 번째 레이어
			float[] hidden2 = new float[24];
			for (int i = 0; i < 24; i++)
			{
				for (int j = 0; j < 60; j++)
				{
					hidden2[i] += hidden1[j] * nn.net_a_fc2_weight[i * 60 + j];
				}
				hidden2[i] = ReLU(hidden2[i] + nn.net_a_fc2_bias[i]);
			}
        
			// 출력 레이어
			float[] output = new float[5];
			for (int i = 0; i < 5; i++)
			{
				for (int j = 0; j < 24; j++)
				{
					output[i] += hidden2[j] * nn.net_a_fc3_weight[i * 24 + j];
				}
				output[i] = Tanh(output[i] + nn.net_a_fc3_bias[i]);
			}
        
			return output;
		}

		public static float[] ForwardB(this CustomNeuralNetwork nn, float[] input)
		{
			// 첫 번째 레이어
			float[] hidden1 = new float[25];
			for (int i = 0; i < 25; i++)
			{
				for (int j = 0; j < 5; j++)
				{
					hidden1[i] += input[j] * nn.net_b_fc1_weight[i * 5 + j];
				}
				hidden1[i] = ReLU(hidden1[i] + nn.net_b_fc1_bias[i]);
			}
			
			// 출력 레이어
			float[] output = new float[1];
			for (int i = 0; i < 1; i++)
			{
				for (int j = 0; j < 24; j++)
				{
					output[i] += hidden1[j] * nn.net_b_fc2_weight[i * 24 + j];
				}
				output[i] = Tanh(output[i] + nn.net_b_fc2_bias[i]);
			}
        
			return output;
		}
	}
}
