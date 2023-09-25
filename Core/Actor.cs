using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using CMF;
using Proto.BasicExtensionUtils;
using Proto.EventSystem;
using Proto.PoolingSystem;
using Resources.Scripts;
using Resources.Scripts.Events;
using SimpleActionFramework.Implements;
using Sirenix.OdinInspector;
using UnityEngine;

namespace SimpleActionFramework.Core
{
    public class Actor : MonoBehaviour, IEventListener
    {
        public int ActorIndex;
        [SerializeField]
        private Color _color;

        public Color Color
        {
            get => _color;
            private set
            {
                _color = value;
                SpriteRenderer.color = _color;
            }
        }
        
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
        public bool LockDirection;
        public bool IsLeft;

        private Vector2? _initialPosition;
        
        public List<InputRecord> RecordedInputs = new List<InputRecord>();
        
        public int[] HitMaskIds;
        
        public bool isAlive => HP > 0;
        
        public readonly float MaxHP = 100f;
        private float _hp = 100f;
        public float HP
        {
            get => _hp;
            set
            {
                var prev = _hp;
                _hp = value;

                if (_hp <= 0)
                {
                    if (prev > 0)
                        StartCoroutine(Dead());
                    _hp = 0;
                }
                
                ActionStateMachine.UpdateData(Constants.DefaultDataKeys[DefaultKeys.HP], _hp);
                MessageSystem.Publish(OnHealthUpdatedEvent.Create(ActorIndex,  prev, _hp));
            }
        }

        private Vector2 _positionCache;
        public Vector2 Position 
        {
            set
            {
                _prevPosition = Position;
                _positionCache = value;
            }
            get => _positionCache;
        }
        private Vector2 _prevPosition;
        
        public Vector2 Velocity => Position - _prevPosition;

        public int DeathCount;

        private void Initiate()
        {
            _actorController = GetComponent<ActorController>();
            SpriteRenderer.color = Color;
        
            ActionStateMachine = Instantiate(ActionStateMachine);
            ActionStateMachine.Init(this);
            
            _actorController.OnJump = (v) =>
            {
                RecordedInputs.Clear();
            };
            
            //TODO: ActorIndex가 겹치지 않도록 고유화 단계를 분리해줘야 함.
            Game.Instance.RegisteredActors.Add(ActorIndex, this);
            
            ActionStateMachine.UpdateData(Constants.DefaultDataKeys[DefaultKeys.INTERACTION], "Neutral");

            _initialPosition ??= Position;
        }

        private void OnEnable()
        {
            Initiate();

            ObjectPoolController.GetOrCreate("DamageTextFX", "Effects");
            ObjectPoolController.GetOrCreate("SlashFX", "Effects");
            ObjectPoolController.GetOrCreate("DeathFX", "Effects");
            
            MessageSystem.Subscribe(typeof(OnAttackHitEvent), this);
            MessageSystem.Subscribe(typeof(OnAttackGuardEvent), this);
        }
        
        private void OnDisable()
        {
            MessageSystem.Unsubscribe(typeof(OnAttackHitEvent), this);
            MessageSystem.Unsubscribe(typeof(OnAttackGuardEvent), this);
            
            Game.Instance?.RegisteredActors?.Remove(ActorIndex);
        }

        public void ResetPosition()
        {
            transform.position = _initialPosition.Value;
            SetVelocity(Vector2.zero);
            HP = MaxHP;
        }

        private IEnumerator Dead()
        {
            while (CurrentState != "Lying")
                yield return null;
            
            DeathCount++;

            MessageSystem.Publish(OnDeathEvent.Create(ActorIndex, DeathCount));
            
            var fx = ObjectPoolController.InstantiateObject("DeathFX", new PoolParameters(Position)) as DeathFX;
            fx.Initialize(Color);
            
            gameObject.SetActive(false);
            
            Timer timer = new Timer(2f)
            {
                Alarm = Revive
            };
        }

        public void Revive()
        {
            Game.Instance.StartCoroutine(OnRevive());
        }
        
        private IEnumerator OnRevive()
        {
            while (Game.IsWriting)
            {
                yield return null;
            }
            ResetPosition();
            gameObject.SetActive(true);
        }

        // Update is called once per frame
        void Update()
        {
            var dt = StateSpeed * Time.deltaTime;

            InputUpdate();
        
            ActionStateMachine.OnUpdate(this, dt);
        
            CurrentState = ActionStateMachine.CurrentStateName;
            CurrentFrame = ActionStateMachine.CurrentFrame;

            Position = transform.position;
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
            if (_actorController.CharacterInput is CharacterArtificialInput ai)
            {
                Color = ai.UseAI ? Color.red : Color.yellow;
                _actorController.CharacterInput.InputCheck(this, null);
            }
            else if (_actorController.CharacterInput is CharacterKeyboardInput or CharacterJoystickInput)
            {
                foreach (var key in ActionKeys)
                {
                    _actorController.CharacterInput.InputCheck(this, key);
                }
            }
            else
            {
                Color = Color.white;
            }
            RecordedInputs.Sort();

            while (RecordedInputs.Count > 0 && RecordedInputs[^1].TimeStamp < Time.realtimeSinceStartup - 0.5f)
            {
                RecordedInputs.RemoveAt(RecordedInputs.Count - 1);
            }
            
            if (isAlive)
                ActionStateMachine.UpdateData(Constants.DefaultDataKeys[DefaultKeys.INPUT], RecordedInputs);
            else
            {
                RecordedInputs.Clear();
            }

            ActionStateMachine.UpdateData(Constants.DefaultDataKeys[DefaultKeys.MOVE], 
                _actorController.GetMovementVelocity().x.Abs() < Constants.Epsilon ? 0f : _actorController.GetMovementVelocity().x.Sign());
            LastDirection = _actorController.GetMovementVelocity().normalized;
        
            if (!LockDirection)
                IsLeft = LastDirection.x > Constants.Epsilon ? false : LastDirection.x < -Constants.Epsilon ? true : IsLeft;
            
            ActionStateMachine.UpdateData(Constants.DefaultDataKeys[DefaultKeys.FACE], IsLeft ? -1f : 1f);
            ActionStateMachine.UpdateData(Constants.DefaultDataKeys[DefaultKeys.VSPEED], _actorController.GetMovementVelocity().y);
            ActionStateMachine.UpdateData(Constants.DefaultDataKeys[DefaultKeys.GROUND], _actorController.IsGrounded() ? 1f : 0f);

            if (_debugText)
            {
                _debugText.text = "";
                foreach (var input in RecordedInputs)
                {
                    _debugText.text += $"{input.Key} : {input.TimeStamp}\n";
                }
            }
        }

        public void LookDirection(float x)
        {
            LastDirection.x = x;
        }
        
        public void SetVelocity(Vector2 velocity)
        {
            _actorController.SetMovementVelocity(velocity);
            _actorController.SetVerticalSpeed(velocity.y);
        }
        
        public void SetVerticalSpeed(float spd)
        {
            _actorController.SetVerticalSpeed(spd);
        }
        
        public void AddExternalVelocity(Vector2 velocity)
        {
            _actorController.AddExternalVelocity(velocity);
        }

        public void SetExternalVelocity(Vector2 velocity)
        {
            _actorController.SetExternalVelocity(velocity);
        }
        
        public void AddVerticalVelocity(float velocity)
        {
            _actorController.AddVerticalSpeed(velocity);
        }
        
        public void SetGravityFactor(float factor = 1f)
        {
            _actorController.SetGravityFactor(factor);
        }
        
        public void ToggleGravity(bool toggle)
        {
            _actorController.ToggleGravity(toggle);
        }
        
        public void ToggleCharacterInput(bool toggle)
        {
            _actorController.ToggleCharacterInput(toggle);
        }

        public bool useCharacterInput => _actorController.useCharacterInput;

        #region SpriteSetters
        
        private static readonly int OverlayColor = Shader.PropertyToID("_OverlayColor");

        private readonly Color _white = Color.white;
        private readonly Color _transparent = Color.clear;
        
        public void SetSprite(Sprite sprite, bool? overridenXFlip = null)
        {
            SpriteRenderer.sprite = sprite;
            SpriteRenderer.flipX = overridenXFlip ?? IsLeft;
        }

        #endregion

        public bool OnEvent(IEvent e)
        {
            if (e is OnAttackHitEvent hit)
            {
                var info = hit.info;
                var giver = hit.giverMask.Owner;
                var taker = hit.takerMask.Owner;
                
                if (giver is null || giver == this)
                    return false;

                // 공격이 맞았고 피격자가 자신일 때
                if (taker == this)
                {
                    var isLeft = giver.IsLeft;
                    var knockBack = info.Direction.normalized.doFlipX(isLeft) * 4f;

                    HP -= info.Damage;
                    ActionStateMachine.SetState(HP > 0 ? info.NextStateOnSuccessToReceiver : "HitLarge");
                    SetVelocity(Vector2.zero);
                    SetVerticalSpeed(0f);
                    AddExternalVelocity(knockBack * info.KnockbackPower);
                    LookDirection(-giver.LastDirection.x);
                    
                    var fx = ObjectPoolController.InstantiateObject("DamageTextFX", new PoolParameters(info.Point)) as DamageTextFX;
                    fx.Initialize(info.Damage.ToString(), knockBack);
                    
                    var slashfx = ObjectPoolController.InstantiateObject("SlashFX", new PoolParameters(info.Point)) as SlashFX;
                    slashfx.Initialize(info.Damage * 0.03f, knockBack, info.Color);

                    Time.timeScale = 0.1f;
                    var _ = new Timer(0.1f)
                    {
                        Alarm = () =>
                        {
                            Time.timeScale = 1f;
                        }
                    };
                    return true;
                }
            }
            if (e is OnAttackGuardEvent guard)
            {
                var info = guard.info;
                var giver = guard.giverMask.Owner;
                var taker = guard.takerMask.Owner;
                
                if (taker is null || taker == this)
                    return false;

                // 공격이 가드당했고 공격자가 자신일 때
                if (giver == this)
                {
                    var knockBack = (taker.IsLeft ? info.Direction.normalized.FlipX() : info.Direction.normalized);
                    ActionStateMachine.SetState(info.NextStateOnSuccessToReceiver);
                    AddExternalVelocity(knockBack * info.KnockbackPower);
                    return true;
                }
            }
            return false;
        }
    }
}
