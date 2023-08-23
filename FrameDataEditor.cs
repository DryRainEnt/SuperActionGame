using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace SimpleActionFramework.Core.Editor
{
	public class FrameDataEditor : EditorWindow
	{
		private ActionStateMachine ASM;
		private string searchKey;
		private string selectedKey;
		private ActionState selectedActionState;
		private SerializedObject serializedActionState;
		private SingleActant selectedActant;
		private int selectedActantIndex;
		private Vector2 listScrollPosition;
		
		private int selectedFrame = 0;

		private Color DefaultGUIColor;
		
		private readonly Color HitBoxColor = new Color(0f, 1f, 0f, 0.5f);
		private readonly Color DamageBoxColor = new Color(1f, 0f, 0f, 0.5f);
		private readonly Color GuardBoxColor = new Color(0f, 0f, 1f, 0.5f);
		
		private Vector2 dataScrollPosition;
		
		private float listAreaWidth = 320f;
		private float timelineAreaHeight = 240f;
		private float dataAreaWidth = 320f;

		[MenuItem("Window/Frame Data Editor")]
		static void Init()
		{
			FrameDataEditor window = (FrameDataEditor)GetWindow(typeof(FrameDataEditor), false, "Frame Data Editor");
			window.Show();
		}

		private void OnGUI()
		{
			DefaultGUIColor = GUI.backgroundColor;
			
			EditorGUILayout.BeginHorizontal();

				// 목록 영역
				EditorGUILayout.BeginVertical(GUILayout.Width(320f));
					DrawListArea();
				EditorGUILayout.EndVertical();

				// 수정 영역
				EditorGUILayout.BeginVertical();
					DrawTimelineArea();
					
					EditorGUILayout.BeginHorizontal();
						DrawEditArea();

						// 데이터 영역
						EditorGUILayout.BeginVertical(GUILayout.Width(320f));
							DrawDataArea();
						EditorGUILayout.EndVertical();
					
					EditorGUILayout.EndHorizontal();

				EditorGUILayout.EndVertical();
			
			EditorGUILayout.EndHorizontal();
            
			GUI.backgroundColor = DefaultGUIColor;
		}
		
		private void DrawListArea()
		{
			Handles.color = new Color(0f, 0f, 0f, 1f); // 영역 구분선 설정
			Handles.DrawLine(
				new Vector3(321f, 0f, 0), 
				new Vector3(321f, position.height, 0)
			);
			Handles.DrawLine(
				new Vector3(320f, 0f, 0), 
				new Vector3(320f, position.height, 0)
			);
			
			// FrameDataSet 선택 드롭다운
			ASM = EditorGUILayout.ObjectField("Action State Machine", ASM, typeof(ActionStateMachine), false) as ActionStateMachine;
			
			// 선택한 FrameDataSet이 있는 경우 키 목록 표시
			if (ASM is not null)
			{
				EditorGUILayout.BeginHorizontal();
				
				searchKey = GUILayout.TextField(searchKey);
				if (GUILayout.Button("Add", GUILayout.Width(80f)) && !ASM.States.ContainsKey(searchKey))
				{
					// TODO: Add new ActionState with menu or something
					ASM.Add(searchKey, new ActionState());
					EditorUtility.SetDirty(ASM);
					AssetDatabase.SaveAssets();
				}
				EditorGUILayout.EndHorizontal();
				
				EditorGUILayout.LabelField("Frame Data Keys:");
				listScrollPosition = EditorGUILayout.BeginScrollView(listScrollPosition);

				var updateExists = false;
				var removeKey = "";
				do
				{
					updateExists = false;
					removeKey = "";
					
					foreach (var key in ASM.States.Keys)
					{
						if (searchKey is not null && searchKey != string.Empty && !key.Contains(searchKey, StringComparison.Ordinal))
							continue;
					
						EditorGUILayout.BeginHorizontal();
					
						if (GUILayout.Button(key))
						{
							selectedKey = key;
							selectedActionState = ASM.States[key];
							serializedActionState = new SerializedObject(selectedActionState);
						}

						GUI.backgroundColor = Color.red;
						if (GUILayout.Button("Remove", GUILayout.Width(80f)))
						{
							GenericMenu menu = new GenericMenu();

							menu.AddItem(new GUIContent("Are you sure with removing this action?"), false, () => { });
							menu.AddItem(new GUIContent("Yes"), false, () =>
							{
								removeKey = key;
								updateExists = true;
							});
							menu.AddItem(new GUIContent("No"), false, () =>
							{
								removeKey = "";
								updateExists = false;
							});

							menu.ShowAsContext();
						}
						GUI.backgroundColor = DefaultGUIColor;
						
						EditorGUILayout.EndHorizontal();
                        
						if (updateExists)
							break;
					}
					
					if (updateExists)
					{
						EditorUtility.SetDirty(ASM);
						ASM.States.Remove(removeKey);
						AssetDatabase.SaveAssets();
					}
				}while (updateExists);

				EditorGUILayout.EndScrollView();
			}
		}
		
		private float zoom = 1f; // 확대/축소 배율
		private Vector2 scrollPosition = Vector2.zero; // 스크롤 위치
		private Vector2 timelineScroll = Vector2.zero; // 스크롤 위치
		private float timelineHeight = 210f; // 타임라인 영역의 높이

		// T를 상속받는 모든 타입을 찾는 메서드입니다.
		private static IEnumerable<Type> GetDerivedTypes<T>()
		{
			return Assembly.GetAssembly(typeof(T)).GetTypes().Where(t => t.IsSubclassOf(typeof(T)) && !t.IsAbstract);
		}
		
		private void DrawTimelineArea()
		{
			var defaultColor = GUI.backgroundColor;
			var killList = new List<int>();
			Handles.color = new Color(0f, 0f, 0f, 1f); // 영역 구분선 설정
			Handles.DrawLine(
				new Vector3(321f, timelineHeight + 14f, 0), 
				new Vector3(position.width, timelineHeight + 14f, 0)
			);
			Handles.DrawLine(
				new Vector3(321f, timelineHeight + 12f, 0), 
				new Vector3(position.width, timelineHeight + 12f, 0)
			);
			
			if (ASM is null || selectedActionState is null)
			{
				EditorGUILayout.LabelField("No action state selected");
				return;
			}
			
			EditorGUILayout.BeginVertical();
	            
				if (GUILayout.Button("Add Actant", GUILayout.Width(96f)))
				{
					GenericMenu menu = new GenericMenu();

					// SingleActant의 모든 자식 타입을 찾아서 메뉴에 추가합니다.
					foreach (var type in GetDerivedTypes<SingleActant>())
					{
						menu.AddItem(new GUIContent(type.Name), false, () =>
						{
							// 선택한 타입의 Actant를 추가합니다.
							selectedActionState.Actants.Add((SingleActant)Activator.CreateInstance(type));
							EditorUtility.SetDirty(ASM);
							AssetDatabase.SaveAssets();
						});
					}

					menu.ShowAsContext();
				}

				EditorGUILayout.BeginHorizontal();

				GUIStyle horizontalScrollbar = GUI.skin.horizontalScrollbar;
				GUIStyle verticalScrollbar = GUI.skin.verticalScrollbar;
				GUI.skin.horizontalScrollbar = GUIStyle.none;
				GUI.skin.verticalScrollbar = GUIStyle.none;

				var swapTarget = (0, 0);
				EditorGUILayout.BeginVertical(GUILayout.Width(44f));
					GUILayout.Button(">>", GUILayout.Width(42f));
					
					EditorGUILayout.BeginScrollView(timelineScroll, GUIStyle.none, GUIStyle.none, GUILayout.Width(42f), GUILayout.Height(192f));
					for (var index = 0; index < selectedActionState.Actants.Count; index++)
					{
						EditorGUILayout.BeginHorizontal();

						GUI.backgroundColor = Color.red;
						GUIStyle simpleStyle = new GUIStyle(GUI.skin.button)
						{
							alignment = TextAnchor.MiddleCenter,
						};
						if (GUILayout.Button("X", simpleStyle, GUILayout.Width(20f), GUILayout.Height(30f)) && index > 0)
						{
							killList.Add(index);
						}
						GUI.backgroundColor = defaultColor;
						
						EditorGUILayout.BeginVertical();
						if (GUILayout.Button("▲", GUILayout.Width(20f), GUILayout.Height(14f)) && index > 0)
						{
							swapTarget = (index, index - 1);
						}

						if (GUILayout.Button("▼", GUILayout.Width(20f), GUILayout.Height(14f)) &&
						    index < selectedActionState.Actants.Count - 1)
						{
							swapTarget = (index, index + 1);
						}
						EditorGUILayout.EndVertical();
						EditorGUILayout.EndHorizontal();
					}
					EditorGUILayout.EndScrollView();
					
				EditorGUILayout.EndVertical();

				EditorGUILayout.BeginVertical();
						
				var totalDuration = selectedActionState.TotalDuration;
	            EditorGUILayout.BeginScrollView(timelineScroll, GUIStyle.none, GUIStyle.none, GUILayout.Width(position.width - 384f), GUILayout.Height(20f));
					EditorGUILayout.BeginHorizontal();
						for (int i = 0; i <= totalDuration; i++)
						{
							if (i == selectedFrame)
								GUI.backgroundColor = Color.yellow;
							if (GUILayout.Button(i.ToString(), GUILayout.Width(30f)))
							{
								selectedFrame = i;
								selectedActant = null;
								selectedActantIndex = -1;
							}
							GUI.backgroundColor = DefaultGUIColor;
						}
					EditorGUILayout.EndHorizontal();
	            EditorGUILayout.EndScrollView();
            
	            GUI.skin.horizontalScrollbar = horizontalScrollbar;
	            GUI.skin.verticalScrollbar = verticalScrollbar;
	            
				// 스크롤뷰 시작
				timelineScroll = EditorGUILayout.BeginScrollView(timelineScroll, true, true , GUILayout.Width(position.width - 370f), GUILayout.Height(timelineHeight - 30f));

            
			            for (var index = 0; index < selectedActionState.Actants.Count; index++)
			            {
				            var act = selectedActionState.Actants[index];
				            EditorGUILayout.BeginHorizontal();
				            
				            for (int i = 0; i <= totalDuration; i++)
				            {
					            if (i == act.StartFrame)
					            {
						            GUI.backgroundColor =
							            (act == selectedActant) ? Color.cyan : Color.green;
						            if (GUILayout.Button(act.ToString(), GUILayout.Width(33f * Mathf.Max(1, act.Duration) - 3f), GUILayout.Height(30f)))
						            {
							            if (selectedFrame < act.StartFrame || selectedFrame > act.DrawnFrame)
											selectedFrame = i;
							            selectedActant = act;
							            selectedActantIndex = index;
						            }

						            GUI.backgroundColor = DefaultGUIColor;
					            }
					            else if (i < act.StartFrame || i > act.DrawnFrame)
					            {
						            if (GUILayout.Button("X", GUILayout.Width(30f), GUILayout.Height(30f)))
						            {
							            selectedFrame = i;
							            selectedActant = null;
							            selectedActantIndex = -1;
						            }
					            }
				            }

				            EditorGUILayout.EndHorizontal();
			            }

					if (swapTarget.Item1 != swapTarget.Item2)
				            (selectedActionState.Actants[swapTarget.Item1], selectedActionState.Actants[swapTarget.Item2])
				            = (selectedActionState.Actants[swapTarget.Item2], selectedActionState.Actants[swapTarget.Item1]);
			            
					EditorGUILayout.EndVertical();
				
				EditorGUILayout.EndScrollView();
				
				EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();

			int killCount = 0;
			while (killList.Count > 0)
			{
				var index = killList[0] - killCount;
				killList.RemoveAt(0);
				selectedActionState.Actants.RemoveAt(index);
				killCount++;
			}
		}
		
		private void DrawEditArea()
		{
			// 스크롤 이벤트 감지 (줌)
			if (Event.current.type == EventType.ScrollWheel)
			{
				float zoomDelta = Event.current.delta.y * 0.01f;
				zoom -= zoomDelta * (zoomDelta < 0 ? 5f : 1f);
				zoom = Mathf.Clamp(zoom, 0.5f, 3f); // 줌 배율 제한
				zoom = Mathf.FloorToInt(zoom * 10f) / 10f; // 배율을 0.1 단위로 제한
			}

			// 마우스 드래그 이벤트 감지 (이동)
			if (Event.current.type == EventType.MouseDrag && Event.current.button == 2) // 마우스 휠 버튼
			{
				scrollPosition -= Event.current.delta;
			}

			
			if (selectedActionState is null || selectedActionState.Actants == null || selectedActionState.Actants.Count == 0)
			{
				EditorGUILayout.LabelField("No actants selected.");
				return;
			}
			
			// 스크롤뷰 시작
			scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

			// 격자 그리기
			DrawActor();

			// 스크롤뷰 끝
			EditorGUILayout.EndScrollView();
		}

		private Vector2 gSize => new(position.width - listAreaWidth - dataAreaWidth, position.height - timelineHeight);
		private Vector2 gPos => gSize / 2f - scrollPosition;
		private Rect gRect => new(gPos, gSize);
		
		private void DrawActor()
		{
			DrawGrid();
			
			selectedActionState.CurrentFrame = selectedFrame;
            selectedActionState.OnGUIFrame(gRect, 2f * zoom);
		}

		private void OnDisable()
		{
		}

		private void DrawGrid()
		{
			//TODO: 대상 Offset 중심으로 그려야 함
			float gridSize = 32 * zoom; // 격자의 크기 설정
			int gridCountX = Mathf.CeilToInt(gSize.x / gridSize);
			int gridCountY = Mathf.CeilToInt(gSize.y / gridSize);

			EditorGUILayout.LabelField($"Zoom Level : {zoom * 100 : #00.00}%");
			
			Handles.color = new Color(1f, 1f, 1f, 0.7f); // 격자 색상 설정
			Handles.DrawLine(
				new Vector3(gPos.x, 0f, 0), 
				new Vector3(gPos.x, gSize.y, 0));
			Handles.DrawLine(
				new Vector3(0, gPos.y, 0), 
				new Vector3(gSize.x, gPos.y, 0));
			
			Handles.color = new Color(0.4f, 0.4f, 0.4f, 0.4f); // 격자 색상 설정
			for (int x = 1; x < gridCountX / 2f; x++)
			{
				for (int y = 1; y < gridCountY / 2f; y++)
				{
					Handles.DrawLine(
						new Vector3(gPos.x + x * gridSize, 0f, 0), 
						new Vector3(gPos.x + x * gridSize, gSize.y, 0));
					Handles.DrawLine(
						new Vector3(gPos.x - x * gridSize, 0f, 0), 
						new Vector3(gPos.x - x * gridSize, gSize.y, 0));
					Handles.DrawLine(
						new Vector3(0, gPos.y + y * gridSize, 0), 
						new Vector3(gSize.x, gPos.y + y * gridSize, 0));
					Handles.DrawLine(
						new Vector3(0, gPos.y - y * gridSize, 0), 
						new Vector3(gSize.x, gPos.y - y * gridSize, 0));
				}
			}
		}

		private ReorderableList reorderableList;
		SerializedProperty listProp;
		
		private void DrawDataArea()
		{
			// TODO: 여기서 selectedActant의 데이터를 표시하고 수정하게 해야 함.
			// 문제는 selectedActant가 여러 가지 타입으로 상속받을 수 있어서, 그 타입마다 다른 데이터 필드를 갖고 있을 수 있음.
			// 여기서 어떻게 serializedProperty로 관리할 수 있을까?
			Handles.color = new Color(0f, 0f, 0f, 1f); // 영역 구분선 설정
			Handles.DrawLine(
				new Vector3(position.width - 319f, timelineHeight + 14f, 0), 
				new Vector3(position.width - 319f, position.height, 0)
			);
			Handles.DrawLine(
				new Vector3(position.width - 320f, timelineHeight + 14f, 0), 
				new Vector3(position.width - 320f, position.height, 0)
			);
			
			if (ASM is null || serializedActionState is null || selectedActant is null)
			{
				EditorGUILayout.LabelField("No Actant data selected.");
				return;
			}
			
			EditorGUILayout.LabelField($"Actant Data: {selectedActant.GetType().Name}");
			
			dataScrollPosition = EditorGUILayout.BeginScrollView(dataScrollPosition);
            
			selectedActionState.actantWrapper = new List<SingleActant>{selectedActant};
			listProp = serializedActionState.FindProperty("actantWrapper");
			reorderableList = new ReorderableList(serializedActionState, listProp, true, false, false, false);
			
			reorderableList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
			{
				var element = reorderableList.serializedProperty.GetArrayElementAtIndex(index);
				rect.y += 2;
				EditorGUI.PropertyField(new Rect(rect.x + 16f, rect.y, rect.width - 16f, 
						EditorGUIUtility.singleLineHeight * 1.2f), 
					element, 
					GUIContent.none, 
					true);
				element.isExpanded = true;
			};
            
			reorderableList.elementHeightCallback = (index) =>
			{
				var element = reorderableList.serializedProperty.GetArrayElementAtIndex(index);
				return EditorGUI.GetPropertyHeight(element, true);
			};
			
            reorderableList.onChangedCallback = (list) =>
			{
	            selectedActionState.Actants[selectedActantIndex] = selectedActionState.actantWrapper[0];
	            EditorUtility.SetDirty(selectedActionState);
	            AssetDatabase.SaveAssets();
			};
			
			serializedActionState.Update();
			reorderableList.DoLayoutList();

			serializedActionState.ApplyModifiedProperties();

			EditorGUILayout.EndScrollView();
		}
	}
}
