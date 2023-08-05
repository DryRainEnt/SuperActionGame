using System.Collections.Generic;
using System.Linq;
using SimpleActionFramework.Core;
using UnityEditor;
using UnityEngine;

namespace Editor.SimpleActionEditor
{
    [CustomEditor(typeof(Actor))]
    public class CharacterEditor : UnityEditor.Editor
    {
        public bool foldout = true;

        void OnEnable()
        {
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }
}
