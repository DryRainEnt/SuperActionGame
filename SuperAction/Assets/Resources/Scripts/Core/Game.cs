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
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void FixedUpdate()
    {
        MaskManager.Update();
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
}
