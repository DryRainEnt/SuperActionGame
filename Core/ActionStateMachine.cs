using System.Collections.Generic;
using UnityEngine;

namespace SimpleActionFramework.Core
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "NewActionMachine", menuName = "Simple Action Framework/Create New Action Machine", order = 1)]
    public class ActionStateMachine : ScriptableObject
    {
        public SerializedDictionary<string, ActionState> States = new SerializedDictionary<string, ActionState>();
    
        public SerializedDictionary<string, object> Data = new SerializedDictionary<string, object>();

        public string CurrentStateName
            => CurrentState ? CurrentState.name : "NullState";

        public string DefaultStateName;
        public ActionState CurrentState { get; private set; }
        public Actor Actor { get; set; }

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
        
        public void SetState(string stateName)
        {
            var targetState = States.TryGetValue(stateName, out var state) ? state : States[DefaultStateName];

            if (!targetState)
            {
                Debug.LogWarning($"State is null for {name}!" +
                                 $" \n called: {stateName} \n default: {DefaultStateName}");
                return;
            }
            
            CurrentState = Instantiate(targetState);
            // CurrentState = targetState;
            CurrentState.ResetState(this, Data);
        }

        public void UpdateState(float dt)
        {
            if (!CurrentState)
            {
                Debug.LogWarning($"State is null for {name}!");
                return;
            }

            CurrentState.UpdateState(this, dt);
            
            if (CurrentState.IsFinished)
            {
                SetState(CurrentState.ReservedState);
            }
        }

        public void UpdateData(string key, object value)
        {
            Data[key] = value;
        }
    }
}
