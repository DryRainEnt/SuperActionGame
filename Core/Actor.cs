using Proto.BasicExtensionUtils;
using Proto.EventSystem;
using SimpleActionFramework.Implements;
using UnityEngine;

namespace SimpleActionFramework.Core
{
    public class Actor : MonoBehaviour, IEventListener
    {
        [SerializeField]
        public ActionStateMachine ActionStateMachine;
    
        private ActorController _actorController;
    
        public SpriteRenderer SpriteRenderer;
        public BoxCollider PhysicsCollider;
    
        public string CurrentState;
        public string CurrentActantName;
        public int CurrentFrame;
    
        public float StateSpeed = 1f;

        public Vector2 LastDirection;
        public bool IsLeft;

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

            var dx = _actorController.GetMovementVelocity().x;
            ActionStateMachine.UpdateData("MoveDirection", 
                dx.Abs() < Constants.Epsilon ? 0f : dx.Sign());
            LastDirection = _actorController.GetMovementVelocity().normalized;
        
            IsLeft = LastDirection.x > 0f ? false : LastDirection.x < 0f ? true : IsLeft;
        
            ActionStateMachine.UpdateState(this, dt);
        
            CurrentState = ActionStateMachine.CurrentStateName;
            CurrentActantName = ActionStateMachine.CurrentState.CurrentActantName;
            CurrentFrame = ActionStateMachine.CurrentState.CurrentFrame;
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
            ActionStateMachine.OnDrawGizmos();
        }

        public bool OnEvent(IEvent e)
        {
        
            return false;
        }
    }
}
