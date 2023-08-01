using UnityEngine;
using UnityEditor;
using System.IO;

public class CreateActantScript : Editor {

	[MenuItem("Assets/Create/Create New Actant", false, 1)]
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
				writer.WriteLine($"public class {className} : SingleActant");
				writer.WriteLine("{");
				writer.WriteLine("\tpublic override void Act(float dt)");
				writer.WriteLine("\t{");
				writer.WriteLine("\t \tbase.Act(dt);");
				writer.WriteLine("\t \t// Put your code here");
				writer.WriteLine("\t}");
				writer.WriteLine("}");
			}

			AssetDatabase.Refresh();
		}
	}
}