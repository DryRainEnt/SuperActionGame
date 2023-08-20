using System;
using System.Collections;
using System.Collections.Generic;
using CMF;
using Proto.BasicExtensionUtils;
using SimpleActionFramework.Implements;
using SimpleActionFramework.Core;
using UnityEngine;
using Constants = Proto.BasicExtensionUtils.Constants;

public class Actor : MonoBehaviour
{
    [SerializeField]
    public FrameDataSet FrameDataSet;
    
    [SerializeField]
    public ActionStateMachine ActionStateMachine;
    
    private ActorController _actorController;
    
    public SpriteRenderer SpriteRenderer;
    public BoxCollider PhysicsCollider;
    
    public string CurrentState;
    public string CurrentActantName;
    public int CurrentFrame;
    
    public float StateSpeed = 1f;
    

    private void Initiate()
    {
        SpriteRenderer = GetComponent<SpriteRenderer>();
        _actorController = GetComponent<ActorController>();
        
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

        var dx = _actorController.GetMovementVelocity().x;
        ActionStateMachine.UpdateData("MoveDirection", 
            dx.Abs() < Constants.Epsilon ? 0f : dx.Sign());
        
        ActionStateMachine.UpdateState(dt);
        
        CurrentState = ActionStateMachine.CurrentStateName;
        CurrentActantName = ActionStateMachine.CurrentState.CurrentActantName;
        CurrentFrame = ActionStateMachine.CurrentState.CurrentFrame;
    }


    #region SpriteSetters

    public void SetSprite(Sprite sprite, bool xFlip = false)
    {
        SpriteRenderer.sprite = sprite;
        SpriteRenderer.flipX = xFlip;
    }

    public void SetFrame(FrameData frameData)
    {
        SetSprite(frameData.Sprite);
    }
    
    #endregion
}
