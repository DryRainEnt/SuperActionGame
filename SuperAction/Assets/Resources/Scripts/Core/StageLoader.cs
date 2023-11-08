using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageLoader : MonoBehaviour
{
    public static StageLoader Instance => _instance ? _instance : _instance = FindObjectOfType<StageLoader>();
    private static StageLoader _instance;

    public GameObject MapParent;

    public void LoadStage(StageData stageData)
    {
        switch (stageData.StageType)
        {
            case StageType.Survival:
                break;
            case StageType.Raid:
                break;
            case StageType.Lobby:
                break;
            case StageType.Test:
                MapParent.SetActive(true);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}

public enum StageType
{
    Survival,
    Raid,
    Lobby,
    Test,
}

public class StageData
{
    public StageType StageType;
}
