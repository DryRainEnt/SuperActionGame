using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageLoader : MonoBehaviour
{
    public static StageLoader Instance => _instance ? _instance : _instance = FindObjectOfType<StageLoader>();
    private static StageLoader _instance;

    public GameObject MapParent;

    public StageData LoadStage(StageData stageData)
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

        return stageData;
    }

    public void UnloadStage()
    {
        AudienceController.Instance.DisposeAudience();

        MapParent.SetActive(false);
    }
}