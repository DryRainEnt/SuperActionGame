using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SimpleActionFramework.Core;
using UnityEditor;
using UnityEngine;

namespace Editor.SimpleActionEditor
{
    [CustomEditor(typeof(ActionState))]
    public class ActionStateEditor : UnityEditor.Editor
    {
        SerializedProperty listProp;
        public bool foldout = true;

        void OnEnable()
        {
            listProp = serializedObject.FindProperty("Actants");
        }

        public override void OnInspectorGUI()
        {
            var defaultColor = GUI.backgroundColor;
            
            serializedObject.Update();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Total Duration : ", GUILayout.Width(96f));
            EditorGUILayout.IntField(((ActionState)serializedObject.targetObject).TotalDuration);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Separator();

            List<int> killList = new List<int>();

            EditorGUILayout.BeginHorizontal();
            foldout = EditorGUILayout.BeginFoldoutHeaderGroup(foldout, "Actants");
            EditorGUILayout.IntField(listProp.arraySize, GUILayout.MaxWidth(60f));
            
            EditorGUILayout.EndHorizontal();
            
            if (foldout)
            {
                if (GUILayout.Button("+"))
                {
                    GenericMenu menu = new GenericMenu();

                    // SingleActant의 모든 자식 타입을 찾아서 메뉴에 추가합니다.
                    foreach (var type in GetDerivedTypes<SingleActant>())
                    {
                        menu.AddItem(new GUIContent(type.Name), false, () =>
                        {
                            // 선택한 타입의 Actant를 추가합니다.
                            //TODO: 이렇게 하면 Actant의 생성자가 호출되지 않습니다.
                            listProp.InsertArrayElementAtIndex(listProp.arraySize);
                            listProp.GetArrayElementAtIndex(listProp.arraySize - 1).managedReferenceValue 
                                = Activator.CreateInstance(type);
                        });
                    }

                    menu.ShowAsContext();
                }

                for (int i = 0; i < listProp.arraySize; i++)
                {
                    SerializedProperty actantProp = listProp.GetArrayElementAtIndex(i);
                    
                    EditorGUILayout.BeginHorizontal(GUILayout.Height(24f));
                    EditorGUILayout.LabelField(i.ToString(), GUILayout.Width(16f));
                    EditorGUILayout.PropertyField(actantProp, GUIContent.none);
                    EditorGUILayout.Space(2f);
                    
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
                listProp.DeleteArrayElementAtIndex(index);
                killCount++;
            }
            
            serializedObject.ApplyModifiedProperties();
        }
        
        // T를 상속받는 모든 타입을 찾는 메서드입니다.
        private static IEnumerable<Type> GetDerivedTypes<T>()
        {
            return Assembly.GetAssembly(typeof(T)).GetTypes().Where(t => t.IsSubclassOf(typeof(T)) && !t.IsAbstract);
        }
    }
}
