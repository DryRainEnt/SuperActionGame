using System;
using System.Collections;
using System.Collections.Generic;
using CMF;
using Proto.BasicExtensionUtils;
using Proto.EventSystem;
using Proto.PoolingSystem;
using Resources.Scripts;
using Resources.Scripts.Events;
using SimpleActionFramework.Implements;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace SimpleActionFramework.Core
{
    public class Actor : NetworkBehaviour, IPooledObject, IEventListener
    {
        public int ActorIndex;
        [SerializeField]
        private Color _color;

        public Color OriginColor;
        public Color Color
        {
            get => _color;
            set
            {
                _color = value;
                SpriteRenderer.color = _color;
            }
        }
        
        [SerializeField]
        public ActionStateMachine DefaultActionStateMachine;
        [SerializeField]
        public ActionStateMachine ActionStateMachine;
        
        public Dictionary<string, object> Data => ActionStateMachine ? ActionStateMachine.Data : null;
    
        private ActorController _actorController;

        private Transform _spriteRoot;
        public SpriteRenderer SpriteRenderer;
        public Material SpriteMaterial => SpriteRenderer ? SpriteRenderer.material : null;
        public Collider PhysicsCollider;
    
        public string CurrentState;
        public string CurrentActantName;
        public int CurrentFrame;
    
        public float StateSpeed = 1f;

        public Vector2 LastDirection;
        public bool LockDirection;
        public bool IsLeft;

        [SerializeField]
        private Vector2 _initialPosition;
        
        public List<InputRecord> RecordedInputs = new List<InputRecord>();
        
        public Vector2 InputAxis => _actorController.CharacterInput.inputAxis.Value;
        public Vector2 CommandAxis => _actorController.CharacterInput.commandAxis.Value;

        public int controllerType = 0;
        
        public bool isInvulnerable = false;
        public int lastHitFrame = -0;
        
        public NetworkObject networkObject;

        public float shakeDuration = 0f;

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
                transform.position = _positionCache;
            }
            get => _positionCache;
        }
        private Vector2 _prevPosition;
        
        public Vector2 Velocity => Position - _prevPosition;

        public int DeathCount;
        public int MaxLifeCount = 5;
        public int LifeCount = 5;
        public int KillCount = 0;

        public float[] Intention => _actorController.CharacterInput ? _actorController.CharacterInput.intention : new []{0f};

        public void Initiate()
        {
            _actorController = GetComponent<ActorController>();
            SpriteRenderer.color = OriginColor;
            Color = OriginColor;
            _initialPosition = transform.position;
            _spriteRoot = SpriteRenderer.transform;
            
            networkObject = GetComponent<NetworkObject>();
        
            ActionStateMachine = Instantiate(DefaultActionStateMachine);
            ActionStateMachine.Init(this);
            
            _actorController.OnJump = (v) =>
            {
                RecordedInputs.Clear();
            };
            
            ActionStateMachine.UpdateData(Constants.DefaultDataKeys[DefaultKeys.INTERACTION], "Neutral");
            
            MessageSystem.Subscribe(typeof(OnAttackHitEvent), this);
            MessageSystem.Subscribe(typeof(OnAttackGuardEvent), this);

            DeathCount = 0;
            LifeCount = MaxLifeCount;
        }

        public void ChangeActorControl(int index)
        {
            var control = GetComponent<CharacterInput>();
            if (control)
                DestroyImmediate(control);
            
            switch (index)
            {
                case 0:
                    gameObject.AddComponent<CharacterInput>();
                    break;
                case 1:
                    // Keyboard
                    gameObject.AddComponent<CharacterKeyboardInput>();
                    break;
                case 2:
                    // AI
                    gameObject.AddComponent<CharacterArtificialInput>();
                    break;
                case 3:
                    // NN
                    gameObject.AddComponent<NeuralNetworkInput>();
                    break;
                case 4:
                    // Net
                    gameObject.AddComponent<NetworkPlayerInput>();
                    break;
            }
            UpdateController();
            
            controllerType = index;
        }
        
        private void UpdateController()
        {
            _actorController = GetComponent<ActorController>();
            _actorController.CharacterInput = GetComponent<CharacterInput>();
        }

        private void OnEnable()
        {
            ObjectPoolController.GetOrCreate("DamageTextFX", "Effects");
            ObjectPoolController.GetOrCreate("SlashFX", "Effects");
            ObjectPoolController.GetOrCreate("GuardFX", "Effects");
            ObjectPoolController.GetOrCreate("DeathFX", "Effects");
            ObjectPoolController.GetOrCreate("ReviveFX", "Effects");
            ObjectPoolController.GetOrCreate("PlayerMarker", "Effects");
        }

        public void ResetPosition()
        {
            Position = _initialPosition;
            SetVelocity(Vector2.zero);
            HP = MaxHP;
        }

        private IEnumerator Dead()
        {
            while (CurrentState != "Lying")
                yield return null;
            
            DeathCount++;
            LifeCount = MaxLifeCount - DeathCount;

            MessageSystem.Publish(OnDeathEvent.Create(ActorIndex, DeathCount));
            
            if (_actorController.CharacterInput is CharacterKeyboardInput)
                CameraTracker.Instance.Track(Position);
            
            var fx = ObjectPoolController.InstantiateObject("DeathFX", 
                new PoolParameters(Position)) as DeathFX;
            fx.Initialize(Color);
            
            if (LifeCount <= 0)
            {
                Game.Instance.survivedPlayers.Remove(ActorIndex);
             
                yield return new WaitForSeconds(1.2f);

                if (Game.Instance.survivedPlayers.Count == 1)
                    MessageSystem.Publish(OnGameEndEvent.Create(Game.Instance.survivedPlayers[0]));
            
            }
            else{
                Timer timer = new Timer(2f)
                {
                    Alarm = Revive
                };
            }

            gameObject.SetActive(false);
            
        }

        public void Revive()
        {
            Game.Instance.StartCoroutine(OnRevive());
        }
        
        public IEnumerator OnRevive()
        {
            while (Game.IsWriting)
            {
                yield return null;
            }
            
            if (_actorController.CharacterInput is CharacterKeyboardInput)
            {
                CameraTracker.Instance.Track(transform, Vector2.up * 24f);
                Game.Player = this;

                var _ = ObjectPoolController.InstantiateObject("PlayerMarker", 
                    new PoolParameters(_initialPosition)) as PlayerMarker;
            }
            
            ResetPosition();
            var fx = ObjectPoolController.InstantiateObject("ReviveFX", 
                new PoolParameters(_initialPosition)) as DeathFX;
            fx.Initialize(Color);

            MessageSystem.Publish(OnReviveEvent.Create(ActorIndex, DeathCount));

            yield return new WaitForSeconds(0.6f);
            
            gameObject.SetActive(true);
            
            ActionStateMachine.SetState("Idle");
            isInvulnerable = true;
            var originColor = Color;
            var flash = 0;

            while (flash < 180){
                Color = flash % 6 < 3 ? Color.white : originColor;
                flash++;
                yield return null;
            }

            Color = originColor;

            isInvulnerable = false;
        }

        // Update is called once per frame
        void Update()
        {
            if (!Game.IsPlayable)
                return;

            var dt = StateSpeed * Time.deltaTime;
            
        
            if (GlobalInputController.Instance.GetPressed("reset"))
            {
                ResetPosition();
            }
            
            InputUpdate();
        
            ActionStateMachine.OnUpdate(this, dt);
        
            CurrentState = ActionStateMachine.CurrentStateName;
            CurrentFrame = ActionStateMachine.CurrentFrame;

            Position = transform.position;
        }

        private void LateUpdate()
        {
            var dt = Time.deltaTime;

            if (shakeDuration > 0)
            {
                shakeDuration -= dt;

                var offsetToken = ((shakeDuration * 100f) % 2) - 0.5f;
                _spriteRoot.localPosition = Vector3.left * (offsetToken * shakeDuration * 8f);
            }
            else
            {
                shakeDuration = 0f;
                _spriteRoot.localPosition = Vector3.zero;
            }
            
        }

        public readonly Dictionary<string, bool> CurrentInputs = new ()
        {
            {"button1", false},
            {"button2", false},
            {"button3", false},
            {"button4", false},
            {"forward", false},
            {"backward", false},
            {"up", false},
            {"down", false},
            {"debug", false},
        };
        
        [SerializeField]
        private UnityEngine.UI.Text _debugText;
        
        private void InputUpdate()
        {
            RecordedInputs.Sort();
            
            Color = OriginColor;
            
            _actorController.CharacterInput.InputCheck(this);
            
            
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
                    if (lastHitFrame + 2 >= Time.frameCount)
                        return false;
                    
                    if (isInvulnerable)
                    {
                        var guardfx = ObjectPoolController.InstantiateObject("GuardFX", new PoolParameters(info.Point)) as GuardFX;
                        guardfx.Initialize(Color.white);
                        return false;
                    }
                    
                    var isLeft = giver.IsLeft;
                    var knockBack = info.Direction.normalized.doFlipX(isLeft) * 4f;

                    var prevHP = HP;
                    
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

                    if (prevHP > 0f && HP <= 0f)
                    {
                        giver.KillCount++;
                    }

                    shakeDuration = 0.2f;
                    
                    StateSpeed = 0.1f;
                    giver.StateSpeed = 0.3f;
                    var _ = new Timer(0.2f)
                    {
                        Alarm = () =>
                        {
                            StateSpeed = 1f;
                            giver.StateSpeed = 1f;
                        }
                    };
                    
                    lastHitFrame = Time.frameCount;
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
                    var guardfx = ObjectPoolController.InstantiateObject("GuardFX", new PoolParameters(info.Point)) as GuardFX;
                    guardfx.Initialize(info.Color);

                    var knockBack = (taker.IsLeft ? info.Direction.normalized.FlipX() : info.Direction.normalized);
                    ActionStateMachine.SetState(info.NextStateOnSuccessToReceiver);
                    AddExternalVelocity(knockBack * info.KnockbackPower);
                    return true;
                }
            }
            return false;
        }

        public string Name { get; set; } = "Actor";
        
        public void OnPooled()
        {
            
        }

        public void Dispose()
        {
            MessageSystem.Unsubscribe(typeof(OnAttackHitEvent), this);
            MessageSystem.Unsubscribe(typeof(OnAttackGuardEvent), this);
            
            Destroy(ActionStateMachine);

            ObjectPoolController.Dispose(this);
        }
    }
}
