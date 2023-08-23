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
	 	 	set => _propertyCount = Mathf.Max(value, _propertyCount);
	 	}
	 	
	 	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	 	{
	 	    return 24f * PropertyCount;
	 	}

	 	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	 	{
	 	 	EditorGUI.BeginProperty(position, label, property);
             
			DefaultGUIColor = GUI.backgroundColor;
	 	 	var pCount = 0;
	 	 	SerializedProperty startFrameProperty = property.FindPropertyRelative("StartFrame");
	 	 	SerializedProperty keyProperty = property.FindPropertyRelative("StateKey");
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
		    EditorGUI.PropertyField(drawRect, keyProperty, 
			    new GUIContent("StateKey"), true);
		    pCount++;
             
		    var keyList = property.FindPropertyRelative("ConditionKeys");
		    var valueList = property.FindPropertyRelative("ConditionValues");
		    
		    drawRect = new Rect(position.x, position.y + 24f * pCount, position.width, position.height);
		    EditorGUI.LabelField(drawRect, "Conditions");
		    drawRect = new Rect(position.x + position.width - 96f, position.y + 24f * pCount, 48, position.height);
		    if (GUI.Button(drawRect, "+"))
		    {
			    keyList.InsertArrayElementAtIndex(keyList.arraySize);
			    valueList.InsertArrayElementAtIndex(valueList.arraySize);
		    }
		    GUI.backgroundColor = Color.red;
		    drawRect = new Rect(position.x + position.width - 36f, position.y + 24f * pCount, 24, position.height);
		    if (GUI.Button(drawRect, "-"))
		    {
			    keyList.DeleteArrayElementAtIndex(keyList.arraySize - 1);
			    valueList.DeleteArrayElementAtIndex(keyList.arraySize - 1);
		    }
		    GUI.backgroundColor = DefaultGUIColor;
		    pCount++;

		    EditorGUI.indentLevel++;
		    
		    drawRect = new Rect(position.x, position.y + 24f * pCount, position.width / 2f - 2f, position.height);
		    EditorGUI.LabelField(drawRect, "Key");
		    drawRect = new Rect(position.x + position.width / 2f + 2f, position.y + 24f * pCount, position.width / 2f - 4f, position.height);
		    EditorGUI.LabelField(drawRect, "Value");
		    pCount++;
		    
		    for (int i = 0; i < keyList.arraySize; i++)
		    {
			    drawRect = new Rect(position.x - 24f, position.y + 24f * pCount, 24f, position.height);
			    EditorGUI.LabelField(drawRect, i.ToString());
			    drawRect = new Rect(position.x, position.y + 24f * pCount, position.width / 2f - 2f, position.height);
			    keyList.GetArrayElementAtIndex(i).stringValue = EditorGUI.TextField(drawRect, keyList.GetArrayElementAtIndex(i).stringValue);
			    drawRect = new Rect(position.x + position.width / 2f + 2f, position.y + 24f * pCount, position.width / 2f - 4f, position.height);
			    valueList.GetArrayElementAtIndex(i).stringValue = EditorGUI.TextField(drawRect, valueList.GetArrayElementAtIndex(i).stringValue);
			    pCount++;
		    }
		    EditorGUI.indentLevel--;
		    
		    pCount++;
		    
	 	 	PropertyCount = pCount;
	 	 	
	 	 	EditorGUI.EndProperty();
	 	}
	}
}
