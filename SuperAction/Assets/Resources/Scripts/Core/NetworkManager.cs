using System;
using System.IO;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Proto.BasicExtensionUtils;
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
        
        public CustomNeuralNetwork NeuralNetwork;

        private bool hasMessage = false;
        private string message = "";
        
        public bool isActive = false;
        public bool isConnected = false;
        public bool isLearning = false;

        private void Awake()
        {
	        _instance = this;
	        isActive = false;
        }

        public async Task<bool> StartLearning()
        {
			if (!isConnected)
				return false;

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
		        NeuralNetwork = CustomNeuralNetwork.FromJson(await File.ReadAllTextAsync(WeightsPath));
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
			if (!isConnected)
				return;

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
				isConnected = true;
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
			isConnected = false;
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
			return Mathf.Max(0f, x);
		}
    
		public static float Tanh(float x)
		{
			float eToX = Mathf.Exp(x);
			float eToNegX = Mathf.Exp(-x);
			return (eToX - eToNegX) / (eToX + eToNegX);
		}

		public static float[,] HeInitialization(int inputSize, int outputSize)
		{
			float[,] weights = new float[inputSize, outputSize];
			float stdDev = Mathf.Sqrt(2f / inputSize);

			for (int i = 0; i < inputSize; i++)
			{
				for (int j = 0; j < outputSize; j++)
				{
					weights[i, j] = GaussianRandom(0f, stdDev);
				}
			}

			return weights;
		}
		
		public static float MeanSquaredError(float[] predicted, float[] actual)
		{
			float sumSquaredError = 0f;
			for (int i = 0; i < predicted.Length; i++)
			{
				sumSquaredError += Mathf.Pow(predicted[i] - actual[i], 2);
			}

			return sumSquaredError / predicted.Length;
		}
		
		private static float GaussianRandom(float mean, float stdDev)
		{
			float u1 = 1.0f - UnityEngine.Random.value;
			float u2 = 1.0f - UnityEngine.Random.value;
			float randStdNormal = Mathf.Sqrt(-2.0f * Mathf.Log(u1)) * Mathf.Sin(2.0f * Mathf.PI * u2);
			return mean + stdDev * randStdNormal;
		}
		
		public static float[] ForwardA(this CustomNeuralNetwork nn, out float[] output, params float[] input)
		{
			try
			{
				// 첫 번째 레이어
				float[] hidden1 = new float[60];
				for (int i = 0; i < 60; i++)
				{
					for (int j = 0; j < 12; j++)
					{
						hidden1[i] += input[j] * nn.net_a_fc1_weight[i,j];
						// Debug.Log($"hidden1[{i}_{j}] = {hidden1[i]} + {input[j]} * {nn.net_a_fc1_weight[i]}");
					}
					hidden1[i] = ReLU(hidden1[i] + nn.net_a_fc1_bias[i]);
					// Debug.Log($"hidden1[{i}] = ReLU({hidden1[i]} + {nn.net_a_fc1_bias[i]})");
				}
	        
				// 두 번째 레이어
				float[] hidden2 = new float[24];
				for (int i = 0; i < 24; i++)
				{
					for (int j = 0; j < 60; j++)
					{
						hidden2[i] += hidden1[j] * nn.net_a_fc2_weight[i,j];
					}
					hidden2[i] = ReLU(hidden2[i] + nn.net_a_fc2_bias[i]);
					// Debug.Log($"hidden2[{i}] = ReLU({hidden2[i]} + {nn.net_a_fc2_bias[i]})");
				}
				
				// 출력 레이어
				output = new float[5];
				for (int i = 0; i < 5; i++)
				{
					for (int j = 0; j < 24; j++)
					{
						output[i] += hidden2[j] * nn.net_a_fc3_weight[i,j];
					}
					output[i] = Tanh(output[i] + nn.net_a_fc3_bias[i]);
					// Debug.Log($"output[{i}] = ReLU({output[i]} + {nn.net_a_fc3_bias[i]})");
				}
	        
				/*
				Debug.Log($"H1: {hidden1[0]}, {hidden1[1]}, {hidden1[2]}, {hidden1[3]}, {hidden1[4]}, {hidden1[5]},\n" +
				          $" {hidden1[6]}, {hidden1[7]}, {hidden1[8]}, {hidden1[9]}, {hidden1[10]}, {hidden1[11]},\n" +
				          $" {hidden1[12]}, {hidden1[13]}, {hidden1[14]}, {hidden1[15]}, {hidden1[16]}, {hidden1[17]},\n" +
				          $" {hidden1[18]}, {hidden1[19]}, {hidden1[20]}, {hidden1[21]}, {hidden1[22]}, {hidden1[23]},\n" +
				          $" {hidden1[24]}, {hidden1[25]}, {hidden1[26]}, {hidden1[27]}, {hidden1[28]}, {hidden1[29]},\n" +
				          $" {hidden1[30]}, {hidden1[31]}, {hidden1[32]}, {hidden1[33]}, {hidden1[34]}, {hidden1[35]},\n" +
				          $" {hidden1[36]}, {hidden1[37]}, {hidden1[38]}, {hidden1[39]}, {hidden1[40]}, {hidden1[41]},\n" +
				          $" {hidden1[42]}, {hidden1[43]}, {hidden1[44]}, {hidden1[45]}, {hidden1[46]}, {hidden1[47]},\n" +
				          $" {hidden1[48]}, {hidden1[49]}, {hidden1[50]}, {hidden1[51]}, {hidden1[52]}, {hidden1[53]},\n" +
				          $" {hidden1[54]}, {hidden1[55]}, {hidden1[56]}, {hidden1[57]}, {hidden1[58]}, {hidden1[59]}" +
				          $"\nH2: {hidden2[0]}, {hidden2[1]}, {hidden2[2]}, {hidden2[3]}, {hidden2[4]}, {hidden2[5]},\n" +
				          $" {hidden2[6]}, {hidden2[7]}, {hidden2[8]}, {hidden2[9]}, {hidden2[10]}, {hidden2[11]},\n" +
				          $" {hidden2[12]}, {hidden2[13]}, {hidden2[14]}, {hidden2[15]}, {hidden2[16]}, {hidden2[17]},\n" +
				          $" {hidden2[18]}, {hidden2[19]}, {hidden2[20]}, {hidden2[21]}, {hidden2[22]}, {hidden2[23]}" +
				          $"\nOutput: {output[0]}, {output[1]}, {output[2]}, {output[3]}, {output[4]}\n" +
				          $"Input1: {input[0]}, {input[1]}, {input[2]}, {input[3]}, {input[4]}, {input[5]}\n" +
				          $"Input2: {input[6]}, {input[7]}, {input[8]}, {input[9]}, {input[10]}, {input[11]}\n");
				          */
				
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
					hidden1[i] += input[j] * nn.net_b_fc1_weight[i,j];
				}
				hidden1[i] = ReLU(hidden1[i] + nn.net_b_fc1_bias[i]);
			}
			
			// 출력 레이어
			float[] output = new float[1];
			for (int i = 0; i < 1; i++)
			{
				for (int j = 0; j < 24; j++)
				{
					output[i] += hidden1[j] * nn.net_b_fc2_weight[i,j];
				}
				output[i] = Tanh(output[i] + nn.net_b_fc2_bias[i]);
			}
        
			return output;
		}
	}
}
