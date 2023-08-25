using System.IO;
using UnityEditor;

namespace Editor.SimpleActionEditor
{
	public class CreateActantScript : UnityEditor.Editor {

		[MenuItem("Assets/Create/Simple Action Framework/Create New Actant", false, 3)]
		public static void CreateNewActant() {
			string path = AssetDatabase.GetAssetPath(Selection.activeObject);
			string editorPath = "Assets/Editor/SimpleActionEditor/ActantEditor/";
			if(path == "") {
				path = "Assets/SimpleActionFramework/Core/Actants";
			} else if(Path.GetExtension(path) != "") {
				path = path.Replace(Path.GetFileName(path), "");
			}

			string className = "NewActant";
			string assetPath = EditorUtility.SaveFilePanel("Enter Class Name", path, className, "cs");
		
			// Remove paths from the string and leave only the class name
			className = Path.GetFileNameWithoutExtension(assetPath);

			if(!string.IsNullOrEmpty(className)) {
			
				using(StreamWriter writer = new StreamWriter(assetPath)) {
					writer.WriteLine("using UnityEngine;");
					writer.WriteLine("using SimpleActionFramework.Core;");
					writer.WriteLine("");
					writer.WriteLine("[System.Serializable]");
					writer.WriteLine($"public class {className} : SingleActant");
					writer.WriteLine("{");
					writer.WriteLine("\tpublic override void Act(Actor actor, float progress, bool isFirstFrame = false)");
					writer.WriteLine("\t{");
					writer.WriteLine("\t \tbase.Act(actor, progress, isFirstFrame);");
					writer.WriteLine("\t \t// Put your code here");
					writer.WriteLine("\t}");
					writer.WriteLine("}");
				}

				using (StreamWriter writer = new StreamWriter(editorPath + className + "Drawer.cs"))
				{
					writer.WriteLine("using System.Collections.Generic;");
					writer.WriteLine("using SimpleActionFramework.Actant;");
					writer.WriteLine("using UnityEditor;");
					writer.WriteLine("using UnityEngine;");
					writer.WriteLine("");
					writer.WriteLine("namespace Editor.SimpleActionEditor.ActantEditor");
					writer.WriteLine("{");
					writer.WriteLine($"\t[CustomPropertyDrawer(typeof({className}))]");
					writer.WriteLine($"\tpublic class {className}Drawer : PropertyDrawer");
					writer.WriteLine("\t{");
					
					writer.WriteLine("\t \tprivate int _propertyCount;");
					writer.WriteLine("\t \tpublic int PropertyCount");
					writer.WriteLine("\t \t{");
					writer.WriteLine("\t \t \tget => _propertyCount;");
					writer.WriteLine("\t \t \tset => _propertyCount = Mathf.Max(value, _propertyCount);");
					writer.WriteLine("\t \t}");
					writer.WriteLine("\t \t");
					writer.WriteLine("\t \tpublic override float GetPropertyHeight(SerializedProperty property, GUIContent label)");
					writer.WriteLine("\t \t{");
					writer.WriteLine("\t \t    return 24f * PropertyCount;");
					writer.WriteLine("\t \t}");
					
					writer.WriteLine("");
					
					writer.WriteLine("\t \tpublic override void OnGUI(Rect position, SerializedProperty property, GUIContent label)");
					writer.WriteLine("\t \t{");
					
					writer.WriteLine("\t \t \tvar pCount = 0;");
					writer.WriteLine("\t \t \tSerializedProperty startFrameProperty = property.FindPropertyRelative(\"StartFrame\");");
					writer.WriteLine("\t \t \tSerializedProperty durationProperty = property.FindPropertyRelative(\"Duration\");");
					writer.WriteLine("\t \t \tEditorGUI.BeginProperty(position, label, property);");
					writer.WriteLine("\t \t \t");
					writer.WriteLine("\t \t \tvar drawRect = new Rect(position.x, position.y + 24f * pCount, position.width, position.height);");
					writer.WriteLine("\t \t \tEditorGUI.PropertyField(drawRect, startFrameProperty, ");
					writer.WriteLine("\t \t \t    new GUIContent(\"StartFrame\"), true);");
					writer.WriteLine("\t \t \tpCount++;");
					writer.WriteLine("\t \t \t");
					writer.WriteLine("\t \t \tdrawRect = new Rect(position.x, position.y + 24f * pCount, position.width, position.height);");
					writer.WriteLine("\t \t \tEditorGUI.PropertyField(drawRect, durationProperty, ");
					writer.WriteLine("\t \t \t    new GUIContent(\"Duration\"), true);");
					writer.WriteLine("\t \t \tpCount++;");
					writer.WriteLine("\t \t \t");
					writer.WriteLine("\t \t \t// Put your code here");
					writer.WriteLine("\t \t \t");
					writer.WriteLine("\t \t \tPropertyCount = pCount;");
					writer.WriteLine("\t \t \t");
					writer.WriteLine("\t \t \tEditorGUI.EndProperty();");
					
					
					writer.WriteLine("\t \t}");
					writer.WriteLine("\t}");
					writer.WriteLine("}");
				}

				AssetDatabase.Refresh();
			}
		}
	}
}