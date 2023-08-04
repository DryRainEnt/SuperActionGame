using System.Collections.Generic;
using SimpleActionFramework.Actant;
using UnityEditor;
using UnityEngine;

namespace Editor.SimpleActionEditor.ActantEditor
{
    [CustomPropertyDrawer(typeof(TransformRotateActant))]
    public class TransformRotateActantDrawer : PropertyDrawer
    {
        // 각 propertyPath에 대한 별도의 foldout 상태를 저장하는 Dictionary
        private Dictionary<string, bool> foldouts = new Dictionary<string, bool>();

        private int _propertyCount;

        public int PropertyCount
        {
            get => _propertyCount;
            set => _propertyCount = Mathf.Max(value, _propertyCount);
        }
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            bool foldout = foldouts.TryGetValue(property.propertyPath, out bool storedFoldout) && storedFoldout;
            return foldout ? 24f * PropertyCount : 20;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            bool foldout = foldouts.TryGetValue(property.propertyPath, out bool storedFoldout) && storedFoldout;
            
            var pCount = 0;
            SerializedProperty startFrameProperty = property.FindPropertyRelative("StartFrame");
            SerializedProperty durationProperty = property.FindPropertyRelative("Duration");

            foldout = EditorGUI.Foldout(position, foldout, $"{startFrameProperty.intValue} ~ TransformRotateActant [{durationProperty.intValue}]", true);
            pCount++;
            
            // save foldout state on Dictionary
            foldouts[property.propertyPath] = foldout;
            
            EditorGUI.indentLevel++;
            if (foldout)
            {
                var drawRect = new Rect(position.x, position.y + 24f * pCount, position.width, position.height);
                EditorGUI.PropertyField(drawRect, startFrameProperty, 
                    new GUIContent("StartFrame"), true);
                pCount++;
                
                drawRect = new Rect(position.x, position.y + 24f * pCount, position.width, position.height);
                EditorGUI.PropertyField(drawRect, durationProperty, 
                    new GUIContent("Duration"), true);
                pCount++;
                
                drawRect = new Rect(position.x, position.y + 24f * pCount, position.width, position.height);
                SerializedProperty moveDirectionProperty = property.FindPropertyRelative("RotationAngle");
                EditorGUI.PropertyField(drawRect, moveDirectionProperty, 
                    new GUIContent("RotationAngle"), true);
                pCount++;
            }
            EditorGUI.indentLevel--;

            PropertyCount = pCount;
            
            EditorGUI.EndProperty();
        }
    }
}
