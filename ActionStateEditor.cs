using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SimpleActionFramework.Core;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Editor.SimpleActionEditor
{
    [CustomEditor(typeof(ActionState))]
    public class ActionStateEditor : UnityEditor.Editor
    {
        private ReorderableList reorderableList;
        
        SerializedProperty listProp;
        ActionState state;
        [SerializeReference] List<SingleActant> actList;
        public bool foldout = true;

        void OnEnable()
        {
            state = serializedObject.targetObject as ActionState;
            listProp = serializedObject.FindProperty("Actants");
            actList = state.Actants;
            
            reorderableList = new ReorderableList(serializedObject, listProp, true, false, true, true);

            reorderableList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var element = reorderableList.serializedProperty.GetArrayElementAtIndex(index);
                rect.y += 2;
                EditorGUI.PropertyField(new Rect(rect.x + 16f, rect.y, rect.width - 16f, 
                        EditorGUIUtility.singleLineHeight * 1.2f), 
                    element, 
                    GUIContent.none, 
                    true);
            };
            
            reorderableList.elementHeightCallback = (index) =>
            {
                var element = reorderableList.serializedProperty.GetArrayElementAtIndex(index);
                return EditorGUI.GetPropertyHeight(element, true);
            };
            
            reorderableList.onAddDropdownCallback = (Rect buttonRect, ReorderableList l) =>
            {
                GenericMenu menu = new GenericMenu();

                // SingleActant의 모든 자식 타입을 찾아서 메뉴에 추가합니다.
                foreach (var type in GetDerivedTypes<SingleActant>())
                {
                    menu.AddItem(new GUIContent(type.Name), false, () =>
                    {
                        // 선택한 타입의 Actant를 추가합니다.
                        actList.Add((SingleActant)Activator.CreateInstance(type));

                        serializedObject.Update();
                            
                        serializedObject.ApplyModifiedProperties();
                    });
                }

                menu.ShowAsContext();
            };

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

            reorderableList.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }

        // T를 상속받는 모든 타입을 찾는 메서드입니다.
        private static IEnumerable<Type> GetDerivedTypes<T>()
        {
            return Assembly.GetAssembly(typeof(T)).GetTypes().Where(t => t.IsSubclassOf(typeof(T)) && !t.IsAbstract);
        }
    }
}
