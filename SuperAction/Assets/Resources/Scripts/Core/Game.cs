using System;
using System.Collections;
using System.Collections.Generic;
using Proto.EventSystem;
using Proto.PoolingSystem;
using Resources.Scripts.Events;
using SimpleActionFramework.Core;
using UnityEngine;

public class Game : MonoBehaviour, IEventListener
{
    private static Game _instance;
    public static Game Instance => _instance ? _instance : FindObjectOfType<Game>();
    
    public Actor Player;
    
    public Dictionary<int, Actor> RegisteredActors = new Dictionary<int, Actor>();
    
    [SerializeField]
    private UnityEngine.UI.Text _debugText;

    public Vector2Int ScreenResolution = new Vector2Int(1280, 720);
    public int ScreenResolutionFactor = 1;
    
    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 60;
    }

    // Update is called once per frame
    void Update()
    {
        while (TimerUtils.Alarms.Count > 0)
            if (!TimerUtils.Alarms[0].AlarmCheck())
                break;
        
        if (GlobalInputController.Instance.GetInput("reset"))
        {
            foreach (var actor in RegisteredActors.Values)
            {
                actor.ResetPosition();
            }
        }

        if (GlobalInputController.Instance.GetPressed("resize"))
        {
            ScreenResolutionFactor++;
            if (ScreenResolutionFactor > 3)
                ScreenResolutionFactor = 1;
            Screen.SetResolution(ScreenResolution.x, ScreenResolution.y, false);
        }
    }

    private void FixedUpdate()
    {
        MaskManager.Instance.Update();
    }

    public bool OnEvent(IEvent e)
    {
        if (e is OnAttackHitEvent hitEvent)
        {
            Debug.Log(hitEvent);
            return true;
        }
        
        return false;
    }

    private void OnDrawGizmos()
    {
        _debugText.text = $"Masks: {MaskManager.HitMaskList.Count}";
        
        foreach (var mask in MaskManager.HitMaskList)
        {
            var c = mask.Type.GetColor();
            c.a = 0.5f;
            Gizmos.color = c;
            Gizmos.DrawCube(mask.Bounds.center, mask.Bounds.size);
        }

        foreach (var hit in MaskManager.HitDataList)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere((hit.GiverMask.Bounds.center + hit.ReceiverMask.Bounds.center) * 0.5f, 0.3f);
        }
    }
}
