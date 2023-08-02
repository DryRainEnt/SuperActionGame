using System.Collections.Generic;
using System.Linq;
using SimpleActionFramework.Core;
using UnityEditor;
using UnityEngine;

namespace Editor.SimpleActionEditor
{
    [CustomEditor(typeof(ActionStateMachine))]
    public class ActionStateMachineEditor : UnityEditor.Editor
    {
        SerializedProperty dictProp;
        SerializedProperty defaultProp;
        public bool foldout = true;

        void OnEnable()
        {
            dictProp = serializedObject.FindProperty("States");
            defaultProp = serializedObject.FindProperty("DefaultStateName");
        }

        public override void OnInspectorGUI()
        {
            var defaultColor = GUI.backgroundColor;
            
            serializedObject.Update();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Default State : ", GUILayout.Width(96f));
            EditorGUILayout.PropertyField(defaultProp, GUIContent.none);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Separator();

            List<int> killList = new List<int>();

            SerializedProperty keysProp = dictProp.FindPropertyRelative("keys");
            SerializedProperty valuesProp = dictProp.FindPropertyRelative("values");

            EditorGUILayout.BeginHorizontal();
            foldout = EditorGUILayout.BeginFoldoutHeaderGroup(foldout, "States");
            EditorGUILayout.IntField(keysProp.arraySize, GUILayout.MaxWidth(60f));
            
            EditorGUILayout.EndHorizontal();
            
            if (foldout)
            {
                if (GUILayout.Button("+"))
                {
                    EditorGUI.BeginChangeCheck();
                    keysProp.InsertArrayElementAtIndex(keysProp.arraySize);
                    valuesProp.InsertArrayElementAtIndex(valuesProp.arraySize);

                    // 새로운 키 값이 고유하도록 만듭니다.
                    var newKeyProp = keysProp.GetArrayElementAtIndex(keysProp.arraySize - 1);
                    newKeyProp.stringValue = "NewState" + keysProp.arraySize.ToString();

                    serializedObject.ApplyModifiedProperties();
                    EditorGUI.EndChangeCheck();
                }

                for (int i = 0; i < keysProp.arraySize; i++)
                {
                    EditorGUILayout.BeginHorizontal(GUILayout.Height(24f));
                    EditorGUILayout.LabelField(i.ToString(), GUILayout.Width(16f));
                    EditorGUILayout.PropertyField(keysProp.GetArrayElementAtIndex(i), GUIContent.none);
                    EditorGUILayout.PropertyField(valuesProp.GetArrayElementAtIndex(i), GUIContent.none);
                    EditorGUILayout.Space(2f);
                    bool frameCountIsOne = false;
                    if (valuesProp.GetArrayElementAtIndex(i).objectReferenceValue is ActionState state)
                    {
                        frameCountIsOne = state.TotalDuration == 1;
                        EditorGUILayout.LabelField($"{state.TotalDuration}", GUILayout.Width(16f));
                    }
                    else
                    {
                        EditorGUILayout.LabelField("0", GUILayout.Width(16f));
                    }
                    EditorGUILayout.LabelField(frameCountIsOne ? "frame" : "frames", GUILayout.Width(42f));

                    GUI.backgroundColor = Color.red;
                    GUIStyle simpleStyle = new GUIStyle(GUI.skin.button)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        fixedWidth = 20f,
                    };
                    
                    if (GUILayout.Button("-", simpleStyle))
                    {
                        killList.Add(i);
                    }
                    
                    GUI.backgroundColor = defaultColor;
                    
                    EditorGUILayout.EndHorizontal();
                }
                
                EditorGUILayout.EndFoldoutHeaderGroup();
            }

            int killCount = 0;
            while (killList.Count > 0)
            {
                var index = killList[0] - killCount;
                killList.RemoveAt(0);
                keysProp.DeleteArrayElementAtIndex(index);
                valuesProp.DeleteArrayElementAtIndex(index);
                killCount++;
            }
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}
