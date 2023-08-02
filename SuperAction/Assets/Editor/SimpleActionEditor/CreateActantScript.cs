using System.IO;
using UnityEditor;

namespace Editor.SimpleActionEditor
{
	public class CreateActantScript : UnityEditor.Editor {

		[MenuItem("Assets/Create/Simple Action Framework/Create New Actant", false, 3)]
		public static void CreateNewActant() {
			string path = AssetDatabase.GetAssetPath(Selection.activeObject);
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
					writer.WriteLine("\tpublic override void Act(ActionStateMachine machine, float progress, bool isFirstFrame = false)");
					writer.WriteLine("\t{");
					writer.WriteLine("\t \tbase.Act(machine, progress, isFirstFrame);");
					writer.WriteLine("\t \t// Put your code here");
					writer.WriteLine("\t}");
					writer.WriteLine("}");
				}

				AssetDatabase.Refresh();
			}
		}
	}
}