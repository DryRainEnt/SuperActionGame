using System;
using System.IO;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Proto.EventSystem;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Resources.Scripts.Core
{
	public class NetworkManager : MonoBehaviour, IEventListener
	{
		private static NetworkManager _instance;
        public static NetworkManager Instance => _instance ? _instance : _instance = FindObjectOfType<NetworkManager>();
		
        private TcpClient client;
        private NetworkStream stream;

		public string FileName = "main.exe";
        public string RelativeFilePath => Utils.BuildString('/', Application.streamingAssetsPath, FileName);

        public string WeightsPath => Utils.BuildString('/', Application.streamingAssetsPath, "Weights", "weights.json");
        
        public bool isActive = false;
        public CustomNeuralNetwork NeuralNetwork;

        private bool hasMessage = false;
        private string message = "";
        
        private bool isLearning = false;

        private void Awake()
        {
	        _instance = this;
	        isActive = false;
        }

        public async Task<bool> StartLearning()
        {
	        SendPyMessage("start");
	        isLearning = true;

	        await WaitForLearning();

	        return true;
        }

        private async Task GetWeightJson()
        {
	        isActive = false;
	        Debug.Log("NeuralNetwork: Loading...");
	        if (File.Exists(WeightsPath))
	        {
		        NeuralNetwork = NeuralNetwork.FromJson(await File.ReadAllTextAsync(WeightsPath));
		        Debug.Log("NeuralNetwork: Loaded.");
		        isActive = true;
	        }
	        else
	        {
		        Debug.LogError("NeuralNetwork: Load failed. No such file exists.");
	        }
        }
        
        public async void SendPyMessage(string m)
		{
			hasMessage = true;
			this.message = m;

			// Send data
	        byte[] data = Encoding.ASCII.GetBytes(message);
	        await stream.WriteAsync(data, 0, data.Length);
	        hasMessage = false;
		}

        public async Task WaitForLearning()
        {
	        while (isLearning)
		        await Task.Delay(100);
        }

        public async Task RunPython()
        {
	        var firstLoad = true;
	        isLearning = true;
	        
	        if (!File.Exists(RelativeFilePath))
	        {
		        Debug.LogError("Learn: File not found.");
		        return;
	        }
	        else
	        {
		        Debug.Log("Learn: File exists.");
	        }

	        await GetWeightJson();
	        
	        Debug.Log("Learn: Started");
	        
	        client = new TcpClient("127.0.0.1", 5000);
	        stream = client.GetStream();

	        // Receive data
	        byte[] buffer = new byte[1024];
	        int bytesRead = stream.Read(buffer, 0, buffer.Length);
	        
			while (bytesRead != 0)
			{
				Debug.Log("Learn: Running...");
				
				bytesRead = stream.Read(buffer, 0, buffer.Length);
				string result = Encoding.ASCII.GetString(buffer, 0, bytesRead);
				Debug.Log($"Received: {result}");

				Debug.Log(result);
				if (firstLoad && result.Contains("Waiting>"))
				{
					firstLoad = false;
					isLearning = false;
					SendPyMessage("start");
				}
					
				if (result.Contains("Done>"))
				{
					Debug.Log("Learn: Success.");

					await GetWeightJson();
					Debug.Log("Learn: Applied.");
					isLearning = false;
				}
			}
			
			Debug.Log("Learn: Finished");
        }

        public bool OnEvent(IEvent e)
        {
	        return false;
        }

        private void OnApplicationQuit()
        {
	        SendPyMessage("quit");
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
			try
			{
				// 첫 번째 레이어
				float[] hidden1 = new float[60];
				for (int i = 0; i < 60; i++)
				{
					for (int j = 0; j < 12; j++)
					{
						hidden1[i] += input[j] * nn.net_a_fc1_weight[i];
					}
					hidden1[i] = ReLU(hidden1[i] + nn.net_a_fc1_bias[i]);
				}
	        
				// 두 번째 레이어
				float[] hidden2 = new float[24];
				for (int i = 0; i < 24; i++)
				{
					for (int j = 0; j < 60; j++)
					{
						hidden2[i] += hidden1[j] * nn.net_a_fc2_weight[i];
					}
					hidden2[i] = ReLU(hidden2[i] + nn.net_a_fc2_bias[i]);
				}
	        
				// 출력 레이어
				float[] output = new float[5];
				for (int i = 0; i < 5; i++)
				{
					for (int j = 0; j < 24; j++)
					{
						output[i] += hidden2[j] * nn.net_a_fc3_weight[i];
					}
					output[i] = Tanh(output[i] + nn.net_a_fc3_bias[i]);
				}
	        
				return output;
			}
			catch (IndexOutOfRangeException e)
			{
				Debug.LogWarning(input.Length);
				Debug.LogWarning(nn.net_a_fc1_weight.Length);
				Debug.LogWarning(nn.net_a_fc1_bias.Length);
				Debug.LogWarning(nn.net_a_fc2_weight.Length);
				Debug.LogWarning(nn.net_a_fc2_bias.Length);
				Debug.LogWarning(nn.net_a_fc3_weight.Length);
				Debug.LogWarning(nn.net_a_fc3_bias.Length);
				Console.WriteLine(e);
				throw;
			}
		}

		public static float[] ForwardB(this CustomNeuralNetwork nn, float[] input)
		{
			// 첫 번째 레이어
			float[] hidden1 = new float[25];
			for (int i = 0; i < 25; i++)
			{
				for (int j = 0; j < 5; j++)
				{
					hidden1[i] += input[j] * nn.net_b_fc1_weight[i];
				}
				hidden1[i] = ReLU(hidden1[i] + nn.net_b_fc1_bias[i]);
			}
			
			// 출력 레이어
			float[] output = new float[1];
			for (int i = 0; i < 1; i++)
			{
				for (int j = 0; j < 24; j++)
				{
					output[i] += hidden1[j] * nn.net_b_fc2_weight[i];
				}
				output[i] = Tanh(output[i] + nn.net_b_fc2_bias[i]);
			}
        
			return output;
		}
	}
}
