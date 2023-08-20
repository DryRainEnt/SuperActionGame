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
		
		private bool showHitBoxes = false;
		private bool showDamageBoxes = false;
		private bool showGuardBoxes = false;
		
		private int selectedFrame = 0;

		private Color DefaultGUIColor;
		
		private readonly Color HitBoxColor = new Color(0f, 1f, 0f, 0.5f);
		private readonly Color DamageBoxColor = new Color(1f, 0f, 0f, 0.5f);
		private readonly Color GuardBoxColor = new Color(0f, 0f, 1f, 0.5f);
		
		private Vector2 dataScrollPosition;

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
		private float timelineHeight = 240f; // 타임라인 영역의 높이

		// T를 상속받는 모든 타입을 찾는 메서드입니다.
		private static IEnumerable<Type> GetDerivedTypes<T>()
		{
			return Assembly.GetAssembly(typeof(T)).GetTypes().Where(t => t.IsSubclassOf(typeof(T)) && !t.IsAbstract);
		}
		
		private void DrawTimelineArea()
		{
			Handles.color = new Color(0f, 0f, 0f, 1f); // 영역 구분선 설정
			Handles.DrawLine(
				new Vector3(321f, timelineHeight, 0), 
				new Vector3(position.width - 321f, timelineHeight, 0)
			);
			Handles.DrawLine(
				new Vector3(321f, timelineHeight + 1f, 0), 
				new Vector3(position.width - 321f, timelineHeight + 1f, 0)
			);
			
			// 스크롤뷰 시작
			timelineScroll = EditorGUILayout.BeginScrollView(timelineScroll, GUILayout.Height(timelineHeight));
			
			if (ASM is null || selectedActionState is null)
			{
				EditorGUILayout.LabelField("No action state selected");
				EditorGUILayout.EndScrollView();
				return;
			}
            
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

			var totalDuration = selectedActionState.TotalDuration;
			
			EditorGUILayout.BeginVertical();
				EditorGUILayout.BeginHorizontal();
				for (int i = 0; i < totalDuration; i++)
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
				            if (GUILayout.Button(i.ToString(), GUILayout.Width(33f * Mathf.Max(1, act.Duration) - 3f), GUILayout.Height(30f)))
				            {
					            selectedFrame = i;
					            selectedActant = act;
					            selectedActantIndex = index;
				            }

				            GUI.backgroundColor = DefaultGUIColor;
			            }
			            else if (i < act.StartFrame || i > act.DrawnFrame)
			            {
				            GUILayout.Button("X", GUILayout.Width(30f), GUILayout.Height(30f));
			            }
		            }

		            EditorGUILayout.EndHorizontal();
	            }

	            EditorGUILayout.EndVertical();
			
			EditorGUILayout.EndScrollView();
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
			if (Event.current.type == EventType.MouseDrag && Event.current.button == 0) // 왼쪽 클릭으로 드래그
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
			DrawGrid();
			
/*
			// 스프라이트 위치와 크기 조정 (PPU 고려)
			float scale = 2f; // PPU에 따른 배율
			
			float xPos = (position.width - selectedActionState.Sprite.rect.width * zoom * scale) / 2
			             + selectedActionState.Offset.x * zoom * scale;
			float yPos = (position.height - selectedActionState.Sprite.rect.height * zoom * scale) / 2
			             + selectedActionState.Offset.y * zoom * scale;
			
			Rect spriteRect = new Rect(xPos, yPos,
				selectedActionState.Sprite.rect.width * zoom * scale,
				selectedActionState.Sprite.rect.height * zoom * scale);


			// 스프라이트의 UV 좌표 계산
			Rect spriteUV = new Rect(selectedActionState.Sprite.textureRect.x / selectedActionState.Sprite.texture.width,
				selectedActionState.Sprite.textureRect.y / selectedActionState.Sprite.texture.height,
				selectedActionState.Sprite.textureRect.width / selectedActionState.Sprite.texture.width,
				selectedActionState.Sprite.textureRect.height / selectedActionState.Sprite.texture.height);

			// 스프라이트 표시
			GUI.DrawTextureWithTexCoords(spriteRect, selectedActionState.Sprite.texture, spriteUV);

			var basePos = new Vector2(
				xPos + selectedActionState.Sprite.pivot.x * zoom * scale,
				yPos + (selectedActionState.Sprite.rect.height - selectedActionState.Sprite.pivot.y) * zoom * scale);
			// 히트박스, 데미지박스, 가드박스 표시
			DrawBounds(basePos, selectedActionState.HitBoxes, HitBoxColor);
			DrawBounds(basePos, selectedActionState.DamageBoxes, DamageBoxColor);
			DrawBounds(basePos, selectedActionState.GuardBoxes, GuardBoxColor);
*/

			// 스크롤뷰 끝
			EditorGUILayout.EndScrollView();
		}

		private void DrawBounds(Vector3 basePos, Bounds[] boundsList, Color color)
		{
			// float scale = 2f * selectedActionState.Sprite.pixelsPerUnit; // PPU에 따른 배율
			float scale = 2f * 16; // PPU에 따른 배율
			GUI.color = color;
			foreach (Bounds bounds in boundsList)
			{
				GUI.DrawTexture(
					new Rect(basePos + (bounds.min - bounds.center.y * 2f * Vector3.up) * (zoom * scale),
					bounds.size * (zoom * scale)), EditorGUIUtility.whiteTexture, ScaleMode.StretchToFill);
			}
			GUI.color = Color.white; // GUI 색상을 기본값으로 돌려놓기
		}

		private void DrawGrid()
		{
			//TODO: 대상 Offset 중심으로 그려야 함
			float gridSize = 32 * zoom; // 격자의 크기 설정
			int gridCountX = Mathf.CeilToInt(position.width / gridSize);
			int gridCountY = Mathf.CeilToInt(position.height / gridSize);

			EditorGUILayout.LabelField($"Zoom Level : {zoom * 100 : #00.00}%");
			
			Handles.color = new Color(0.8f, 0.8f, 0.8f, 0.4f); // 격자 색상 설정
			for (int x = 0; x < gridCountX; x++)
			{
				for (int y = 0; y < gridCountY; y++)
				{
					Handles.DrawLine(new Vector3(x * gridSize, y * gridSize, 0), new Vector3(x * gridSize, (y + 1) * gridSize, 0));
					Handles.DrawLine(new Vector3(x * gridSize, y * gridSize, 0), new Vector3((x + 1) * gridSize, y * gridSize, 0));
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
				new Vector3(position.width - 319f, timelineHeight, 0), 
				new Vector3(position.width - 319f, position.height, 0)
			);
			Handles.DrawLine(
				new Vector3(position.width - 320f, timelineHeight, 0), 
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


			/*
			
			EditorGUI.BeginChangeCheck(); // 변경 감지 시작
			var sprite = EditorGUILayout.ObjectField("Sprite", selectedActionState.Sprite,
				typeof(Sprite), false) as Sprite;
			if (EditorGUI.EndChangeCheck())
			{
				EditorUtility.SetDirty(frameDataSet);
				selectedActionState.Sprite = sprite;
				frameDataSet.FrameData[selectedKey] = selectedActionState;
				AssetDatabase.SaveAssets();
			}
			
			EditorGUI.BeginChangeCheck(); // 변경 감지 시작
			var offset = EditorGUILayout.Vector2Field("Offset", selectedActionState.Offset);
			if (EditorGUI.EndChangeCheck())
			{
				EditorUtility.SetDirty(frameDataSet);
				selectedActionState.Offset = offset;
				frameDataSet.FrameData[selectedKey] = selectedActionState;
				AssetDatabase.SaveAssets();
			}


			EditorGUILayout.Separator();

			#region HitBox

			showHitBoxes = EditorGUILayout.Foldout(showHitBoxes, "Hit Boxes");

			if (showHitBoxes)
			{
				EditorGUI.indentLevel++;
				// 배열의 크기를 수정할 수 있는 필드를 만듭니다.

				EditorGUILayout.BeginVertical();

				int i = 0;
				while(i < selectedActionState.HitBoxes.Length)
				{
					EditorGUI.BeginChangeCheck(); // 변경 감지 시작
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.BeginVertical();

					EditorGUILayout.LabelField("Element " + i);

					EditorGUI.indentLevel++;
					var center = EditorGUILayout.Vector2Field("Center", selectedActionState.HitBoxes[i].center);
					var size = EditorGUILayout.Vector2Field("Size", selectedActionState.HitBoxes[i].size);

					EditorGUILayout.EndVertical();

					GUI.backgroundColor = Color.red;
					// "Remove Last Hit Box" 버튼을 누르면 HitBoxes 배열에서 현재 인덱스를 제거하고 위치를 조정합니다.
					if (GUILayout.Button("X", GUILayout.Width(20f), GUILayout.Height(98f)))
					{
						for (int p = i; p + 1 < selectedActionState.HitBoxes.Length; p++)
						{
							selectedActionState.HitBoxes[p] = selectedActionState.HitBoxes[p + 1];
						}

						Array.Resize(ref selectedActionState.HitBoxes, selectedActionState.HitBoxes.Length - 1);

						center = selectedActionState.HitBoxes[i].center;
						size = selectedActionState.HitBoxes[i].size;
					}
					GUI.backgroundColor = DefaultGUIColor;

					EditorGUILayout.EndHorizontal();
					EditorGUI.indentLevel--;
					
					if (EditorGUI.EndChangeCheck())
					{
						EditorUtility.SetDirty(frameDataSet);
						selectedActionState.HitBoxes[i] = new Bounds(center, size);
						AssetDatabase.SaveAssets();
					}
					
					i++;
				}

				// "Add Hit Box" 버튼을 누르면 HitBoxes 배열에 새 항목을 추가합니다.
				if (GUILayout.Button("Add HitBox"))
				{
					EditorGUI.BeginChangeCheck(); // 변경 감지 시작
					Array.Resize(ref selectedActionState.HitBoxes, selectedActionState.HitBoxes.Length + 1);
					if (EditorGUI.EndChangeCheck())
					{
						EditorUtility.SetDirty(frameDataSet);
						foreach (var hitBox in selectedActionState.HitBoxes)
						{
							selectedActionState.HitBoxes[i] = new Bounds(hitBox.center, hitBox.size);
						}
						AssetDatabase.SaveAssets();
					}
				}

				EditorGUI.indentLevel--;

				EditorGUILayout.EndVertical();
			}

			#endregion
			
			EditorGUILayout.Separator();

			#region DamageBox
            
			showDamageBoxes = EditorGUILayout.Foldout(showDamageBoxes, "Damage Boxes");

			if (showDamageBoxes)
			{
				EditorGUI.indentLevel++;
				// 배열의 크기를 수정할 수 있는 필드를 만듭니다.
				EditorGUI.BeginChangeCheck(); // 변경 감지 시작

				// 배열의 각 항목을 수정할 수 있는 필드를 만듭니다.
				EditorGUI.BeginChangeCheck(); // 변경 감지 시작
				EditorGUILayout.BeginVertical();

				int i = 0;
				while(i < selectedActionState.DamageBoxes.Length)
				{
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.BeginVertical();

					EditorGUILayout.LabelField("Element " + i);

					EditorGUI.indentLevel++;
					var center = EditorGUILayout.Vector2Field("Center", selectedActionState.DamageBoxes[i].center);
					var size = EditorGUILayout.Vector2Field("Size", selectedActionState.DamageBoxes[i].size);

					EditorGUILayout.EndVertical();

					GUI.backgroundColor = Color.red;
					// "Remove Last Hit Box" 버튼을 누르면 HitBoxes 배열에서 현재 인덱스를 제거하고 위치를 조정합니다.
					if (GUILayout.Button("X", GUILayout.Width(20f), GUILayout.Height(98f)))
					{
						for (int p = i; p + 1 < selectedActionState.DamageBoxes.Length; p++)
						{
							selectedActionState.DamageBoxes[p] = selectedActionState.DamageBoxes[p + 1];
						}

						Array.Resize(ref selectedActionState.DamageBoxes, selectedActionState.DamageBoxes.Length - 1);

						center = selectedActionState.DamageBoxes[i].center;
						size = selectedActionState.DamageBoxes[i].size;
					}
					GUI.backgroundColor = DefaultGUIColor;

					EditorGUILayout.EndHorizontal();

					selectedActionState.DamageBoxes[i] = new Bounds(center, size);

					EditorGUI.indentLevel--;

					i++;
				}

				// "Add Hit Box" 버튼을 누르면 HitBoxes 배열에 새 항목을 추가합니다.
				if (GUILayout.Button("Add DamageBox"))
				{
					Array.Resize(ref selectedActionState.DamageBoxes, selectedActionState.DamageBoxes.Length + 1);
				}

				EditorGUI.indentLevel--;

				EditorGUILayout.EndVertical();
				EditorGUI.EndChangeCheck(); // 변경 감지 종료
			}
			#endregion
			
			EditorGUILayout.Separator();
			
			#region GuardBox
            
			showGuardBoxes = EditorGUILayout.Foldout(showGuardBoxes, "Guard Boxes");

			if (showGuardBoxes)
			{
				EditorGUI.indentLevel++;
				// 배열의 크기를 수정할 수 있는 필드를 만듭니다.
				EditorGUI.BeginChangeCheck(); // 변경 감지 시작

				// 배열의 각 항목을 수정할 수 있는 필드를 만듭니다.
				EditorGUI.BeginChangeCheck(); // 변경 감지 시작
				EditorGUILayout.BeginVertical();

				int i = 0;
				while(i < selectedActionState.GuardBoxes.Length)
				{
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.BeginVertical();

					EditorGUILayout.LabelField("Element " + i);

					EditorGUI.indentLevel++;
					var center = EditorGUILayout.Vector2Field("Center", selectedActionState.GuardBoxes[i].center);
					var size = EditorGUILayout.Vector2Field("Size", selectedActionState.GuardBoxes[i].size);

					EditorGUILayout.EndVertical();

					GUI.backgroundColor = Color.red;
					// "Remove Last Hit Box" 버튼을 누르면 HitBoxes 배열에서 현재 인덱스를 제거하고 위치를 조정합니다.
					if (GUILayout.Button("X", GUILayout.Width(20f), GUILayout.Height(98f)))
					{
						for (int p = i; p + 1 < selectedActionState.GuardBoxes.Length; p++)
						{
							selectedActionState.GuardBoxes[p] = selectedActionState.GuardBoxes[p + 1];
						}

						Array.Resize(ref selectedActionState.GuardBoxes, selectedActionState.GuardBoxes.Length - 1);

						center = selectedActionState.GuardBoxes[i].center;
						size = selectedActionState.GuardBoxes[i].size;
					}
					GUI.backgroundColor = DefaultGUIColor;

					EditorGUILayout.EndHorizontal();

					selectedActionState.GuardBoxes[i] = new Bounds(center, size);

					EditorGUI.indentLevel--;

					i++;
				}

				// "Add Hit Box" 버튼을 누르면 HitBoxes 배열에 새 항목을 추가합니다.
				if (GUILayout.Button("Add DamageBox"))
				{
					Array.Resize(ref selectedActionState.GuardBoxes, selectedActionState.GuardBoxes.Length + 1);
				}

				EditorGUI.indentLevel--;

				EditorGUILayout.EndVertical();
				EditorGUI.EndChangeCheck(); // 변경 감지 종료
			}
			#endregion
			
			
			EditorGUI.indentLevel--;
			
			*/