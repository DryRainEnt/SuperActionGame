using Proto.CustomDebugTool;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class DebugDrawer : MonoBehaviour
{
	private void OnPostRender()
	{
		CustomDebugger.DrawGizmos();
	}
}
