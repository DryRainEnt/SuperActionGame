using System.Collections.Generic;
using UnityEngine;

namespace SimpleActionFramework.Core
{
    public static class Constants
    {
        public const float Epsilon = 0.00001f;
        public const float DefaultActionFrameRate = 60;
        public const float DefaultPPU = 16;
        
        public static readonly Dictionary<DefaultKeys, string> DefaultDataKeys = new Dictionary<DefaultKeys, string>()
        {
            { DefaultKeys.INPUT, "Inputs"},
            { DefaultKeys.MOVE, "MoveDirection"},
            { DefaultKeys.FACE, "FaceDirection"},
            { DefaultKeys.VSPEED, "VerticalSpeed"},
            { DefaultKeys.GROUND, "IsGrounded"},
            { DefaultKeys.INTERACTION, "InteractionState"},
            { DefaultKeys.CUSTOM , ""},
        };
    }
    
    public enum DefaultKeys
    {
        /// <summary>
        /// Inputs
        /// </summary>
        INPUT,
        /// <summary>
        /// Number
        /// </summary>
        MOVE,
        /// <summary>
        /// Number
        /// </summary>
        FACE,
        /// <summary>
        /// Number
        /// </summary>
        VSPEED,
        /// <summary>
        /// Number (0 or 1)
        /// </summary>
        GROUND,
        /// <summary>
        /// String
        /// </summary>
        INTERACTION,
        
        CUSTOM = 999,
    }
}
