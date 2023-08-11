using System;
using System.Collections.Generic;
using UnityEngine;

namespace SimpleActionFramework.Core
{
    [Serializable]
    public struct FrameData
    {
        public Sprite Sprite;
        public Vector2 Offset;

        public Bounds[] HitBoxes;
        public Bounds[] DamageBoxes;
        public Bounds[] GuardBoxes;
    
        // Further data lie here
    }

}
