using UnityEngine;

namespace SimpleActionFramework.Core
{
    [System.Serializable]
    public class ActionStateMachine : ScriptableObject
    {
        public SerializedDictionary<string, ActionState> States = new SerializedDictionary<string, ActionState>();
    
        public SerializedDictionary<string, string> Data = new SerializedDictionary<string, string>();

        public string CurrentStateName
            => States.ContainsValue(CurrentState) ? States.GetKey(CurrentState) : "NullState";

        public ActionState DefaultState;
        public ActionState CurrentState { get; private set; }
        public Character Character { get; set; }

        public void Init(Character character)
        {
            Character = character;
            Data.Clear();
        }
        
        public void SetState(string stateName)
        {
            CurrentState = States.ContainsKey(stateName) ? States[stateName] : DefaultState;
            CurrentState.ResetState(Character, Data);
        }

        public void UpdateState(float dt)
        {
            CurrentState.UpdateState(this, dt);
            
            if (CurrentState.IsFinished)
            {
                SetState(CurrentState.ReservedState);
            }
        }
    }
}
