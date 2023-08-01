using UnityEngine;

namespace SimpleActionFramework.Core
{
    [System.Serializable]
    public class ActionStateMachine : ScriptableObject
    {
        public SerializedDictionary<string, ActionState> States = new SerializedDictionary<string, ActionState>();
    
        public string CurrentStateName
            => States.ContainsValue(CurrentState) ? States.GetKey(CurrentState) : "NullState";

        public ActionState DefaultState;
        public ActionState CurrentState { get; private set; }
        
        public void SetState(string stateName)
        {
            if (States.ContainsKey(stateName))
            {
                CurrentState = States[stateName];
            }
        }
    }
}
