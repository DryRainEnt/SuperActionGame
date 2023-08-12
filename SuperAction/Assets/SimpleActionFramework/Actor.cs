using System;
using System.Collections;
using System.Collections.Generic;
using SimpleActionFramework.Core;
using UnityEngine;

public class Actor : MonoBehaviour
{
    [SerializeField]
    public FrameDataSet FrameDataSet;
    
    [SerializeField]
    public ActionStateMachine ActionStateMachine;
    
    public SpriteRenderer SpriteRenderer;
    public BoxCollider PhysicsCollider;
    
    public string CurrentState;
    public string CurrentActantName;
    public int CurrentFrame;
    
    public float StateSpeed = 1f;
    

    private void Initiate()
    {
        SpriteRenderer = GetComponent<SpriteRenderer>();
        
        ActionStateMachine = Instantiate(ActionStateMachine);
        ActionStateMachine.Init(this);
    }

    private void OnEnable()
    {
        Initiate();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var dt = StateSpeed * Time.deltaTime;
        
        ActionStateMachine.UpdateState(dt);
        
        CurrentState = ActionStateMachine.CurrentStateName;
        CurrentActantName = ActionStateMachine.CurrentState.CurrentActantName;
        CurrentFrame = ActionStateMachine.CurrentState.CurrentFrame;
    }


    #region SpriteSetters
    
    public void SetSprite(Sprite sprite)
    {
        SpriteRenderer.sprite = sprite;
    }

    public void SetFrame(FrameData frameData)
    {
        SetSprite(frameData.Sprite);
    }
    
    #endregion
}
