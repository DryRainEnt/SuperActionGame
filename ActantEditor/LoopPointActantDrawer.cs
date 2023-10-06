using System.Collections.Generic;
using SimpleActionFramework.Actant;
using UnityEditor;
using UnityEngine;

namespace Editor.SimpleActionEditor.ActantEditor
{
	[CustomPropertyDrawer(typeof(LoopPointActant))]
	public class LoopPointActantDrawer : PropertyDrawer
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
	 	 	var pCount = 0;
	 	 	SerializedProperty startFrameProperty = property.FindPropertyRelative("StartFrame");
	 	 	SerializedProperty durationProperty = property.FindPropertyRelative("Duration");
		    SerializedProperty conditionList = property.FindPropertyRelative("ConditionStates");
	 	 	EditorGUI.BeginProperty(position, label, property);
	 	 	
	 	 	var drawRect = new Rect(position.x, position.y + 24f * pCount, position.width, position.height);
	 	 	EditorGUI.PropertyField(drawRect, startFrameProperty, 
	 	 	    new GUIContent("StartFrame"), true);
	 	 	pCount++;
	 	 	
	 	 	drawRect = new Rect(position.x, position.y + 24f * pCount, position.width, position.height);
	 	 	EditorGUI.PropertyField(drawRect, durationProperty, 
	 	 	    new GUIContent("Duration"), true);
	 	 	pCount++;
	 	 	
	 	 	// Put your code here
		    drawRect = new Rect(position.x, position.y + 24f * pCount, position.width, position.height);
		    EditorGUI.PropertyField(drawRect, conditionList, 
			    new GUIContent("Conditions"), true);

		    pCount += (conditionList.arraySize + 1) * 3;
		    pCount++;
	 	 	
	 	 	PropertyCount = pCount;
	 	 	
	 	 	EditorGUI.EndProperty();
	 	}
	}
}
