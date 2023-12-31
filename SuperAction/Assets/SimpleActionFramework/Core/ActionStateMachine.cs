using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SimpleActionFramework.Core
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "NewActionMachine", menuName = "Simple Action Framework/Create New Action Machine", order = 1)]
    public class ActionStateMachine : ScriptableObject
    {
        public SerializedDictionary<string, ActionState> States = new SerializedDictionary<string, ActionState>();
    
        // 박싱이 빈번하게 일어나는 중. 수정 필요
        public Dictionary<string, object> Data = new Dictionary<string, object>();

        public Dictionary<SingleActant, ActantState> ActantStates = new Dictionary<SingleActant, ActantState>();
        
        private Dictionary<SingleActant, IDisposable> _disposables = new Dictionary<SingleActant, IDisposable>();
        public Dictionary<SingleActant, IDisposable> Disposables => _disposables;

        public string CurrentStateName = "null";

        public string DefaultStateName;
        public ActionState CurrentState { get; private set; }
        public Actor Actor { get; set; }
        
        public ActionState this[string key]
        {
            get => States[key];
            set => States[key] = value;
        }
        
        private ActionState _passiveState;

        public ActionState PassiveState
        {
            get
            {
                if (_passiveState) return _passiveState;
                if (States.TryGetValue("Passive", out var state))
                    return _passiveState = state;
                
                
	
	#if UNITY_EDITOR
                var newName = $"{name}PassiveState";
                ActionState newState = CreateInstance<ActionState>();
                string path = $"Assets/SimpleActionFramework/ActionState/{name}";
                path += $"/{newName}.asset";
                AssetDatabase.CreateAsset(newState, path);
                States.Add("Passive", newState);
                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                    
                _passiveState = newState;
	#endif

                return _passiveState;
            }
        }
        
        public int GetId => GetInstanceID();
        
        private float _innerTimer;

        public int CurrentFrame
        {
            get => Mathf.FloorToInt(_innerTimer * Constants.DefaultActionFrameRate);
            set => _innerTimer = value / Constants.DefaultActionFrameRate;
        }
        private int _previousFrame = 0;
		
        public bool IsFinished => CurrentState && CurrentFrame > CurrentState.TotalDuration;


        public void Add(string key, ActionState data)
        {
            States.Add(new KeyValuePair<string, ActionState>(key, data));
        }
        
        public void Init(Actor actor)
        {
            Actor = actor;
            Data.Clear();
            
            SetState(DefaultStateName);
        }
        
        public void Flush()
        {
            foreach (var (key, disposable) in _disposables)
            {
                disposable.Dispose();
            }
            _disposables.Clear();
        }
        
        public void RegisterDisposable(SingleActant key, IDisposable disposable)
        {
            _disposables.Add(key, disposable);
        }
        
        public void DisposeDisposable(SingleActant key)
        {
            var disposable = _disposables[key];
            _disposables.Remove(key);
            disposable.Dispose();
        }
        
        public T[] FindAllActant<T>() where T : SingleActant
        {
            return CurrentState.Actants.FindAll(actant => actant.GetType() == typeof(T)) as T[];
        }

        public void RewindState(int frame = 0)
        {
            CurrentFrame = frame;
        }
        
        public void SetState()
        {
            SetState(DefaultStateName);
        }
        
        public void SetState(string stateName)
        {
            var targetState = States.TryGetValue(stateName, out var state) ? state : States[DefaultStateName];

            if (!targetState)
            {
                Debug.LogWarning($"State is null for {name}!" +
                                 $" \n called: {stateName} \n default: {DefaultStateName}");
                return;
            }
            
            foreach (var (act, ast) in ActantStates)
            {
                if (ast != ActantState.Running)
                    continue;
                act.OnFinished(Actor);
            }
            ActantStates.Clear();
            Flush();
            Actor.LockDirection = false;
            Actor.SetGravityFactor();
            CurrentStateName = stateName;
            
            CurrentState = targetState;
            ResetState();
        }

        public void OnUpdate(Actor actor, float dt)
        {
            if (!CurrentState)
            {
                Debug.LogWarning($"State is null for {name}!");
                return;
            }

            UpdateState(actor, dt);
            
            if (IsFinished)
            {
                SetState();
            }
        }
        
		
        public void ResetState()
        {
            _innerTimer = 0f;
            _previousFrame = CurrentFrame;

            ActantStates.Clear();
            foreach (var act in CurrentState.Actants)
            {
                ActantStates.Add(act, ActantState.NotStarted);
                act.GetId(Actor);
            }
        }
		
        public void UpdateState(Actor actor, float dt)
        {
            _previousFrame = CurrentFrame;
            _innerTimer += dt;

            foreach (var act in CurrentState.Actants)
            {
                var progress = Mathf.Clamp01((_innerTimer - act.StartFrame / Constants.DefaultActionFrameRate)
                                             / ((float)act.Duration / Constants.DefaultActionFrameRate));
				
                UpdateActant(act, actor, progress);
            }

            foreach (var act in PassiveState.Actants)
            {
                act.Act(actor, 1f);
            }
        }
		
        private bool UpdateActant(SingleActant act, Actor actor, float progress)
        {
            if (!ActantStates.ContainsKey(act))
                return false;
            
            if (ActantStates[act] == ActantState.Finished || act.StartFrame > CurrentFrame)
                return false;
			
            var isFirst = act.EndFrame > CurrentFrame
                          && act.StartFrame <= CurrentFrame
                          && ActantStates[act] == ActantState.NotStarted;

            if (isFirst)
            {
                ActantStates[act] = ActantState.Running;
                act.OnStart(actor);
            }
            if (act.EndFrame < CurrentFrame)
            {
                ActantStates[act] = ActantState.Finished;
                act.OnFinished(actor);
            }
            if (act.StartFrame > CurrentFrame)
            {
                ActantStates[act] = ActantState.NotStarted;
                act.OnReset(actor);
            }

            act.Act(actor, progress, isFirst);

            return true;
        }

        public void UpdateData(string key, object value)
        {
            Data[key] = value;
        }
    }
}
