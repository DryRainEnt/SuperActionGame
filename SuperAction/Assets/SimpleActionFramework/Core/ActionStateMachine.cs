using UnityEngine;

namespace SimpleActionFramework.Core
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "New Action Machine", menuName = "Simple Action Framework/Create New Action Machine", order = 1)]
    public class ActionStateMachine : ScriptableObject
    {
        public SerializedDictionary<string, ActionState> States = new SerializedDictionary<string, ActionState>();
    
        public SerializedDictionary<string, string> Data = new SerializedDictionary<string, string>();

        public string CurrentStateName
            => States.ContainsValue(CurrentState) ? States.GetKey(CurrentState) : "NullState";

        public string DefaultStateName;
        public ActionState CurrentState { get; private set; }
        public Character Character { get; set; }

        public void Init(Character character)
        {
            Character = character;
            Data.Clear();
        }
        
        public void SetState(string stateName)
        {
            CurrentState = States.ContainsKey(stateName) ? States[stateName] : States[DefaultStateName];

            if (!CurrentState)
            {
                Debug.LogWarning($"State is null for {name}!" +
                                 $" \n called: {stateName} \n default: {DefaultStateName}");
                return;
            }
            
            CurrentState.ResetState(Character, Data);
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
    }
}
