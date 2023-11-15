using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Proto.PoolingSystem;
using SimpleActionFramework.Core;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class MainUIController : MonoBehaviour
{
    public static MainUIController Instance => _instance ? _instance : _instance = FindObjectOfType<MainUIController>();
    private static MainUIController _instance;

    public GameObject MainUI;
    public GameObject PlayRaidButton;
    public GameObject PlaySurvivalButton;
    public Slider AISlider;

    public Toggle IsHost;

    private void Awake()
    {
        PlayRaidButton.GetComponent<Image>().alphaHitTestMinimumThreshold = 1f;
        PlaySurvivalButton.GetComponent<Image>().alphaHitTestMinimumThreshold = 1f;
        
        PlaySurvivalButton.GetComponent<Button>().onClick.AddListener(async () =>
        {
            MainUI.SetActive(false);
            
            await ReadySurvivalMode(new PlayerData());
        });
    }

    private async Task ReadySurvivalMode(PlayerData playerData)
    {
        if (IsHost.isOn)
        {
            NetworkManager.Singleton.StartHost();
        
            var participants = new ParticipantData[(int)AISlider.value + 1];
            participants[0] = new ParticipantData()
            {
                Owner = playerData,
                Color = Color.cyan,
                ControllerType = 1,
            };
        
            for (int i = 1; i < participants.Length; i++)
            {
                var randomColor = UnityEngine.Random.ColorHSV(0f, 1f, 0.8f, 1f, 0.8f, 1f);

                participants[i] = new ParticipantData()
                {
                    Owner = new PlayerData(),
                    Color = randomColor,
                    ControllerType = 2,
                };
            }
        
            var stage = StageLoader.Instance.LoadStage(new StageData()
            {
                StageType = StageType.Test,
                Participants = participants,
            });
        
            await Game.Instance.StartGame(stage);
        }
        else
        {
            NetworkManager.Singleton.StartClient();
            
        }
    }
}

public class MainUIData
{
    
}
