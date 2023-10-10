using System.Collections;
using System.Collections.Generic;
using SimpleActionFramework.Core;
using UnityEngine;

namespace CMF
{
    //This abstract character input class serves as a base for all other character input classes;
    //The 'Controller' component will access this script at runtime to get input for the character's movement (and jumping);
    //By extending this class, it is possible to implement custom character input;
    public abstract class CharacterInput : MonoBehaviour
    {
        public Vector2 prevInputAxis;
        public Vector2 inputAxis;
		
        public Vector2 prevCommandAxis;
        public Vector2 commandAxis;

        public float[] intention = {0f, 0f, 0f, 0f, 0f};
        
        public abstract float GetHorizontalMovementInput();
        public abstract float GetVerticalMovementInput();

        public abstract bool IsJumpKeyPressed();
        public abstract void InputCheck(Actor actor, string key);
    }
}
