using System.Collections.Generic;
using SimpleActionFramework.Actant;
using UnityEditor;
using UnityEngine;

namespace Editor.SimpleActionEditor.ActantEditor
{
	[CustomPropertyDrawer(typeof(SetActionStateActant))]
	public class SetActionStateActantDrawer : PropertyDrawer
	{
	 	private int _propertyCount;
	    
	    private Color DefaultGUIColor;
        
	 	public int PropertyCount
	 	{
	 	 	get => _propertyCount;
	 	 	set => _propertyCount = value;
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
		    SerializedProperty keyProperty = property.FindPropertyRelative("StateKey");
		    SerializedProperty conditionList = property.FindPropertyRelative("ConditionStates");

		    var drawRect = new Rect(position.x, position.y + 24f * pCount, position.width, position.height);
		    EditorGUI.PropertyField(drawRect, startFrameProperty, 
			    new GUIContent("StartFrame"), true);
		    pCount++;
             
		    drawRect = new Rect(position.x, position.y + 24f * pCount, position.width, position.height);
		    EditorGUI.PropertyField(drawRect, durationProperty, 
			    new GUIContent("Duration"), true);
		    pCount++;
	 	 	    
		    drawRect = new Rect(position.x, position.y + 24f * pCount, position.width, position.height);
		    EditorGUI.PropertyField(drawRect, keyProperty, 
			    new GUIContent("StateKey"), true);
		    pCount++;
             
		    drawRect = new Rect(position.x, position.y + 24f * pCount, position.width, position.height);
		    EditorGUI.PropertyField(drawRect, conditionList, 
			    new GUIContent("Conditions"), true);

		    pCount += (conditionList.arraySize + 1) * 4;
		    
	 	 	PropertyCount = pCount;
	 	 	
	 	 	EditorGUI.EndProperty();
	 	}
	}
}
