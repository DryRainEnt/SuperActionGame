using System;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace SimpleActionFramework.Core.Editor
{
	public class FrameDataEditor : EditorWindow
	{
		private FrameDataSet frameDataSet;
		private string searchKey;
		private string selectedKey;
		private FrameData selectedFrameData;
		private Vector2 listScrollPosition;
		
		private bool showHitBoxes = false;
		private bool showDamageBoxes = false;
		private bool showGuardBoxes = false;

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
			DrawEditArea();
			EditorGUILayout.EndVertical();

			// 데이터 영역
			EditorGUILayout.BeginVertical(GUILayout.Width(320f));
			DrawDataArea();
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
			frameDataSet = EditorGUILayout.ObjectField("Frame Data Set", frameDataSet, typeof(FrameDataSet), false) as FrameDataSet;
			
			// 선택한 FrameDataSet이 있는 경우 키 목록 표시
			if (frameDataSet is not null)
			{
				EditorGUILayout.BeginHorizontal();
				
				searchKey = GUILayout.TextField(searchKey);
				if (GUILayout.Button("Add", GUILayout.Width(80f)) && !frameDataSet.FrameData.ContainsKey(searchKey))
				{
					// 여기서 frameDataSet은 수정된 FrameDataSet의 인스턴스입니다.
					frameDataSet.Add(searchKey, new FrameData());
					EditorUtility.SetDirty(frameDataSet);
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
					
					foreach (var key in frameDataSet.FrameData.Keys)
					{
						if (searchKey is not null && searchKey != string.Empty && !key.Contains(searchKey, StringComparison.Ordinal))
							continue;
					
						EditorGUILayout.BeginHorizontal();
					
						if (GUILayout.Button(key))
						{
							selectedKey = key;
							selectedFrameData = frameDataSet.FrameData[key];
						}

						GUI.backgroundColor = Color.red;
						if (GUILayout.Button("Remove", GUILayout.Width(80f)))
						{
							GenericMenu menu = new GenericMenu();

							menu.AddItem(new GUIContent("Are you sure with removing this frame?"), false, () => { });
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
						EditorUtility.SetDirty(frameDataSet);
						frameDataSet.FrameData.Remove(removeKey);
						AssetDatabase.SaveAssets();
					}
				}while (updateExists);

				EditorGUILayout.EndScrollView();
			}
		}
		
		private float zoom = 1f; // 확대/축소 배율
		private Vector2 scrollPosition = Vector2.zero; // 스크롤 위치

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

			if (selectedFrameData.Sprite == null)
			{
				EditorGUILayout.LabelField("No sprite selected.");
				return;
			}
			
			// 스크롤뷰 시작
			scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

			// 격자 그리기
			DrawGrid();

			// 스프라이트 위치와 크기 조정 (PPU 고려)
			float scale = 2f; // PPU에 따른 배율
			
			float xPos = (position.width - selectedFrameData.Sprite.rect.width * zoom * scale) / 2
			             + selectedFrameData.Offset.x * zoom * scale;
			float yPos = (position.height - selectedFrameData.Sprite.rect.height * zoom * scale) / 2
			             + selectedFrameData.Offset.y * zoom * scale;
			
			Rect spriteRect = new Rect(xPos, yPos,
				selectedFrameData.Sprite.rect.width * zoom * scale,
				selectedFrameData.Sprite.rect.height * zoom * scale);


			// 스프라이트의 UV 좌표 계산
			Rect spriteUV = new Rect(selectedFrameData.Sprite.textureRect.x / selectedFrameData.Sprite.texture.width,
				selectedFrameData.Sprite.textureRect.y / selectedFrameData.Sprite.texture.height,
				selectedFrameData.Sprite.textureRect.width / selectedFrameData.Sprite.texture.width,
				selectedFrameData.Sprite.textureRect.height / selectedFrameData.Sprite.texture.height);

			// 스프라이트 표시
			GUI.DrawTextureWithTexCoords(spriteRect, selectedFrameData.Sprite.texture, spriteUV);

			var basePos = new Vector2(
				xPos + selectedFrameData.Sprite.pivot.x * zoom * scale,
				yPos + (selectedFrameData.Sprite.rect.height - selectedFrameData.Sprite.pivot.y) * zoom * scale);
			// 히트박스, 데미지박스, 가드박스 표시
			DrawBounds(basePos, selectedFrameData.HitBoxes, HitBoxColor);
			DrawBounds(basePos, selectedFrameData.DamageBoxes, DamageBoxColor);
			DrawBounds(basePos, selectedFrameData.GuardBoxes, GuardBoxColor);

			// 스크롤뷰 끝
			EditorGUILayout.EndScrollView();
		}

		private void DrawBounds(Vector3 basePos, Bounds[] boundsList, Color color)
		{
			float scale = 2f * selectedFrameData.Sprite.pixelsPerUnit; // PPU에 따른 배율
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

		private void DrawDataArea()
		{
			Handles.color = new Color(0f, 0f, 0f, 1f); // 영역 구분선 설정
			Handles.DrawLine(
				new Vector3(position.width - 319f, 0f, 0), 
				new Vector3(position.width - 319f, position.height, 0)
				);
			Handles.DrawLine(
				new Vector3(position.width - 320f, 0f, 0), 
				new Vector3(position.width - 320f, position.height, 0)
				);

			
			if (selectedFrameData.Sprite is null || frameDataSet is null)
			{
				EditorGUILayout.LabelField("No FrameData selected.");
				return;
			}

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(selectedKey, EditorStyles.boldLabel);
			
			if (GUILayout.Button("Save"))
			{
				// 여기서 frameDataSet은 수정된 FrameDataSet의 인스턴스입니다.
				EditorUtility.SetDirty(frameDataSet);
				frameDataSet.FrameData[selectedKey] = selectedFrameData;
				AssetDatabase.SaveAssets();
			}

			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.Separator();
			
			EditorGUI.indentLevel++;

			dataScrollPosition = EditorGUILayout.BeginScrollView(dataScrollPosition);
			
			EditorGUI.BeginChangeCheck(); // 변경 감지 시작
			var sprite = EditorGUILayout.ObjectField("Sprite", selectedFrameData.Sprite,
				typeof(Sprite), false) as Sprite;
			if (EditorGUI.EndChangeCheck())
			{
				EditorUtility.SetDirty(frameDataSet);
				selectedFrameData.Sprite = sprite;
				frameDataSet.FrameData[selectedKey] = selectedFrameData;
				AssetDatabase.SaveAssets();
			}
			
			EditorGUI.BeginChangeCheck(); // 변경 감지 시작
			var offset = EditorGUILayout.Vector2Field("Offset", selectedFrameData.Offset);
			if (EditorGUI.EndChangeCheck())
			{
				EditorUtility.SetDirty(frameDataSet);
				selectedFrameData.Offset = offset;
				frameDataSet.FrameData[selectedKey] = selectedFrameData;
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
				while(i < selectedFrameData.HitBoxes.Length)
				{
					EditorGUI.BeginChangeCheck(); // 변경 감지 시작
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.BeginVertical();

					EditorGUILayout.LabelField("Element " + i);

					EditorGUI.indentLevel++;
					var center = EditorGUILayout.Vector2Field("Center", selectedFrameData.HitBoxes[i].center);
					var size = EditorGUILayout.Vector2Field("Size", selectedFrameData.HitBoxes[i].size);

					EditorGUILayout.EndVertical();

					GUI.backgroundColor = Color.red;
					// "Remove Last Hit Box" 버튼을 누르면 HitBoxes 배열에서 현재 인덱스를 제거하고 위치를 조정합니다.
					if (GUILayout.Button("X", GUILayout.Width(20f), GUILayout.Height(98f)))
					{
						for (int p = i; p + 1 < selectedFrameData.HitBoxes.Length; p++)
						{
							selectedFrameData.HitBoxes[p] = selectedFrameData.HitBoxes[p + 1];
						}

						Array.Resize(ref selectedFrameData.HitBoxes, selectedFrameData.HitBoxes.Length - 1);

						center = selectedFrameData.HitBoxes[i].center;
						size = selectedFrameData.HitBoxes[i].size;
					}
					GUI.backgroundColor = DefaultGUIColor;

					EditorGUILayout.EndHorizontal();
					EditorGUI.indentLevel--;
					
					if (EditorGUI.EndChangeCheck())
					{
						EditorUtility.SetDirty(frameDataSet);
						selectedFrameData.HitBoxes[i] = new Bounds(center, size);
						AssetDatabase.SaveAssets();
					}
					
					i++;
				}

				// "Add Hit Box" 버튼을 누르면 HitBoxes 배열에 새 항목을 추가합니다.
				if (GUILayout.Button("Add HitBox"))
				{
					EditorGUI.BeginChangeCheck(); // 변경 감지 시작
					Array.Resize(ref selectedFrameData.HitBoxes, selectedFrameData.HitBoxes.Length + 1);
					if (EditorGUI.EndChangeCheck())
					{
						EditorUtility.SetDirty(frameDataSet);
						foreach (var hitBox in selectedFrameData.HitBoxes)
						{
							selectedFrameData.HitBoxes[i] = new Bounds(hitBox.center, hitBox.size);
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
				while(i < selectedFrameData.DamageBoxes.Length)
				{
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.BeginVertical();

					EditorGUILayout.LabelField("Element " + i);

					EditorGUI.indentLevel++;
					var center = EditorGUILayout.Vector2Field("Center", selectedFrameData.DamageBoxes[i].center);
					var size = EditorGUILayout.Vector2Field("Size", selectedFrameData.DamageBoxes[i].size);

					EditorGUILayout.EndVertical();

					GUI.backgroundColor = Color.red;
					// "Remove Last Hit Box" 버튼을 누르면 HitBoxes 배열에서 현재 인덱스를 제거하고 위치를 조정합니다.
					if (GUILayout.Button("X", GUILayout.Width(20f), GUILayout.Height(98f)))
					{
						for (int p = i; p + 1 < selectedFrameData.DamageBoxes.Length; p++)
						{
							selectedFrameData.DamageBoxes[p] = selectedFrameData.DamageBoxes[p + 1];
						}

						Array.Resize(ref selectedFrameData.DamageBoxes, selectedFrameData.DamageBoxes.Length - 1);

						center = selectedFrameData.DamageBoxes[i].center;
						size = selectedFrameData.DamageBoxes[i].size;
					}
					GUI.backgroundColor = DefaultGUIColor;

					EditorGUILayout.EndHorizontal();

					selectedFrameData.DamageBoxes[i] = new Bounds(center, size);

					EditorGUI.indentLevel--;

					i++;
				}

				// "Add Hit Box" 버튼을 누르면 HitBoxes 배열에 새 항목을 추가합니다.
				if (GUILayout.Button("Add DamageBox"))
				{
					Array.Resize(ref selectedFrameData.DamageBoxes, selectedFrameData.DamageBoxes.Length + 1);
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
				while(i < selectedFrameData.GuardBoxes.Length)
				{
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.BeginVertical();

					EditorGUILayout.LabelField("Element " + i);

					EditorGUI.indentLevel++;
					var center = EditorGUILayout.Vector2Field("Center", selectedFrameData.GuardBoxes[i].center);
					var size = EditorGUILayout.Vector2Field("Size", selectedFrameData.GuardBoxes[i].size);

					EditorGUILayout.EndVertical();

					GUI.backgroundColor = Color.red;
					// "Remove Last Hit Box" 버튼을 누르면 HitBoxes 배열에서 현재 인덱스를 제거하고 위치를 조정합니다.
					if (GUILayout.Button("X", GUILayout.Width(20f), GUILayout.Height(98f)))
					{
						for (int p = i; p + 1 < selectedFrameData.GuardBoxes.Length; p++)
						{
							selectedFrameData.GuardBoxes[p] = selectedFrameData.GuardBoxes[p + 1];
						}

						Array.Resize(ref selectedFrameData.GuardBoxes, selectedFrameData.GuardBoxes.Length - 1);

						center = selectedFrameData.GuardBoxes[i].center;
						size = selectedFrameData.GuardBoxes[i].size;
					}
					GUI.backgroundColor = DefaultGUIColor;

					EditorGUILayout.EndHorizontal();

					selectedFrameData.GuardBoxes[i] = new Bounds(center, size);

					EditorGUI.indentLevel--;

					i++;
				}

				// "Add Hit Box" 버튼을 누르면 HitBoxes 배열에 새 항목을 추가합니다.
				if (GUILayout.Button("Add DamageBox"))
				{
					Array.Resize(ref selectedFrameData.GuardBoxes, selectedFrameData.GuardBoxes.Length + 1);
				}

				EditorGUI.indentLevel--;

				EditorGUILayout.EndVertical();
				EditorGUI.EndChangeCheck(); // 변경 감지 종료
			}
			#endregion
			
			
			EditorGUI.indentLevel--;
			
			EditorGUILayout.EndScrollView();
		}
	}
}