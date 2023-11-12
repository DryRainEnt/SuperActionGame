using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public ParticipantData[] Participants;
}

public struct ParticipantData
{
    public PlayerData Owner;
    public Color Color;
    public int ControllerType;
}
