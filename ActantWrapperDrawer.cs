using SimpleActionFramework.Core;
using UnityEditor;
using UnityEngine;

namespace Editor.SimpleActionEditor
{
	[CustomPropertyDrawer(typeof(ActantWrapper))]
	public class ActantWrapperDrawer : PropertyDrawer
	{
		SerializedProperty actantProperty;
		
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return EditorGUI.GetPropertyHeight(actantProperty) + 20f;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);
	 	 	
			actantProperty = property.FindPropertyRelative("actant");
			
			EditorGUI.PropertyField(position, actantProperty, GUIContent.none, true);
			
			EditorGUI.EndProperty();
		}
	}
}