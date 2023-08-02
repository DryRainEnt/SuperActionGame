using SimpleActionFramework.Actant;
using UnityEditor;
using UnityEngine;

namespace Editor.SimpleActionEditor.ActantEditor
{
    [CustomPropertyDrawer(typeof(TransformMoveActant))]
    public class TransformMoveActantDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            base.OnGUI(position, property, label);

            EditorGUILayout.LabelField("TransformMoveActant");
            
            EditorGUI.BeginProperty(position, label, property);
        }
    }
}
