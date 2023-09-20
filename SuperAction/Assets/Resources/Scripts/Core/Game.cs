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
            Player.transform.position = Vector3.up * 6;
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
