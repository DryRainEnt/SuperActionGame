using System;
using System.Collections;
using System.Collections.Generic;
using Proto.EventSystem;
using Resources.Scripts.Events;
using SimpleActionFramework.Core;
using UnityEngine;

public class ActorUIField : MonoBehaviour, IEventListener
{
    public int TargetActorIndex;
    
    public UnityEngine.UI.Image HPBar;
    public UnityEngine.UI.Image PrevBar;
    public TMPro.TMP_Text HPText;
    
    private void Start()
    {
        MessageSystem.Subscribe(typeof(OnHealthUpdatedEvent), this);
    }

    // Update is called once per frame
    void Update()
    {
        var dt = Time.deltaTime;
        PrevBar.fillAmount = Mathf.Lerp(PrevBar.fillAmount, HPBar.fillAmount, dt);
    }

    public bool OnEvent(IEvent e)
    {
        if (e is OnHealthUpdatedEvent hue)
        {
            if (hue.ActorIndex != TargetActorIndex)
                return false;
            
            HPBar.fillAmount = hue.CurrentHP / 100f;
            HPText.text = $"{hue.CurrentHP:0} / 100";
            return true;
        }
        return false;
    }
}
