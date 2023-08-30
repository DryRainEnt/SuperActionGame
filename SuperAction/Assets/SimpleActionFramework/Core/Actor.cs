using System.Collections.Generic;
using System.Threading.Tasks;
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
        public Material SpriteMaterial => SpriteRenderer ? SpriteRenderer.material : null;
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
            
            ActionStateMachine.UpdateData(Constants.DefaultDataKeys[DefaultKeys.INPUT], RecordedInputs);

            ActionStateMachine.UpdateData(Constants.DefaultDataKeys[DefaultKeys.MOVE], 
                _actorController.GetMovementVelocity().x.Abs() < Constants.Epsilon ? 0f : _actorController.GetMovementVelocity().x.Sign());
            LastDirection = _actorController.GetMovementVelocity().normalized;
        
            IsLeft = LastDirection.x > Constants.Epsilon ? false : LastDirection.x < -Constants.Epsilon ? true : IsLeft;
            ActionStateMachine.UpdateData(Constants.DefaultDataKeys[DefaultKeys.FACE], IsLeft ? -1f : 1f);
            ActionStateMachine.UpdateData(Constants.DefaultDataKeys[DefaultKeys.VSPEED], _actorController.GetMovementVelocity().y);
            ActionStateMachine.UpdateData(Constants.DefaultDataKeys[DefaultKeys.GROUND], _actorController.IsGrounded() ? 1f : 0f);

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
        
        private static readonly int OverlayColor = Shader.PropertyToID("_OverlayColor");

        private readonly Color _white = Color.white;
        private readonly Color _transparent = Color.clear;
        
        public void SetSprite(Sprite sprite, bool? overridenXFlip = null)
        {
            SpriteRenderer.sprite = sprite;
            SpriteRenderer.flipX = overridenXFlip ?? IsLeft;
        }
        
        private void ColorOverlay(Color color)
        {
            SpriteMaterial.SetColor(OverlayColor, color);
        }

        private async void Blink(int count = 0)
        {
            bool isWhite = true;
            do
            {
                ColorOverlay(isWhite ? _white : _transparent);
                isWhite = !isWhite;
                await Task.Delay(100);
            } while (count-- > 0);
            
            ResetOverlay();
        }
        
        private void ResetOverlay()
        {
            SpriteMaterial.SetColor(OverlayColor, _transparent);
        }

        #endregion

        private void OnDrawGizmos()
        {
            return;
            // if (!Application.isPlaying || !ActionStateMachine || !ActionStateMachine.CurrentState)
            //     return;
            //TODO: debug condition
            // ActionStateMachine.CurrentState.DrawGizmos(ActionStateMachine.CurrentFrame);
        }

        public bool OnEvent(IEvent e)
        {
        
            return false;
        }
    }
}
