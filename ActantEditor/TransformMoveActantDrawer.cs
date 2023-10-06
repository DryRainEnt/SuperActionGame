using System.Collections.Generic;
using SimpleActionFramework.Actant;
using UnityEditor;
using UnityEngine;

namespace Editor.SimpleActionEditor.ActantEditor
{
    [CustomPropertyDrawer(typeof(TransformMoveActant))]
    public class TransformMoveActantDrawer : PropertyDrawer
    {
        private int _propertyCount;

        public int PropertyCount
        {
            get => _propertyCount;
            set => _propertyCount = Mathf.Max(value, _propertyCount);
        }
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 24f * PropertyCount;
        }
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var pCount = 0;
            SerializedProperty startFrameProperty = property.FindPropertyRelative("StartFrame");
            SerializedProperty durationProperty = property.FindPropertyRelative("Duration");


            var drawRect = new Rect(position.x, position.y + 24f * pCount, position.width, position.height);
            EditorGUI.PropertyField(drawRect, startFrameProperty, 
                new GUIContent("StartFrame"), true);
            pCount++;
                
            drawRect = new Rect(position.x, position.y + 24f * pCount, position.width, position.height);
            EditorGUI.PropertyField(drawRect, durationProperty, 
                new GUIContent("Duration"), true);
            pCount++;
                
            drawRect = new Rect(position.x, position.y + 24f * pCount, position.width, position.height);
            SerializedProperty moveDirectionProperty = property.FindPropertyRelative("MoveDirection");
            EditorGUI.LabelField(drawRect, "MoveDirection");
            pCount++;
            drawRect = new Rect(position.x, position.y + 24f * pCount, position.width, position.height);
            moveDirectionProperty.vector3Value = EditorGUI.Vector3Field(drawRect, "", moveDirectionProperty.vector3Value);
            pCount++;
                
            drawRect = new Rect(position.x, position.y + 24f * pCount, position.width, position.height);
            SerializedProperty interpolationProperty = property.FindPropertyRelative("InterpolationType");
            EditorGUI.PropertyField(drawRect, interpolationProperty, 
                new GUIContent("InterpolationType"), true);
            pCount++;
                
            drawRect = new Rect(position.x, position.y + 24f * pCount, position.width, position.height);
            SerializedProperty moveRelativeProperty = property.FindPropertyRelative("IsRelative");
            EditorGUI.PropertyField(drawRect, moveRelativeProperty, 
                new GUIContent("IsRelative"), true);
            pCount++;

            PropertyCount = pCount;
            
            EditorGUI.EndProperty();
        }
    }
}
