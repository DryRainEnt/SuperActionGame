using System.Collections.Generic;
using Proto.BasicExtensionUtils;
using Proto.EventSystem;
using SimpleActionFramework.Implements;
using Sirenix.OdinInspector;
using UnityEngine;

namespace SimpleActionFramework.Core
{
    public class Actor : MonoBehaviour, IEventListener
    {
        [SerializeField]
        public ActionStateMachine ActionStateMachine;
        
        [ShowInInspector]
        public Dictionary<string, object> Data => ActionStateMachine ? ActionStateMachine.Data : null;
    
        private ActorController _actorController;
    
        public SpriteRenderer SpriteRenderer;
        public BoxCollider PhysicsCollider;
    
        public string CurrentState;
        public string CurrentActantName;
        public int CurrentFrame;
    
        public float StateSpeed = 1f;

        public Vector2 LastDirection;
        public bool IsLeft;
        
        public List<InputRecord> RecordedInputs = new List<InputRecord>();

        private void Initiate()
        {
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

            InputUpdate();
        
            ActionStateMachine.OnUpdate(this, dt);
        
            CurrentState = ActionStateMachine.CurrentStateName;
            CurrentFrame = ActionStateMachine.CurrentFrame;
        }

        private readonly string[] ActionKeys = new[]
        {
            "button1",
            "button2",
            "button3",
            "button4",
            "jump",
            "forward",
            "backward",
            "up",
            "down",
        };
        
        [SerializeField]
        private UnityEngine.UI.Text _debugText;
        
        private void InputUpdate()
        {
            RecordedInputs.Sort();
            foreach (var key in ActionKeys)
            {
                _actorController.CharacterInput.InputCheck(this, key);
            }
            RecordedInputs.Sort();

            while (RecordedInputs.Count > 0 && RecordedInputs[^1].ReleaseTime < Time.realtimeSinceStartup - 0.5f)
            {
                RecordedInputs.RemoveAt(RecordedInputs.Count - 1);
            }
            
            ActionStateMachine.UpdateData("Inputs", RecordedInputs);

            ActionStateMachine.UpdateData("MoveDirection", 
                _actorController.GetMovementVelocity().x.Abs() < Constants.Epsilon ? 0f : _actorController.GetMovementVelocity().x.Sign());
            LastDirection = _actorController.GetMovementVelocity().normalized;
        
            IsLeft = LastDirection.x > Constants.Epsilon ? false : LastDirection.x < -Constants.Epsilon ? true : IsLeft;
            ActionStateMachine.UpdateData("FaceDirection", IsLeft ? -1f : 1f);
            ActionStateMachine.UpdateData("VerticalSpeed", _actorController.GetMovementVelocity().y);
            ActionStateMachine.UpdateData("IsGrounded", _actorController.IsGrounded() ? 1f : 0f);

            if (_debugText)
            {
                _debugText.text = "";
                foreach (var input in RecordedInputs)
                {
                    _debugText.text += input.Key + " : " + input.PressTime + " ~ " + input.ReleaseTime + "\n";
                }
            }
        }
        
        public void SetVelocity(Vector2 velocity)
        {
            _actorController.SetMovementVelocity(velocity);
        }
        
        public void ToggleGravity(bool toggle)
        {
            _actorController.ToggleGravity(toggle);
        }
        
        public void ToggleCharacterInput(bool toggle)
        {
            _actorController.ToggleCharacterInput(toggle);
        }

        #region SpriteSetters

        public void SetSprite(Sprite sprite, bool xFlip = false)
        {
            SpriteRenderer.sprite = sprite;
            SpriteRenderer.flipX = IsLeft;
        }
    
        #endregion

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying || !ActionStateMachine || !ActionStateMachine.CurrentState)
                return;
            //TODO: debug condition
            ActionStateMachine.CurrentState.DrawGizmos(ActionStateMachine.CurrentFrame);
        }

        public bool OnEvent(IEvent e)
        {
        
            return false;
        }
    }
}
