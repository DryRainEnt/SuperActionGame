using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Proto.BasicExtensionUtils;
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
		private float dataAreaWidth = 320f;
		
        private float StartTime = 0f;
        private float InnerTimer = 0f;
		private bool IsPlaying = false;


		[MenuItem("Window/Frame Data Editor")]
		static void Init()
		{
			FrameDataEditor window = (FrameDataEditor)GetWindow(typeof(FrameDataEditor), false, "Frame Data Editor");
			window.Show();
		}

		private void OnEnable()
		{
			selectedKey = "";
			selectedActionState = null;
			serializedActionState = null;
			selectedActant = null;
			selectedFrame = 0;
			selectedActantIndex = -1;
		}

		private void Update()
		{
			if (selectedActionState && IsPlaying)
			{
				InnerTimer = (float)EditorApplication.timeSinceStartup - StartTime;
				selectedFrame = (selectedActionState.TotalDuration > 0) ? Mathf.FloorToInt(InnerTimer * Constants.DefaultActionFrameRate) % selectedActionState.TotalDuration : 0;
				Repaint();
			}
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

		private bool IsAddMode = false;
		private string newName = "";
		private string newKey = "";
		
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
				EditorGUILayout.Separator();
                
				GUI.backgroundColor = Color.yellow;
				if (GUILayout.Button("Passive"))
				{
					selectedKey = "Passive";
					selectedActionState = ASM.PassiveState;
					serializedActionState = new SerializedObject(selectedActionState);
					IsAddMode = false;
					selectedActant = null;
					selectedFrame = 0;
					selectedActantIndex = -1;
				}
				GUI.backgroundColor = DefaultGUIColor;
				
				EditorGUILayout.BeginHorizontal();
				GUILayout.Label("Default State Key: ", GUILayout.Width(128f));
				ASM.DefaultStateName = GUILayout.TextField(ASM.DefaultStateName);
				EditorGUILayout.EndHorizontal();

				Handles.DrawLine(
					new Vector3(0f, IsAddMode ? 138f: 76f, 0), 
					new Vector3(320f, IsAddMode ? 138f: 76f, 0)
				);
				Handles.DrawLine(
					new Vector3(0f, IsAddMode ? 140f: 78f, 0), 
					new Vector3(320f, IsAddMode ? 140f: 78f, 0)
				);
				
				EditorGUILayout.Separator();

				if (IsAddMode)
				{
					EditorGUILayout.BeginHorizontal();
					GUILayout.Label("New Key: ", GUILayout.Width(64f));
					newKey = GUILayout.TextField(newKey);
					EditorGUILayout.EndHorizontal();
					
					EditorGUILayout.BeginHorizontal();
					newName = GUILayout.TextField(newName);
					
					GUI.backgroundColor = Color.green;
					if (GUILayout.Button("Add", GUILayout.Width(80f)) && newKey != string.Empty && !ASM.States.ContainsKey(newKey))
					{
						if (newName == string.Empty)
							newName = $"{ASM.name}{newKey}State";
						ActionState newState = CreateInstance<ActionState>();
						string path = $"Assets/SimpleActionFramework/ActionState/{ASM.name}";
						if (!AssetDatabase.IsValidFolder(path))
							AssetDatabase.CreateFolder($"Assets/SimpleActionFramework/ActionState", ASM.name);
						path += $"/{newName}.asset";
						AssetDatabase.CreateAsset(newState, path);
						ASM.Add(newKey, newState);
						EditorUtility.SetDirty(ASM);
						AssetDatabase.SaveAssets();
						AssetDatabase.Refresh();
						EditorUtility.FocusProjectWindow();
						Selection.activeObject = newState;
						
						newKey = "";
						newName = "";
						IsAddMode = false;
					}
					GUI.backgroundColor = Color.red;
					if (GUILayout.Button("Cancel", GUILayout.Width(80f)))
					{
						newKey = "";
						newName = "";
						IsAddMode = false;
					}
					GUI.backgroundColor = DefaultGUIColor;
					EditorGUILayout.EndHorizontal();
					
					EditorGUILayout.HelpBox("Enter new key name above, new state name below. \nThen press Add button to create new action state.", MessageType.Info);
				}
				else
				{
					if (GUILayout.Button("New State"))
					{
						IsAddMode = true;
					}
				}
				
				EditorGUILayout.Separator();
				
				EditorGUILayout.BeginHorizontal();
				GUILayout.Label("Frame Data Keys: ", GUILayout.Width(128f));
				searchKey = GUILayout.TextField(searchKey);
				EditorGUILayout.EndHorizontal();
				
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
						if (key == "Passive")
							continue;
					
						EditorGUILayout.BeginHorizontal();
					
						GUI.backgroundColor = (key == selectedKey) ? Color.cyan : DefaultGUIColor;
						if (GUILayout.Button(key))
						{
							selectedKey = key;
							selectedActionState = ASM.States[key];
							serializedActionState = new SerializedObject(selectedActionState);
							IsAddMode = false;
							selectedActant = null;
							selectedFrame = 0;
							selectedActantIndex = -1;
						}

						GUI.backgroundColor = Color.red;
						if (GUILayout.Button("Remove", GUILayout.Width(80f)))
						{
							IsAddMode = false;
							GenericMenu menu = new GenericMenu();

							menu.AddItem(new GUIContent("Are you sure with removing this action?"), false, () => { });
							menu.AddItem(new GUIContent("Yes"), false, () =>
							{
								removeKey = key;
								updateExists = true;
								
								EditorUtility.SetDirty(ASM);
								var killState = ASM.States[removeKey];
								ASM.States.Remove(removeKey);
								AssetDatabase.DeleteAsset($"Assets/SimpleActionFramework/ActionState/{ASM.name}/{killState.name}.asset");
						
								AssetDatabase.SaveAssets();
								AssetDatabase.Refresh();
								
								selectedKey = "";
								selectedActionState = null;
								serializedActionState = null;
								selectedActant = null;
								selectedFrame = 0;
								selectedActantIndex = -1;
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
				}while (updateExists);

				EditorGUILayout.EndScrollView();
			}
			else
			{
				selectedKey = "";
				selectedActionState = null;
				serializedActionState = null;
				selectedActant = null;
				selectedFrame = 0;
				selectedActantIndex = -1;

				if (IsAddMode)
				{
					EditorGUILayout.BeginHorizontal(GUILayout.Width(300f));
					GUILayout.Label("Name: ", GUILayout.Width(64f));
					newName = GUILayout.TextField(newName);
					EditorGUILayout.EndHorizontal();
					
					EditorGUILayout.BeginHorizontal(GUILayout.Width(300f));
					GUI.backgroundColor = Color.green;
					if (GUILayout.Button("Create"))
					{
						ActionStateMachine newStateMachine = CreateInstance<ActionStateMachine>();
						string path = $"Assets/SimpleActionFramework/ActionState/{newName}";
						if (!AssetDatabase.IsValidFolder(path))
							AssetDatabase.CreateFolder($"Assets/SimpleActionFramework/ActionState", newName);
						path += $"/{newName}.asset";
						AssetDatabase.CreateAsset(newStateMachine, path);
						AssetDatabase.SaveAssets();
						AssetDatabase.Refresh();
						EditorUtility.FocusProjectWindow();
						Selection.activeObject = newStateMachine;
						
						newName = "";
						IsAddMode = false;

						ASM = newStateMachine;
					}
					GUI.backgroundColor = Color.red;
					if (GUILayout.Button("Cancel", GUILayout.Width(80f)))
					{
						newName = "";
						IsAddMode = false;
					}
					GUI.backgroundColor = DefaultGUIColor;
					EditorGUILayout.EndHorizontal();
				}
                else
				{
					if (GUILayout.Button("New State Machine", GUILayout.Width(300f)))
					{
						IsAddMode = true;
					}
				}
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
				EditorGUILayout.LabelField("", GUILayout.Height(timelineHeight - 12f));
				return;
			}
			
			EditorGUILayout.BeginVertical();
	            
				EditorGUILayout.BeginHorizontal();
			
				if (GUILayout.Button("Add Actant", GUILayout.Width(96f)))
				{
					GenericMenu menu = new GenericMenu();

					// SingleActant의 모든 자식 타입을 찾아서 메뉴에 추가합니다.
					foreach (var type in GetDerivedTypes<SingleActant>())
					{
						menu.AddItem(new GUIContent(type.Name), false, () =>
						{
							// 선택한 타입의 Actant를 추가합니다.
							var newActant = (SingleActant)Activator.CreateInstance(type);
							newActant.StartFrame = selectedFrame;
							selectedActionState.Actants.Add(newActant);
							EditorUtility.SetDirty(ASM);
							AssetDatabase.SaveAssets();
							selectedActant = newActant;
							selectedActantIndex = selectedActionState.Actants.Count - 1;
						});
					}

					menu.ShowAsContext();
				}
				
				EditorGUILayout.LabelField($"Total Connected States: {selectedActionState.connectedStateCount}");
				
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();

				GUIStyle horizontalScrollbar = GUI.skin.horizontalScrollbar;
				GUIStyle verticalScrollbar = GUI.skin.verticalScrollbar;
				GUI.skin.horizontalScrollbar = GUIStyle.none;
				GUI.skin.verticalScrollbar = GUIStyle.none;

				var headerBaseWidth = 192f;

				var swapTarget = (0, 0);
				EditorGUILayout.BeginVertical(GUILayout.Width(headerBaseWidth + 2f));
				
					if (GUILayout.Button(">>", GUILayout.Width(headerBaseWidth - 4f)))
					{
						GenericMenu menu = new GenericMenu();

						menu.AddItem(new GUIContent("There's still no function here... Sorry!"), false, () => { });

						menu.ShowAsContext();
					}
					
					EditorGUILayout.BeginScrollView(timelineScroll.GetYFlat(), GUIStyle.none, GUIStyle.none, GUILayout.Width(headerBaseWidth), GUILayout.Height(timelineHeight - 42f));
					for (var index = 0; index < selectedActionState.Actants.Count; index++)
					{
						var act = selectedActionState.Actants[index];
						
						// 이전과 동일한 타입의 Actant이고 시작 프레임이 이전 종료 프레임 이후일 때 같은 줄에 표시합니다.
						if (index > 0)
						{
							var prev = selectedActionState.Actants[index-1];
							if (act.GetType() == prev.GetType() && act.StartFrame >= prev.EndFrame)
								continue;
						}
						
						EditorGUILayout.BeginHorizontal();

						GUI.backgroundColor = Color.red;
						GUIStyle simpleStyle = new GUIStyle(GUI.skin.button)
						{
							alignment = TextAnchor.MiddleCenter,
						};
						if (GUILayout.Button("X", simpleStyle, GUILayout.Width(24f), GUILayout.Height(30f)) && index > 0)
						{
							killList.Add(index);
						}
						GUI.backgroundColor = defaultColor;
						
						if (GUILayout.Button(act.ToString(), GUILayout.Width(headerBaseWidth - 88f), GUILayout.Height(30f)))
						{
							if (selectedFrame < act.StartFrame || selectedFrame > act.DrawnFrame)
								selectedFrame = act.StartFrame;
							selectedActant = act;
							selectedActantIndex = index;
						}
                        
						if (GUILayout.Button("+", simpleStyle, GUILayout.Width(24f), GUILayout.Height(30f)))
						{
							// 선택한 타입의 Actant를 추가합니다.
							var newActant = (SingleActant)Activator.CreateInstance(act.GetType());
							newActant.CopyFrom(act);
							selectedActionState.Actants.Insert(index+1, newActant);
							EditorUtility.SetDirty(ASM);
							AssetDatabase.SaveAssets();
							selectedActant = newActant;
							selectedActantIndex = selectedActionState.Actants.Count - 1;
						}
						
						EditorGUILayout.BeginVertical();
						if (GUILayout.Button("▲", GUILayout.Width(24f), GUILayout.Height(14f)) && index > 0)
						{
							swapTarget = (index, index - 1);
						}

						if (GUILayout.Button("▼", GUILayout.Width(24f), GUILayout.Height(14f)) &&
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
				
				var frameBoxBaseWidth = 30f;
				var totalDuration = selectedActionState.TotalDuration;
	            EditorGUILayout.BeginScrollView(timelineScroll.GetXFlat(), GUIStyle.none, GUIStyle.none, 
		            GUILayout.Width(position.width - (320f + headerBaseWidth + 22f)), GUILayout.Height(20f));
					EditorGUILayout.BeginHorizontal();
						for (int i = 0; i <= totalDuration; i++)
						{
							if (i == selectedFrame)
								GUI.backgroundColor = Color.yellow;
							if (GUILayout.Button(i.ToString(), GUILayout.Width(frameBoxBaseWidth)))
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

	            void fill_space(int end)
	            {
		            for (int i = end + 1; i <= totalDuration; i++)
			            if (GUILayout.Button(" ", GUILayout.Width(frameBoxBaseWidth), GUILayout.Height(30f)))
			            {
				            selectedFrame = i;
				            selectedActant = null;
				            selectedActantIndex = -1;
			            }
	            }
	            
				// 스크롤뷰 시작
				timelineScroll = EditorGUILayout.BeginScrollView(timelineScroll, true, true , 
					GUILayout.Width(position.width - (320f + headerBaseWidth + 8f)), GUILayout.Height(timelineHeight - 30f));

						EditorGUILayout.BeginHorizontal();
			            for (var index = 0; index < selectedActionState.Actants.Count; index++)
			            {
				            var act = selectedActionState.Actants[index];
				            var sFrame = 0;
				            
				            // 이전과 동일한 타입의 Actant이고 시작 프레임이 이전 종료 프레임 이후일 때 같은 줄에 표시합니다.
				            if (index > 0)
				            {
					            var prev = selectedActionState.Actants[index-1];
					            if (act.GetType() == prev.GetType() && act.StartFrame >= prev.EndFrame)
					            {
						            sFrame = prev.EndFrame;
					            }
					            else
					            {
						            fill_space(prev.EndFrame);
						            
						            EditorGUILayout.EndHorizontal();
						            EditorGUILayout.BeginHorizontal();
					            }
				            }
				            
				            for (int i = sFrame; i <= totalDuration; i++)
				            {
					            if (i == act.StartFrame)
					            {
						            GUI.backgroundColor =
							            (act == selectedActant) ? Color.cyan : Color.green;
						            if (GUILayout.Button(act.ToString(), GUILayout.Width((frameBoxBaseWidth + 3f) * Mathf.Max(1, act.Duration) - 3f), GUILayout.Height(30f)))
						            {
							            if (selectedFrame < act.StartFrame || selectedFrame > act.DrawnFrame)
											selectedFrame = i;
							            selectedActant = act;
							            selectedActantIndex = index;
						            }

						            GUI.backgroundColor = DefaultGUIColor;
					            }
					            else if (i < act.StartFrame)
					            {
						            if (GUILayout.Button(" ", GUILayout.Width(frameBoxBaseWidth), GUILayout.Height(30f)))
						            {
							            selectedFrame = i;
							            selectedActant = null;
							            selectedActantIndex = -1;
						            }
					            }
					            else
						            break;
				            }
			            }
			            
			            fill_space(selectedActionState.Actants[^1].EndFrame);
			            EditorGUILayout.EndHorizontal();

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
			
			ASM.CurrentFrame = selectedFrame;
            selectedActionState.OnGUIFrame(gRect, 2f * zoom, selectedFrame);
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
            
			if (IsPlaying)
			{
				if (GUILayout.Button("■", GUILayout.Width(48f)))
				{
					StartTime = 0f;
					InnerTimer = 0f;
					IsPlaying = false;
					selectedFrame = 0;
				}
			}
			else
			{
				if (GUILayout.Button("▶", GUILayout.Width(48f)))
				{
					selectedActant = null;
					selectedFrame = 0;
					selectedActantIndex = -1;
					StartTime = (float)EditorApplication.timeSinceStartup;
					IsPlaying = true;
				}
			}
			
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
				EditorGUILayout.LabelField("  No Actant data selected.");
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
