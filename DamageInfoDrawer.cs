using SimpleActionFramework.Core;
using UnityEditor;
using UnityEngine;

namespace Editor.SimpleActionEditor
{
	[CustomPropertyDrawer(typeof(DamageInfo))]
	public class DamageInfoDrawer : PropertyDrawer
	{
		private int _propertyCount;
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
			var pCount = 0;
			SerializedProperty damageProperty = property.FindPropertyRelative("Damage");
			SerializedProperty guardDamageProperty = property.FindPropertyRelative("GuardDamage");
			SerializedProperty directionProperty = property.FindPropertyRelative("Direction");
			SerializedProperty knockBackProperty = property.FindPropertyRelative("KnockbackPower");
			SerializedProperty guardCrashProperty = property.FindPropertyRelative("GuardCrash");
			SerializedProperty onHitSelfProperty = property.FindPropertyRelative("NextStateOnSuccessToSelf");
			SerializedProperty onHitOtherProperty = property.FindPropertyRelative("NextStateOnSuccessToReceiver");
            
			EditorGUI.BeginProperty(position, label, property);
			
			var drawRect = new Rect(position.x, position.y + 24f * pCount, position.width, 24f);
			EditorGUI.PropertyField(drawRect, damageProperty, true);
			pCount++;
			
			drawRect = new Rect(position.x, position.y + 24f * pCount, position.width, 24f);
			EditorGUI.PropertyField(drawRect, guardDamageProperty, true);
			pCount++;
			
			drawRect = new Rect(position.x, position.y + 24f * pCount, position.width, 24f);
			EditorGUI.PropertyField(drawRect, directionProperty, true);
			pCount++;
			pCount++;
			
			drawRect = new Rect(position.x, position.y + 24f * pCount, position.width, 24f);
			EditorGUI.PropertyField(drawRect, knockBackProperty, true);
			pCount++;
			
			drawRect = new Rect(position.x, position.y + 24f * pCount, position.width, 24f);
			EditorGUI.PropertyField(drawRect, guardCrashProperty, true);
			pCount++;
			
			drawRect = new Rect(position.x, position.y + 24f * pCount, position.width, 24f);
			EditorGUI.PropertyField(drawRect, onHitSelfProperty, new GUIContent("DataSelf"), true);
			pCount++;
			
			drawRect = new Rect(position.x, position.y + 24f * pCount, position.width, 24f);
			EditorGUI.PropertyField(drawRect, onHitOtherProperty, new GUIContent("DataOther"), true);
			pCount++;
			
			PropertyCount = pCount;
	 	 	
			EditorGUI.EndProperty();
		}
	}
}