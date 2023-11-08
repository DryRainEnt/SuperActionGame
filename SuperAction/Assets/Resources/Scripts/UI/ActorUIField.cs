using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using CMF;
using Proto.EventSystem;
using Resources.Scripts;
using Resources.Scripts.Events;
using SimpleActionFramework.Core;
using UnityEngine;

public class ActorUIField : MonoBehaviour, IEventListener
{
    public int TargetActorIndex;
    private Actor _targetActor;
    
    public UnityEngine.UI.Image HPBar;
    public UnityEngine.UI.Image PrevBar;
    public TMPro.TMP_Text HPText;

    public TMPro.TMP_Text DeathCountText;
    public TMPro.TMP_Text FrameDataText;
    private List<string> _frameDataQueue = new List<string>();

    public UnityEngine.UI.Image AxisStick;
    public Vector2 AxisStickOrigin;
    public UnityEngine.UI.Image CommandButtonA;
    public UnityEngine.UI.Image CommandButtonB;

    public TMPro.TMP_Text IntentionText;
    
    private async void Start()
    {
        MessageSystem.Subscribe(typeof(OnHealthUpdatedEvent), this);
        MessageSystem.Subscribe(typeof(OnDeathEvent), this);
        AxisStickOrigin = AxisStick.transform.localPosition;

        await Task.Run(() =>
        {
            while (_targetActor == null)
            {
                if (Game.Instance.RegisteredActors.ContainsKey(TargetActorIndex))
                {
                    _targetActor = Game.Instance.RegisteredActors[TargetActorIndex];
                }
            }
        });
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!_targetActor)
            return;
        
        var dt = Time.deltaTime;
        PrevBar.fillAmount = Mathf.Lerp(PrevBar.fillAmount, HPBar.fillAmount, dt);

        _frameDataQueue.Insert(0, _targetActor.useCharacterInput ? "<#00aa00>O__</color>" : "<#aa0000>__X</color>");
        if (_frameDataQueue.Count > 0)
        {
            FrameDataText.text = string.Join("\n", _frameDataQueue);
        }

        while(_frameDataQueue.Count > 60)
            _frameDataQueue.RemoveAt(_frameDataQueue.Count - 1);
        
        AxisStick.transform.localPosition = AxisStickOrigin + _targetActor.InputAxis.normalized * 20f;
        CommandButtonA.color = _targetActor.CommandAxis.x > 0.5f ? Color.grey : Color.white;
        CommandButtonB.color = _targetActor.CommandAxis.y > 0.5f ? Color.grey : Color.white;
        
        IntentionText.text = string.Join(" | ", _targetActor.Intention);
        
        DeathCountText.color = _targetActor.OriginColor;
    }

    public void OnControllerChange(int index)
    {
        if (!_targetActor)
            return;
        
        _targetActor.ChangeActorControl(index);
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
        if (e is OnDeathEvent de)
        {
            if (de.ActorIndex == TargetActorIndex)
                return false;
            
            DeathCountText.text = $"{de.NewDeathCount}";
            return true;
        }
        return false;
    }
}
