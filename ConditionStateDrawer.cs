using UnityEditor;
using SimpleActionFramework.Core;
using UnityEngine;

namespace Editor.SimpleActionEditor
{
	[CustomPropertyDrawer(typeof(ConditionState))]
	public class ConditionStateDrawer : PropertyDrawer
	{
		private int _propertyCount;
		public int PropertyCount
		{
			get => _propertyCount;
			set => _propertyCount = value;
		}
		
		private ValueType _valueType = ValueType.Number;
	 	
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return 24f * PropertyCount;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var pCount = 0;
			SerializedProperty jointProperty = property.FindPropertyRelative("JointType");
			SerializedProperty defaultKeyProperty = property.FindPropertyRelative("DefaultKey");
			SerializedProperty keyProperty = property.FindPropertyRelative("Key");
			SerializedProperty valueTypeProperty = property.FindPropertyRelative("ValueType");
			SerializedProperty conditionTypeProperty = property.FindPropertyRelative("ConditionType");
			SerializedProperty stringValueProperty = property.FindPropertyRelative("StringValue");
			SerializedProperty floatValueProperty = property.FindPropertyRelative("NumberValue");
			SerializedProperty consumeInputProperty = property.FindPropertyRelative("ConsumeInput");
			var valueType = (ValueType)valueTypeProperty.enumValueIndex;
			
			EditorGUI.BeginProperty(position, label, property);
			var drawRect = new Rect(position.x, position.y + 24f * pCount, position.width, 24f);
			EditorGUI.PropertyField(drawRect, jointProperty, new GUIContent("Joint"), true);
			pCount++;
			
			drawRect = new Rect(position.x, position.y + 24f * pCount, position.width, 24f);
			EditorGUI.PropertyField(drawRect, defaultKeyProperty, new GUIContent("Preset"), true);
			pCount++;

			if (defaultKeyProperty.intValue == 999)
			{
				drawRect = new Rect(position.x, position.y + 24f * pCount, 36f, 24f);
				EditorGUI.LabelField(drawRect, "Key");
			
				drawRect = new Rect(position.x + 40f, position.y + 24f * pCount, 96f, 20f);
				keyProperty.stringValue = EditorGUI.TextArea(drawRect, keyProperty.stringValue);
			
				drawRect = new Rect(position.x + 144f, position.y + 24f * pCount, 96f, 24f);
				EditorGUI.PropertyField(drawRect, valueTypeProperty, new GUIContent(""), true);
				pCount++;
			}
			else
			{
				var defaultKey = Constants.DefaultDataKeys.TryGetValue((DefaultKeys)defaultKeyProperty.intValue, 
					out var defaultKeyString) ? defaultKeyString : keyProperty.stringValue;
				switch ((DefaultKeys)defaultKeyProperty.intValue)
				{
					case DefaultKeys.INPUT:
						_valueType = ValueType.Input;
						break;
					case DefaultKeys.MOVE:
						_valueType = ValueType.Number;
						break;
					case DefaultKeys.FACE:
						_valueType = ValueType.Number;
						break;
					case DefaultKeys.VSPEED:
						_valueType = ValueType.Number;
						break;
					case DefaultKeys.GROUND:
						_valueType = ValueType.Number;
						break;
					case DefaultKeys.INTERACTION:
						_valueType = ValueType.String;
						break;
				}
				
				drawRect = new Rect(position.x, position.y + 24f * pCount, 36f, 24f);
				EditorGUI.LabelField(drawRect, "Key");

				drawRect = new Rect(position.x + 40f, position.y + 24f * pCount, 96f, 20f);
				keyProperty.stringValue = EditorGUI.TextArea(drawRect, defaultKey);
			
				drawRect = new Rect(position.x + 144f, position.y + 24f * pCount, 96f, 24f);
				_valueType = (ValueType)EditorGUI.EnumPopup(drawRect, _valueType);
				valueTypeProperty.enumValueIndex = (int)_valueType;
				pCount++;
			}
	 	 	    
			drawRect = new Rect(position.x, position.y + 24f * pCount, 136f, 24f);
			EditorGUI.PropertyField(drawRect, conditionTypeProperty, new GUIContent(""), true);
			
	 	 	    
			drawRect = new Rect(position.x + 144f, position.y + 24f * pCount, 96f, 24f);
			if (valueType == ValueType.Number)
				EditorGUI.PropertyField(drawRect, floatValueProperty, new GUIContent(""), true);
			else if (valueType == ValueType.Input)
			{
				EditorGUI.PropertyField(drawRect, stringValueProperty, new GUIContent(""), true);
				pCount++;
				
				drawRect = new Rect(position.x, position.y + 24f * pCount, 140f, 24f);
				EditorGUI.LabelField(drawRect, "Consume Input Record");
				drawRect = new Rect(position.x + 144f, position.y + 24f * pCount, 36f, 24f);
				consumeInputProperty.boolValue = EditorGUI.Toggle(drawRect, consumeInputProperty.boolValue);
			}
			else
				EditorGUI.PropertyField(drawRect, stringValueProperty, new GUIContent(""), true);
			pCount++;
			
			PropertyCount = pCount;
	 	 	
			EditorGUI.EndProperty();
		}
	}
}