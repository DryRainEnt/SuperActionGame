using System.Collections;
using System.Collections.Generic;
using SimpleActionFramework.Core;
using Unity.Netcode;
using UnityEngine;

namespace CMF
{
    //This abstract character input class serves as a base for all other character input classes;
    //The 'Controller' component will access this script at runtime to get input for the character's movement (and jumping);
    //By extending this class, it is possible to implement custom character input;
    public abstract class CharacterInput : NetworkBehaviour
    {
        public NetworkVariable<Vector2> prevInputAxis 
            = new NetworkVariable<Vector2>(Vector2.zero,
                NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Owner);
        public NetworkVariable<Vector2> inputAxis
            = new NetworkVariable<Vector2>(Vector2.zero,
                NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Owner);
		
        public NetworkVariable<Vector2> prevCommandAxis
            = new NetworkVariable<Vector2>(Vector2.zero,
                NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Owner);
        public NetworkVariable<Vector2> commandAxis
            = new NetworkVariable<Vector2>(Vector2.zero,
                NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Owner);

        public float[] intention = {0f, 0f, 0f, 0f, 0f};

        protected readonly string[] ActionKeys = new[]
        {
            "button1",
            "button2",
            "button3",
            "button4",
            "forward",
            "backward",
            "up",
            "down",
            "debug",
        };

        public abstract float GetHorizontalMovementInput();
        public abstract float GetVerticalMovementInput();

        public abstract bool IsJumpKeyPressed();
        public abstract void InputCheck(Actor actor);
    }
}
