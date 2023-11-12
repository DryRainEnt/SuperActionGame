using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class MainUIController : MonoBehaviour
{
    public static MainUIController Instance => _instance ? _instance : _instance = FindObjectOfType<MainUIController>();
    private static MainUIController _instance;

    public GameObject PlayRaidButton;
    public GameObject PlaySurvivalButton;

    private void Awake()
    {
        PlaySurvivalButton.GetComponent<Button>().onClick.AddListener(async () =>
        {
            PlayRaidButton.SetActive(false);
            PlaySurvivalButton.SetActive(false);
            
            await ReadySurvivalMode(new PlayerData());
        });
    }

    public async Task ReadySurvivalMode(PlayerData playerData)
    {
        StageLoader.Instance.LoadStage(new StageData()
        {
            StageType = StageType.Test,
            Participants = new[]
            {
                new ParticipantData()
                {
                    Owner = playerData,
                    Color = Color.cyan,
                    ControllerType = 1,
                },
            },
        });
        
        await Game.Instance.StartGame();
    }
}

public class MainUIData
{
    
}
